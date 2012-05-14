
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

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

#ifdef	NULL_MOVE

//	This does a null-move search to see if the side not to move has a threat.
//	If they can't drive the score above alpha, even if they get to move twice
//	in a row, their positions probably sucks, and we'll prune this variation.

//	It's not called in endgames, because it has problems detecting zugzwang.

BOOL FThreat(PCON pcon, PSTE pste)
{
	int	val;

	if ((pste->plyRem <= 0) || (pste->fNull))
		return fTRUE;
	VMakeNullMove(pcon, pste);
	(pste + 1)->valAlpha = -pste->valBeta;
	(pste + 1)->valBeta = -pste->valBeta + 1;
	if ((pste + 1)->plyRem < 0)
		val = -ValSearchQ(pcon, pste + 1);
	else
		val = -ValSearch(pcon, pste + 1);
	if (val >= pste->valBeta)
		return fFALSE;
	return fTRUE;
}

#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Quiescent search.  This lets captures play out at the end of the search,
//	so I don't end up aborting a line at a stupid place.

//	The quiescent search doesn't see checks, so it will not detect checkmates,
//	and might actually end up making decisions based upon an illegal line.

//	This is one reason I don't record the PV while in quiescent search -- I
//	don't want people telling me the program is doing illegal stuff.

int ValSearchQ(PCON pcon, PSTE pste)
{
	PCM	pcm;
	int	valQuies;

	pcon->ss.nodes++;	// Increment global node counter.
	pste->ccmPv = 0;
	//
	//	See if static eval will cause a cutoff or raise alpha.
	//
	valQuies = ValEval(pcon, pste);
	if (valQuies >= pste->valBeta)
		return pste->valBeta;
	if (valQuies > pste->valAlpha)
		pste->valAlpha = valQuies;
	VGenMoves(pcon, pste, fTRUE);
	//
	//	Iterate through capturing moves, to see if I can improve upon alpha
	//	(which may be the static eval).
	//
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++) {
		int	val;

		if (pcm - pste->pcmFirst <= 4)
			VSort(pste, pcm);
		VMakeMove(pcon, pste, pcm);
		(pste + 1)->valAlpha = -pste->valBeta;
		(pste + 1)->valBeta = -pste->valAlpha;
		val = -ValSearchQ(pcon, pste + 1);
		VUnmakeMove(pcon, pste, pcm);
		if (val >= pste->valBeta)
			return pste->valBeta;
		if (val > pste->valAlpha)
			pste->valAlpha = val;
	}
	return pste->valAlpha;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Full-width search.  This is the heart of the "chess playing" part of this
//	program.  It performs a recursive alpha-beta search.

