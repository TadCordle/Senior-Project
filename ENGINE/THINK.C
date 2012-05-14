
//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
//
//	Gerbil
//
//	Copyright (c) 2001, Bruce Moreland.  All rights reserved.
//
//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
//
//	This file is part of the Gerbil chess program project.
//
//	Gerbil is free software; you can redistribute it and/or modify it under
//	the terms of the GNU General Public License as published by the Free
//	Software Foundation; either version 2 of the License, or (at your option)
//	any later version.
//
//	Gerbil is distributed in the hope that it will be useful, but WITHOUT ANY
//	WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//	FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//	details.
//
//	You should have received a copy of the GNU General Public License along
//	with Gerbil; if not, write to the Free Software Foundation, Inc.,
//	59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

#include "gproto.h"
#include "engine.h"
#include <stdlib.h>

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

BOOL FAcceptDraw(PCON pcon, PSTE pste)
{
	//	WARNING!  This code is called right after the move is made on the
	//	internal board, so "us" is really "them" and vice versa, except with
	//	regard to "ss.tmUs" and "ss.tmThem", which are not switched.
	//
	//	First case is I have a bare king.  That can't be any better than a
	//	draw for me, so I will accept it.
	//
	if (pste->valPcThem + pste->valPnThem == 0)		// We have nothing.
		return fTRUE;
	//
	//	Neither side has pawns.
	//
	if ((pste->valPnUs == 0) && (pste->valPnThem == 0)) {
		//
		//	If I'm ahead on the score significantly, forget about it, or if
		//	they have less than 30 seconds left.
		//
		if ((pcon->ss.val >= valPAWN) || (pcon->ss.tmThem < 30000))
			return fFALSE;
		//
		//	Even material with a queen or less is a draw.
		//
		if ((pste->valPcThem == pste->valPcUs) &&
			(pste->valPcThem <= valQUEEN))
			return fTRUE;
		//
		//	I'll accept the case where I have a minor and they have a rook.
		//
		if ((pste->valPcThem == valMINOR) && (pste->valPcUs == valROOK))
			return fTRUE;
		//
		//	I'll accept the case where I have a rook and they have a minor and
		//	a rook.
		//
		if ((pste->valPcThem == valROOK) &&
			(pste->valPcUs == valMINOR + valROOK))
			return fTRUE;
	}
	return fFALSE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

BOOL FCheckResign(PCON pcon, PSTE pste)
{
	int	valDeficit;
	char	aszResult[64];
	char	aszReason[64];
				
	//	WARNING!  This code is called right after the move is made on the
	//	internal board, so "us" is really "them" and vice versa, except with
	//	regard to "ss.tmUs" and "ss.tmThem", which are not switched.
	//
	//	If I'm better than -12, this might conceivably be a draw.  An obvious
	//	case is KQ vs KP, where the P is a supported c- or f-pawn on the 7th.
	//
	//	If it really is won they can kick me below -12 shortly, I'm sure.
	//
	if ((pcon->ss.val >= -1200) || (pcon->ss.tmThem < 30000))
		return fFALSE;
	//
	//	This one is a weird one.  If they haven't captured anything or moved
	//	a pawn for a while, I'm not going to give up.  I added this after a
	//	very strange game where the opponent had a mate in 4 with about five
	//	seconds left in an increment game.  The opponent would not mate.
	//	Every move was a new mate in 4.  But it was moving very quickly, so
	//	eventually it got more than 30 seconds on its clock and Gerbil
	//	resigned.  This will prevent that stupid case, as well as cases where
	//	Gerbil sees a mate in a KQ vs KN (which is suprisingly hard for
	//	beginners), but the opponent isn't making progress.
	//
	if (pste->plyFifty > 10)
		return fFALSE;
	//
	//	Okay, we're at worse than -12 from search, and they have some time
	//	left and should be able to win.
	//
	//	If the board still looks alright (not too far down on material), I'm
	//	going to make them work to beat the engine.  I'm going to insist that
	//	I be down at least a rook on the board.  This prevents the program
	//	resigning while the opponent is scratching their head trying to find
	//	the mate in five after having sacrificed a rook.  I'll make them go
	//	until mate or until they've taken something significant off the board.
	//
	valDeficit = pste->valPcThem +			// Since the colors have been
		pste->valPnThem - pste->valPcUs -	//  swapped, this produces a
		pste->valPnUs;						//  negative value if we are
											//  behind on the board.
	if (valDeficit > -valROOK)				// We have to be down at least
		return fFALSE;						//  a rook.
	//
	//	"pste->coUs" won.  Tell the interface, which will end the game.
	//
	sprintf(aszResult, "%d-%d", pste->coUs ^ 1, pste->coUs);
	sprintf(aszReason, "%s resigns", s_argszCo[pste->coUs ^ 1]);
	VPrSendResult(aszResult, aszReason);
	return fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	The "Think" function is an important part of the engine.  The engine
//	thread spends most of its time in here.

//	The basic idea is that it will sit in a loop and call the search function
//	over and over, each time with one more ply of depth, until the search
//	runs out of time.  This is called "iterative deepening".  One advantage of
//	this is is that this is easily interruptable, since the first few plies
//	take so little time that you'll always have *some* move to make, even if
//	you interrupt after a few milliseconds.  

//	The most important reason to do iterative deepening is that it's faster
//	than just calling the search with some larger final depth.  The reason is
//	that the PV from the previous iteration is passed along to the subsequent
//	iteration.  The moves in this PV are searched first.  The previous PV is
//	probably an excellent line, and alpha-beta is much more efficient if you
//	search a good line first, so you end up searching a smaller tree.

//	After the search has timed out, it will pass the best move to the
//	interface.  If the PV was two moves or more long, it will then go into
//	"ponder" mode, which means that it will execute the second move in the PV
//	on its internal board, and go into an infinite duration think about this
//	move.  This is called "pondering".

//	The the opponent makes the move the program is guessing that they'll make,
//	the engine will seamlessly switch from "ponder" mode to "thinking" mode,
//	as if the opponent really did make their move immediately after the
//	engine's last move.

//	It's possible that the opponent may have been thinking a long time on
//	their move, and by the time they are done, the engine will have already
//	used up its normal time allocation for this move.  If that's the case,
//	the engine will move instantly.

//	If the opponent doesn't make the move the engine is guessing, the search
//	will be aborted and a new search started.

//	Aborting incorrect pondering, and switching from ponder into think mode if
//	the engine guesses the right move, aren't a part of this function, but it
//	makes some sense to explain this here.

#define	plyMAX	120	// Maximum number of iterations.  This seems huge, but it
					//  is sometimes reached.

void VThink(PCON pcon, TM tmUs, TM tmThem)
{
	Assert(pcon->smode == smodeIDLE);
	if (pcon->fLowPriority)
		VLowPriority();
	//
	//	Set up time-out, and remind myself that I'm not dead.
	//
	pcon->ss.tmUs = tmUs;
	pcon->ss.tmThem = tmThem;
	pcon->fAbort = fFALSE;
	pcon->smode = (tmUs == tmANALYZE) ? smodeANALYZE : smodeTHINK;
	//
	//	This loop is going to execute at least once.  It might execute for the
	//	whole game, if the program continuously ponders correctly.
	//
	for (;;) {
		char	aszMove[32];
		char	aszPonder[32];
		char	aszSanHint[32];
		PSTE	pste = &pcon->argste[0];
		int	i;
		SAN	sanHint;
		CM	cmHint;
		CM	cm;
		int	valCur;						// Value returned by search.
		int	valLow;						// Alpha-beta search window.
		int	valHigh;
		BOOL	f;


		pcon->fTimeout = fFALSE;		// I'm not timed out yet.
		pcon->ss.tmStart = TmNow();		// Record when I started.
		if (pcon->smode ==				// If I'm pondering, I don't set the
			smodeTHINK)					//  time control stuff.  This will be
			VSetTime(pcon);				//  handled at the point it's clear
										//  that I picked the correct move.
		pste->ccmPv = 0;				// Clear PV.
		VSetRepHash(pcon);				// Pump game history into rep hash.
		if (pcon->smode == smodeTHINK)
			VDrawCheck(pcon);			// Try to claim appropriate 50-move
										//  and 3x rep draws before examining
										//  moves.
		//
		//	During the first 20 moves of the game I will try to find a book
		//	move.  If I find one I will play it.
		//
		if ((pcon->smode == smodeTHINK) && (pcon->fUseBook) &&
			(pcon->gc.ccm < 40) && (FBookMove(pcon, &cm))) {
			char	asz[16];
			BOOL	f;

			VCmToSz(&cm, asz);	// Make book move.
			f = FAdvance(pcon, asz);
			Assert(f);
			VPrSendMove(asz);	// I am not going to bother checking for draws
								//  and mates that occur while in book.  If by
								//  weird chance one of these happens, if
								//  we're hooked up to the chess server, the
								//  server might call it.
			break;
		}
		//	Increment sequence number.  The transposition hash system will
		//	overwrite nodes from old searches, and this is how it knows that
		//	this is a new search, rather than just more of the old one.
		//
		VIncrementHashSeq();
		//
		//	Search the position via iterative deepening.
		//
		pcon->ss.nodes = 0;			// Global node counter.
		pcon->ss.nodesNext = 2000;	// Time (in nodes) of next callback to
									//  interface.
		valCur = 0;
		valLow = valMIN;			// Initial window is wide open.
		valHigh = valMAX;
		for (i = 0; i < plyMAX; i++)  {
			pste->valAlpha = valLow;
			pste->valBeta = valHigh;
			pste->plyRem = i;
			pcon->ss.plyDepth = i + 1;
			VSetPv(pcon);	// This remembers the current PV so that the
							//  search can search its elements first.
			valCur = ValSearch(pcon, pste);
			if (pcon->fAbort)
				break;
			if (pcon->fTimeout)
				break;
			//
			//	This checks for a result outside the window, and if it finds
			//	one it re-searches with the window wide open.
			//
			if ((valCur <= valLow) || (valCur >= valHigh)) {
				if (valCur <= valLow)	// Record a fail-low (fail-high is
										//  handled inside "ValSearch").
					//
					//	This function needs the root moves generated, and the
					//	are, because "ValSearch" does it.
					//
					VDumpPv(pcon, pcon->ss.plyDepth, valCur, prsaFAIL_LOW);
				pste->valAlpha = valMIN;
				pste->valBeta = valMAX;
				VSetPv(pcon);
				valCur = ValSearch(pcon, pste);
				if (pcon->fAbort)
					break;
				if (pcon->fTimeout)
					break;
			}
			pcon->ss.prsa = prsaNORMAL;
			pcon->ss.val = valCur;
			//
			//	Mated now or this move mates, drop out of here.
			//
			if ((pcon->ss.val == valMATE - 1) || (pcon->ss.val == -valMATE))
				break;
			//
			//	Depth-restricted search.  Check for "timeout", meaning target
			//	depth met.
			//
			if (((pcon->smode == smodeTHINK) ||
				(pcon->smode == smodePONDER)) && (pcon->tc.plyDepth > 0) &&
				(pcon->ss.plyDepth >= pcon->tc.plyDepth))
				break;
			//
			//	Set up for next iteration, which will use a window centered
			//	around the current position value.
			//
			valLow = valCur - 50;
			valHigh = valCur + 50;
		}
		//	If abort or doing pondering, I'm done, because I don't need to
		//	make any moves or set up the next search.
		//
		//	NOTE: None of the stuff below happens in analysis mode, because
		//	the next line exits the routine!  This means that results won't
		//	be posted (draws, mates, etc.) if the game is in analysis mode,
		//	which seems to be the right thing to do according to Tim Mann,
		//	circa 26-Apr-2001 on the Yahoo chess engines mailing list.
		//
		if ((pcon->fAbort) || (pcon->smode != smodeTHINK))
			break;
		//
		//	If there is a move in the first element of the PV, I can make it.
		//	I don't make it now, because I want to look at the second element
		//	of the PV for a pondering move, and if I execute the first move,
		//	it will destroy the PV.
		//
		aszMove[0] = aszPonder[0] = '\0';
		if (pste->ccmPv >= 1)
			VCmToSz(&pste->argcmPv[0], aszMove);
		if ((pste->ccmPv >= 2) && (pcon->fPonder)) {
			VCmToSz(&pste->argcmPv[1], aszPonder);
			cmHint = pste->argcmPv[1];
		}
		//	As of this point, I have move to make in "aszMove", move to ponder
		//	in "aszPonder".  If I can't move, or I can't ponder, these strings
		//	are nulled out.
		//
		//	Check to see if I'm mated or stalemated, which will be the case
		//	if I can't move.
		//
		if (aszMove[0] == '\0') {
#ifdef	LOSERS
			if ((!pcon->fTimeout) && (pcon->ss.val == valMATE)) {
#else
			if ((!pcon->fTimeout) && (pcon->ss.val == -valMATE)) {
#endif
				VMates(pste->coUs ^ 1);
				break;
			}
			VStalemate(pste->coUs);
			break;
		}
		f = FAdvance(pcon, aszMove);
		Assert(f);
#if !defined(LOSERS)
		//
		//	Deal with draw offers right before the move is sent to the
		//	interface.  This boolean is set when the opponent offers a draw.
		//	It is cleared when the interface tells us to set up a new
		//	position, and when the engine makes a move.
		//
		//	If this boolean is not cleared at the appropriate time, it's not
		//	critically bad, because all that will happen is that the program
		//	will offer an unsolicited draw in a position it thinks is drawn.
		//
		//	The engine will execute its planned move after trying to agree to
		//	the draw, in case something goes wrong with the draw offer.
		//
		//	If the interface can't handle this, there could be problems.
		//
		//	This code is before the resignation code in order to catch the
		//	one-in-a-million case where the opponent offers a draw before the
		//	engine can resign.
		//
		if (pcon->fDrawOffered) {
			if (FAcceptDraw(pcon, pste))
				VPrOfferDraw();
			pcon->fDrawOffered = fFALSE;
		}
		//	Check to see if I should resign, and do so if necessary.  The move
		//	the engine is planning to make is not executed in this case, in
		//	order to avoid the unattractive <move made> <resigns> sequence.
		//
		//	If the interface doesn't listen to the resignation, the program
		//	simply won't move.
		//
		else if (FCheckResign(pcon, pste))
			break;
#endif
		//	Inform the interface that we have a move.
		//
		VPrSendMove(aszMove);
		//
		//	The score from the last search might indicate that this is mate.
		//
#ifdef	LOSERS
		if ((!pcon->fTimeout) && (valCur == -valMATE + 1)) {
#else
		if ((!pcon->fTimeout) && (valCur == valMATE - 1)) {
#endif
			VMates(pste->coUs ^ 1);	// This is backwards because we already
			break;					//  moved on the internal board.
		}
		//	Check for 50-move or 3x repetition draws.
		//
		VDrawCheck(pcon);
		//
		//	Check to see if I stalemated my opponent, and report the result as
		//	appropriate.
		//
		if (FStalemated(pcon))
			VStalemate(pste->coUs ^ 1);
		//
		//	If the PV contained at least two moves, and the "fPonder" boolean
		//	is set, the second one is sitting in "aszPonder".
		//
		//	If there is no move, we're done, and we're going to drop out of
		//	the bottom of this routine and leave.
		//
		//	If there was a move, I'm going to set our state to "smodePONDER",
		//	remember the move that we are pondering on, execute the move on
		//	the board, and loop around to the top of this think loop and start
		//	thinking again.
		//
		//	Later, if the the interface passes in the move that we are
		//	pondering on, the state info will be dummied up so the program
		//	thinks it's doing a normal search.  If the wrong move is passed in
		//	(the opponent made a move we didn't expect), we'll abort the
		//	search, undo the last move, exit this routine, and be called again
		//	and told to think.
		//
		//	So we get into this routine when we're told to think about
		//	something, and we stay in here until we fail to predict the
		//	opponent's move.
		//
		if (aszPonder[0] == '\0')
			break;
		//
		//	Get the algebraic so I can send a hint move.  The Winboard
		//	interface will stick this at the beginning of the PV it displays
		//	while we are pondering.
		//
		VGenMoves(pcon,				// Pseudo-legal is okay, because the SAN
			pcon->argste, fFALSE);	//  routines can handle it.
		VCmToSan(pcon, pcon->argste, &cmHint, &sanHint);
		VSanToSz(&sanHint, aszSanHint);
		strcpy(pcon->aszPonder, aszPonder);
		//
		//	Execute the move.  I'm now one move ahead of the game, which is
		//	tricky.  After that, send the hint move.
		//
		f = FAdvance(pcon, aszPonder);
		Assert(f);
		if (pcon->fPost)
			VPrSendHint(aszSanHint);
		pcon->smode = smodePONDER;
	}
	//	If I broke out, and I'm in ponder mode, back up the search and pretend
	//	that I never pondered.  I could do some sneaky stuff having to do with
	//	remembering the move that would have been made if the pondered search
	//	had been converted into a normal search soon enough, but this would be
	//	annoying to do and it's kind of rare and pointless.
	//
	if (pcon->smode == smodePONDER)
		VUndoMove(pcon);
	pcon->smode = smodeIDLE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