int ValSearch(PCON pcon, PSTE pste)
{
	PCM	pcm;
	PCM	pcmBest;
	BOOL	fFound;
	int	hashf = hashfALPHA;
	int	val;

	//	Increment global node counter, and figure out if it is time to call
	//	the interface callback, which is what allows for an interruptible
	//	search.  It's crucial that this be called now and then.
	//
	if (++pcon->ss.nodes >= pcon->ss.nodesNext) {
		//
		//	2000 nodes is maybe 1/50to 1/100 of a second.
		//
		pcon->ss.nodesNext = pcon->ss.nodes + 2000;
		VPrCallback((void *)pcon);
		VCheckTime(pcon);
		if ((pcon->fAbort) || (pcon->fTimeout))
			return 0;
	}
	//	Stuff is doone a little differently at the root than at any other
	//	ply.
	//
	if (pste == pcon->argste) {
		//
		//	Mark this move so if I find it in a sub-tree I can detect a rep
		//	draw.  I don't check to see if a rep is already set, because if
		//	we are here, we have to be making a move, not whining about a
		//	blown chance for a draw.  I also don't try to cut off from the
		//	hash table, for the same reason.  I want a move, not a score.
		//
		//	I do probe the table though, in case there is "best move" info
		//	in there.
		//
		(void)FProbeHash(pcon, pste, &val);
		VSetRep(pcon, pste->hashkPc);
		//
		//	Generate legal moves at the root.  I do this simply so I can
		//	count them in order to put this information into the "ss" struct,
		//	where it might be passed back to the interface later.
		//
		VGenLegalMoves(pcon, pste);
		pcon->ss.ccmLegalMoves = (pste + 1)->pcmFirst - pste->pcmFirst;
	} else {
		//
		//	I can return draws and cut off if I'm somewhere other than the
		//	first ply of depth.
		//
		//	Return draw score if this is a repeated node.
		//
		if (FRepSet(pcon, pste->hashkPc)) {
			pste->ccmPv = 0;
			return 0;
		}
		//	Check hash table, in order to get "best" moves, and try to cut
		//	off.  Don't cut off within two plies of the root, otherwise the
		//	program might allow stupid rep draws.
		//
		if ((FProbeHash(pcon, pste, &val)) && (pste > pcon->argste + 1)) {
			pste->ccmPv = 0;
			return val;
		}
		//	Mark this move so if I find it in a sub-tree I can detect a rep
		//	draw.
		//
		VSetRep(pcon, pste->hashkPc);
#ifdef	NULL_MOVE
		//
		//	Do null-move search, which is a reduced depth that tries to
		//	figure out if the opponent has no threats.
		//
		if ((!pste->fInCheck) && (pste->valPcUs > valROOK) &&
			(!FThreat(pcon, pste))) {
			VClearRep(pcon, pste->hashkPc);
			return pste->valBeta;
		}
#endif
		//	Generate moves.
		//
		VGenMoves(pcon, pste, fFALSE);
	}
	//	Mark the PV and hash moves so they sort first.
	//
	if (pste->cmPv.isqFrom != isqNIL) {
		VFindPvCm(pste);
		pste->cmPv.isqFrom = isqNIL;
	}
	if (pste->phashAlways != NULL)
		VFindHash(pste, pste->phashAlways);
	if (pste->phashDepth != NULL)
		VFindHash(pste, pste->phashDepth);
	Assert(pste->plyRem >= 0);
	pcmBest = NULL;
	fFound = fFALSE;
	//
	//	Iterate through the move list, trying everything out.
	//
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++) {
		//
		//	For the first few moves, I'm going to use the sort key attached
		//	to the moves in order to try to get a good one.  If I don't find
		//	a fail-high within the first few, I'm going to assume that I don't
		//	have a clue how to order these moves, so I'll just take them all
		//	in natural order.
		//
		if (pcm - pste->pcmFirst <= 4)
			VSort(pste, pcm);
		//
		//	Things are done differently at the root of the search.
		//
		if (pste == pcon->argste) {
			SAN	san;

			//	If I'm at the root, record information about which move I am
			//	searching ...
			//
			pcon->ss.icmSearching = pcm - pste->pcmFirst;
			VCmToSan(pcon, pste, pcm, &san);
			VSanToSz(&san, pcon->ss.aszSearching);
			//
			//	... then go make the move.
			//
			VMakeMove(pcon, pste, pcm);
			//
			//	This move cannot be illegal.
			//
			Assert(!FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq,
				pste->coUs ^ 1));
		} else {
			//
			//	If I'm not at the root, I make the move, check legality, then
			//	see if the 50-move counter indicates that I need to return a
			//	draw score.
			//
			VMakeMove(pcon, pste, pcm);
			//
			//	I'm going to discard moves that leave me in check.  This
			//	could be handled, perhaps more cheaply, elsewhere, but this is
			//	a sensible way to do this.
			//
			if (FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq,
				pste->coUs ^ 1)) {
				VUnmakeMove(pcon, pste, pcm);
				continue;
			}
			//
			//	I know how many reversible moves have been made up until now.
			//	I just tried to make another move.  If I am here, I didn't
			//	leave myself in check, so I succeeded.
			//
			//	If the number of reversible moves made before this is at
			//	least 50 for each side, this was a draw.
			//
			//	Imagine a "one-move rule".  If I play Nf3 Nc6, that is not an
			//	immediate draw, because Nc6 might be mate.  So the reason I am
			//	here instead of doing this 50-move check in some other more
			//	obvious place is that I'm looking for any legal move, so I
			//	know that I wasn't mated on the 50th move.
			//
			if (pste->plyFifty >= 50 * 2) {
				pste->ccmPv = 0;
				VUnmakeMove(pcon, pste, pcm);
				VClearRep(pcon, pste->hashkPc);
				return 0;
			}
		}
		//	Set up for recursion.
		//
		(pste + 1)->fInCheck = FAttacked(pcon,
			pcon->argpi[pste->coUs ^ 1][ipiKING].isq, pste->coUs);
		(pste + 1)->plyRem = (pste + 1)->fInCheck ?
			pste->plyRem : pste->plyRem - 1;
		(pste + 1)->valAlpha = -pste->valBeta;
		(pste + 1)->valBeta = -pste->valAlpha;
		//
		//	Recurse into normal search or quiescent search, as appropriate.
		//
		if ((pste + 1)->plyRem < 0)
			val = -ValSearchQ(pcon, pste + 1);
		else
			val = -ValSearch(pcon, pste + 1);
		VUnmakeMove(pcon, pste, pcm);
		if ((pcon->fAbort) || (pcon->fTimeout)) {
			VClearRep(pcon, pste->hashkPc);
			return 0;
		}
		//	We got a value back.  We unmade the move.  We're not dead.  Let's
		//	see how good this move was.  If it was >= "beta", it was so good
		//	that we don't need to search for anything better, so we'll leave.
		//
		//	If it was not >= "beta", but it was > "alpha", this is better than
		//	anything else we've found before, but not so good we have to
		//	leave.  These kinds of moves are actually quite rare.  If I find
		//	one of these, I have to store it in the PV (main-line) that I'm
		//	constructing.  This might end up being the main-line for the whole
		//	search, if it gets backed up all the way to the root.
		//
		if (val > pste->valAlpha) {
			if (val >= pste->valBeta) {
				//
				//	This move failed high, so we are going to return beta,
				//	but if we're sitting at the root of the search I will set
				//	up a one-move PV (if this isn't already the move I'm
				//	planning to make), so this move will be made if I run out
				//	of time before resolving the fail-high.
				//
				if (pste == pcon->argste) {
					if (memcmp(pcm, pste->argcmPv, sizeof(CM))) {
						pste->argcmPv[0] = *pcm;
						pste->ccmPv = 1;
					}
					//	This function needs the root moves generated, which
					//	they are.
					//
					VDumpPv(pcon, pcon->ss.plyDepth, val, prsaFAIL_HIGH);
				}
				VRecordHash(pcon, pste, pcm, pste->valBeta, hashfBETA);
				VClearRep(pcon, pste->hashkPc);
				return pste->valBeta;
			}
			//	This move is between alpha and beta, which is actually pretty
			//	rare.  If this happens I have to add a PV move, and append the
			//	returned PV to it, and if I'm at the root I'll send the PV to
			//	the interface so it can display it.
			//
			memcpy(pste->argcmPv + 1, (pste + 1)->argcmPv,
				(pste + 1)->ccmPv * sizeof(CM));
			pste->argcmPv[0] = *pcm;
			pste->ccmPv = (pste + 1)->ccmPv + 1;
			pste->valAlpha = val;
			if (pste == pcon->argste)
				//
				//	This function needs the root moves generated, and they
				//	are.
				//	
				VDumpPv(pcon, pcon->ss.plyDepth, val, prsaNORMAL);
			hashf = hashfALPHA | hashfBETA;
			pcmBest = pcm;
		}
		fFound = fTRUE;
	}
	VClearRep(pcon, pste->hashkPc);
	if (!fFound) {
		pste->ccmPv = 0;
		if (pste->fInCheck) {
			int	val = -valMATE + (pste - pcon->argste);

			VRecordHash(pcon, pste, pcmBest, val, hashfALPHA | hashfBETA);
			return val;
		}
		VRecordHash(pcon, pste, pcmBest, 0, hashfALPHA | hashfBETA);
		return 0;
	}
	VRecordHash(pcon, pste, pcmBest, pste->valAlpha, hashf);
	return pste->valAlpha;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	These routines are in here for reasons that are a little difficult to
//	explain, but it has to do with Loser's chess being different.  This module
//	isn't used for Loser's chess, and the Loser's chess module includes its
//	own version of these two routines.

void VStalemate(int coStalemated)
{
	VPrSendResult("1/2-1/2", "Stalemate");
}

//	"coWin" just checkmated.  Tell the interface.

void VMates(int coWin)
{
	char	aszResult[64];
	char	aszReason[64];
				
	sprintf(aszResult, "%d-%d", coWin ^ 1, coWin);
	sprintf(aszReason, "%s mates", s_argszCo[coWin]);
	VPrSendResult(aszResult, aszReason);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
