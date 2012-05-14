
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

#include <gproto.h>
#include "engine.h"
#include <stdlib.h>
#include <stdarg.h>
#include <ctype.h>

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	If this is called while a search is running, it won't change the time
//	control that's being used for this move.

void VSetTimeControl(PCON pcon, int cMoves, TM tmBase, TM tmIncr)
{
	switch (cMoves) {
	case 0:
		pcon->tc.tct = tctINCREMENT;
		break;
	case 1:
		pcon->tc.tct = tctFIXED_TIME;
		break;
	default:
		pcon->tc.tct = tctTOURNEY;
		break;
	}
	pcon->tc.cMoves = cMoves;
	pcon->tc.tmBase = tmBase;
	pcon->tc.tmIncr = tmIncr;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	See the caveat regarding "VSetTimeControl".

void VSetFixedDepthTimeControl(PCON pcon, int plyDepth)
{
	pcon->tc.plyDepth = plyDepth;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This is called at the start of a search that's expected to output a move
//	at some point.
//
//	"tmStart" is *not* set by this routine.

void VSetTime(PCON pcon)
{
	Assert(sizeof(long) == sizeof(TM));		// If this fires, increment
											//  time control is broken.
	switch (pcon->tc.tct) {
		TM	tmBase;
		TM	tmExtra;
		int	movLeft;
		int	movMade;

	case tctINCREMENT:
		if (pcon->tc.tmIncr) {
			TM	tmTarget;

			//	An increment time control tries not to go below the increment
			//	plus 30 seconds, or 6 times the increment, whichever is more.
			//
			//	If it finds itself below that, it will the increment minus
			//	1/10 of the time by which it is under the target.
			//
			//	If it is over that, it will use the increment plus 1/20 of
			//	the time by which it is over the target.
			//
			//	So the program will drift slowly down to the target, but if
			//	it gets a ways below it, it will tend to come up quickly.
			//
			//	There are a zillion different increment time controls, and
			//	sometimes I could end up with a stupid base time.  I try hard
			//	to make sure the base time doesn't go below zero, which would
			//	be very bad since my time quantity is an unsigned value.
			//
			//	I will try hard to use >= half of the increment.  I will also
			//	make sure to not use more than half the remaining time (which
			//	is taken care of by the code at "lblSet".
			//
			tmTarget = pcon->tc.tmIncr + 30000;
			if (tmTarget < 6 * pcon->tc.tmIncr)
				tmTarget = 6 * pcon->tc.tmIncr;
			if (pcon->ss.tmUs >= tmTarget)
				tmBase = pcon->tc.tmIncr + (pcon->ss.tmUs - tmTarget) / 20;
			else	// The following statement can go negative on an unsigned.
				tmBase = pcon->tc.tmIncr - (tmTarget - pcon->ss.tmUs) / 10;
			Assert(sizeof(long) == sizeof(TM));
			if ((long)tmBase < (long)pcon->tc.tmIncr / 2)
				tmBase = pcon->tc.tmIncr / 2;
		} else {
			//
			//	A zero-increment time control will use 1/30 of the remaining
			//	time down to 10 minutes, 1/40 down to one minute, and 1/60
			//	after that.
			//
			if (pcon->ss.tmUs >= 600000)		// 10 minutes.
				tmBase = pcon->ss.tmUs / 30;
			else if (pcon->ss.tmUs > 60000)	// 1 minute.
				tmBase = pcon->ss.tmUs / 40;
			else
				tmBase = pcon->ss.tmUs / 60;
		}
		//	This code dummy-checks "tmBase", assigns "tmExtra", then assigns
		//	an end-time based upon "pcon->ss.tmStart".
		//
		//	First I check to see if I'm scheduled to eat more than half of
		//	my remaining time.  I put a ceiling at that amount, right off.
		//
		//	Next, I'll assign some emergency time.  This is 3x the base time,
		//	with a ceiling on base + extra of 1/2 the remaining time.  I will
		//	also allocate no emergency time if I have < 30 seconds on the
		//	clock.
		//
		//	So there will be plenty of "extra" time if there is time left,
		//	otherwise there is little or none.
		//
lblSet:	if (tmBase > pcon->ss.tmUs / 2)
			tmBase = pcon->ss.tmUs / 2;
		tmExtra = (pcon->ss.tmUs < 20000) ? 0 : tmBase * 3;
		if (tmBase + tmExtra > pcon->ss.tmUs / 2)
			tmExtra = pcon->ss.tmUs / 2 - tmBase;
		pcon->ss.tmEnd = pcon->ss.tmStart + tmBase;
		pcon->ss.tmExtra = tmExtra;
		break;
	case tctFIXED_TIME:
		tmBase = pcon->tc.tmBase;		// Use *all* the time.
		pcon->ss.tmEnd = pcon->ss.tmStart + tmBase;
		pcon->ss.tmExtra = 0;			// No emergency time.
		break;
	case tctTOURNEY:
		movMade = pcon->gc.ccm / 2 + pcon->gc.movStart - 1;
		while (movMade >= pcon->tc.cMoves)
			movMade -= pcon->tc.cMoves;
		movLeft = pcon->tc.cMoves - movMade;
		Assert(movLeft >= 1);
		//
		//	This expression is kind of nasty.  It is:
		//
		//		Time / (3/4 x Moves + 3)
		//
		//	The point of the "3" is to make sure there's some extra time pad
		//	at the beginning, and the point of the 3/4 is to make sure that
		//	earlier moves take a little longer per move.
		//
		//	This expression has been reduced to ...
		//
		//		(4 * Time) / (3 * Moves + 12)
		//
		//	.. by multiplying the top and the bottom by 4.  This helps out
		//	with the integer math.
		//
		tmBase = (4 * pcon->ss.tmUs) / (3 * movLeft + 12);
		goto lblSet;
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This is called a whole bunch of times via the engine/interface protocol.
//	Given what is known about the time control, the current time, and the
//	search depth, this may set "ptcon->fTimeout", which will eventually finish
//	the search.

//	This routine has a built-in defense against cases where someone might try
//	to force a move before the first ply has been fully considered.  It will
//	simply ignore such cases.

void VCheckTime(PCON pcon)
{
	TM	tmNow;

	if (pcon->ss.plyDepth == 1)	// Can't time out before a one-ply
		return;					//  search is finished.
	if (pcon->smode != smodeTHINK)
		return;
	if ((tmNow = TmNow()) >= pcon->ss.tmEnd) {
		//
		//	Time has expired.  Check for a fail-high or fail low and try to
		//	add some time.  Don't add any time if we're already dead lost and
		//	just slowly collapsing, or if we are won and are probably failing
		//	high repeatedly.  The standard for "won" is a little greater than
		//	for "lost".
		//
		if ((pcon->ss.prsa != prsaNORMAL) && (pcon->ss.val < valROOK) &&
			(pcon->ss.val >= -valMINOR))
			if (pcon->ss.tmExtra < 1000) {
				pcon->ss.tmEnd += pcon->ss.tmExtra;
				pcon->ss.tmExtra = 0;
			} else {
				pcon->ss.tmEnd += pcon->ss.tmExtra / 2;
				pcon->ss.tmExtra -= pcon->ss.tmExtra / 2;
			}
		if (tmNow >= pcon->ss.tmEnd)
			pcon->fTimeout = fTRUE;
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This routine takes a "SAN" record and produces a SAN move output string
//	(such as "e4", or "dxe6+", or "Nbd7", or "Qxg7#").

//	The SAN element already has all the information needed to do this, so it's
//	a very simple process.

void VSanToSz(PSAN psan, char * sz)
{
	if (psan->cmf & cmfCASTLE) {
		if ((psan->isqTo == isqG1) || (psan->isqTo == isqG8))
			sz += sprintf(sz, "O-O");
		else
			sz += sprintf(sz, "O-O-O");
	} else {
		if (psan->pc == pcPAWN)
			*sz++ = FilFromIsq(psan->isqFrom) + 'a';
		else {
			*sz++ = s_argbPc[coWHITE][psan->pc];
			if (psan->sanf & sanfRNK_AMBIG)
				*sz++ = FilFromIsq(psan->isqFrom) + 'a';
			if (psan->sanf & sanfFIL_AMBIG)
				*sz++ = RnkFromIsq(psan->isqFrom) + '1';
		}
		if (psan->cmf & cmfCAPTURE) {
			*sz++ = 'x';
			*sz++ = FilFromIsq(psan->isqTo) + 'a';
			*sz++ = RnkFromIsq(psan->isqTo) + '1';
		} else if (psan->pc == pcPAWN)
			*sz++ = RnkFromIsq(psan->isqTo) + '1';
		else {
			*sz++ = FilFromIsq(psan->isqTo) + 'a';
			*sz++ = RnkFromIsq(psan->isqTo) + '1';
		}
		if (psan->cmf & cmfPR_MASK) {
			*sz++ = '=';
			*sz++ = s_argbPc[coWHITE][psan->cmf & cmfPR_MASK];
		}
	}
	if (psan->sanf & sanfMATE)
		*sz++ = '#';
	else if (psan->sanf & sanfCHECK)
		*sz++ = '+';
	*sz = '\0';
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This takes a CM and converts it to a SAN record, which can be used by
//	"VSanToSz" to make something pretty to display.

//	This is a terrible process, in large part because of disambiguation.
//	Disambiguation would be extremely easy, if all I had to do is disambiguate
//	between moves produced by the move generator.  But some of these moves are
//	not actually legal, so I have to delete those.

//	Also, part of the process includes appending "+", or "#" if necessary,
//	which especially in the latter case is annoying.

//	So this routine figures out if a given move is ambiguous, and if so
//	records exactly how, and it also figures out if the move is check or mate.

//	WARNING! This function won't work if the moves aren't generated for this
//	position, and these moves are pseudo-legal, not fully legal.

void VCmToSan(PCON pcon, PSTE pste, PCM pcmSrc, PSAN psan)
{
	PCM	pcm;
	BOOL fAmbiguous;

	//	Easy stuff first.
	//
	psan->pc = pcon->argsq[pcmSrc->isqFrom].ppi->pc;
	psan->isqFrom = pcmSrc->isqFrom;
	psan->isqTo = pcmSrc->isqTo;
	psan->cmf = pcmSrc->cmf;
	psan->sanf = sanfNONE;
	//
	//	Disambiguate.
	//
	fAmbiguous = fFALSE;
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++)
		if ((pcm->isqTo == pcmSrc->isqTo) &&
			(pcm->isqFrom != pcmSrc->isqFrom) &&
			(pcon->argsq[pcm->isqFrom].ppi->pc == psan->pc)) {
			VMakeMove(pcon, pste, pcm);
			if (!FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq,
				pste->coUs ^ 1)) {
				if (RnkFromIsq(pcm->isqFrom) == RnkFromIsq(pcmSrc->isqFrom))
					psan->sanf |= sanfRNK_AMBIG;
				if (FilFromIsq(pcm->isqFrom) == FilFromIsq(pcmSrc->isqFrom))
					psan->sanf |= sanfFIL_AMBIG;
				fAmbiguous = fTRUE;
			}
			VUnmakeMove(pcon, pste, pcm);
		}
	if ((fAmbiguous) && (!(psan->sanf & (sanfRNK_AMBIG | sanfFIL_AMBIG))))
		psan->sanf |= sanfRNK_AMBIG;	// "Nbd2" rather than "N1d2".
	//
	//	The gross disambiguation has already been done.  Now I'm going to
	//	execute the move and see if it is check or mate.
	//
	VMakeMove(pcon, pste, pcmSrc);
	if (FAttacked(pcon, pcon->argpi[pste->coUs ^ 1]
		[ipiKING].isq, pste->coUs)) {
		psan->sanf |= sanfCHECK;
		VGenLegalMoves(pcon, pste + 1);
		if ((pste + 1)->pcmFirst == (pste + 2)->pcmFirst)
			psan->sanf |= sanfMATE;
	}
	VUnmakeMove(pcon, pste, pcmSrc);
	//
	//	That was painful.
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This spits a PV out to the interface.
//
//	WARNING: This function must have the root moves already generated.

void VDumpPv(PCON pcon, int ply, int val, int prsa)
{
	char asz[1024];
	char * sz;
	int	i;

	pcon->ss.val = val;	// Remember the value of this search so far.
	pcon->ss.prsa = prsa;
	if (!pcon->fPost)	// The interface doesn't want PV's.
		return;
	sz = asz;
	//
	//	In order to do the SAN thing, I need to actually execute the moves
	//	on the board, so I call "VMakeMove" at the end of this.
	//
	//	I assume the moves in the PV are legal (it can't be otherwise).
	//
	for (i = 0; i < pcon->argste[0].ccmPv; i++) {
		SAN	san;
		PCM	pcmPv;
		PSTE	pste;

		pste = pcon->argste + i;
		pcmPv = &pcon->argste[0].argcmPv[i];
		if (i)
			VGenMoves(pcon, pste, fFALSE);
		VCmToSan(pcon, pste, pcmPv, &san);
		VSanToSz(&san, sz);
		sz += strlen(sz);
		if (i + 1 < pcon->argste[0].ccmPv)
			*sz++ = ' ';
		VMakeMove(pcon, pste, pcmPv);
	}
	//	Undo all of the make-move's.
	//
	for (; --i >= 0;)
		VUnmakeMove(pcon, pcon->argste + i, &pcon->argste[0].argcmPv[i]);
	*sz++ = '\0';
	VPrSendAnalysis(ply, val, TmNow() - pcon->ss.tmStart,
		pcon->ss.nodes, prsa, asz);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This is called before a search in order to tell the search which moves
//	to try first when iterating through them.

void VSetPv(PCON pcon)
{
	int	i;
	
	for (i = 0; i < pcon->argste[0].ccmPv; i++)
		pcon->argste[i].cmPv = pcon->argste[0].argcmPv[i];
	for (; i < csteMAX; i++) {
		pcon->argste[i].cmPv.isqFrom = isqNIL;
		pcon->argste[i].cmPv.isqTo = isqNIL;
		pcon->argste[i].cmPv.cmf = cmfNONE;
		pcon->argste[i].cmPv.cmk = cmkNONE;
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	"rand()" produces 15-bit values (0..32767).  I want 64 bits.

static U64 U64Rand(void)
{
	return (U64)rand() ^ ((U64)rand() << 15) ^ ((U64)rand() << 30);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Fills up the hash key table with random gibberish.  These will be XOR'd
//	together in order to create the semi-unique hash key for any given
//	position.  They need to be as random as possible otherwise there will be
//	too many hash collisions.

//	I've never cared about hash collisions.  If they are a bigger problem
//	than I think, you might want to make this more random.

void VInitHashk(void)
{
	int	co;
	int	pc;
	int	isq;
	int	fil;
	int	cf;
	
	for (pc = pcPAWN; pc <= pcKING; pc++)
		for (co = coWHITE; co <= coBLACK; co++)
			for (isq = 0; isq < csqMAX; isq++)
				if (FilFromIsq(isq) <= filH)
					s_arghashkPc[pc][co][isq] = U64Rand();
	for (fil = filA; fil <= filH; fil++)
		s_arghashkEnP[fil] = U64Rand();
	for (cf = 0; cf <= cfMAX; cf++)
		s_arghashkCf[cf] = U64Rand();
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This converts a CM to "e2e4" (or "e7e8q") format.  I'm writing this with
//	Winboard in mind, and that is a format that Winboard will accept.  It is
//	also very easy to generate.

void VCmToSz(PCM pcm, char * sz)
{
	sz += sprintf(sz, "%c%c%c%c",
		FilFromIsq(pcm->isqFrom) + 'a',
		RnkFromIsq(pcm->isqFrom) + '1',
		FilFromIsq(pcm->isqTo) + 'a',
		RnkFromIsq(pcm->isqTo) + '1');
	if ((pcm->cmf & cmfPR_MASK) != pcPAWN)
		sz += sprintf(sz, "%c", s_argbPc[coBLACK][pcm->cmf & cmfPR_MASK]);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Once the first STE record is set up properly, this cleans up a few
//	details in the record, and sets up the rest of the array properly.
//	

void VFixSte(PCON pcon)
{
	int	i;
	PSTE	pste = &pcon->argste[0];

	pste->pcmFirst = pcon->argcm;
#ifdef	NULL_MOVE
	pste->fNull = fFALSE;
#endif
	pste->fInCheck = FAttacked(pcon,
		pcon->argpi[pste->coUs][ipiKING].isq, pste->coUs ^ 1);
	for (i = 1; i < csteMAX; i++)
		pcon->argste[i].coUs = pste->coUs ^ (i & 1);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This removes dead pieces from the piece list.  It isn't called during the
//	search, because the pieces have to be there when the move is unmade.

void VFixPi(PCON pcon)
{
	int	co;
	
	for (co = coWHITE; co <= coBLACK; co++) {
		int	ipiI;
		int	ipiJ;

		for (ipiI = ipiJ = 0; ipiI < pcon->argcpi[co]; ipiI++) {
			PPI ppiI = &pcon->argpi[co][ipiI];

			if (!ppiI->fDead) {
				PPI ppiJ = &pcon->argpi[co][ipiJ++];

				*ppiJ = *ppiI;
				pcon->argsq[ppiJ->isq].ppi = ppiJ;
			}
		}
		pcon->argcpi[co] = ipiJ;
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	FIDE rules state that if a position repeats three times, a draw can be
//	claimed.  But the rules also state that two positions with the pieces in
//	the same place, but with different castling or en-passant possibilities,
//	are not the same position.  In order to avoid claiming draws erroneously,
//	I maintain the notion of a "full" key, which includes XOR'd values for
//	various castling flags and en-passant files.
//
//	It's kind of nasty to compute this, because even though an en-passant
//	capture might appear possible, it might not actually be legal.
//
//	This routine generates legal moves for this ply, so if that screws you up,
//	watch out.
//
//	Ordinarily I don't mess with full hash keys, but sometimes I have to.

HASHK HashkFull(PCON pcon, PSTE pste)
{
	HASHK	hashk = pste->hashkPc;
	
	Assert(pste->cf < cfMAX);
	hashk ^= s_arghashkCf[pste->cf];
	if (pste->isqEnP != isqNIL) {
		PCM	pcm;

		VGenLegalMoves(pcon, pste);
		for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++)
			if (pcm->cmf & cmfMAKE_ENP) {
				hashk ^= s_arghashkEnP[FilFromIsq(pste->isqEnP)];
				break;
			}
	}
	return hashk;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Execute "szMove" on the board, update game context ("pcon->gc"), then set
//	the other "pcon" fields up so it's ready to search the new position.

//	If it doesn't find the move, it returns FALSE.

//	Advancing a move is mostly moving "pcon->argste[1]" on top of
//	"pcon->argste[0]", after a call to "VMakeMove" has set up the new context.
//	This is a little nasty and dangerous, so it's a potential bug source.

//	If you modify "VMakeMove", take this potential problem into account.

//	Be especially careful if you mess with this routine, since bugs in it
//	won't show up in test suites -- they'll happen in games.

BOOL FAdvance(PCON pcon, char * szMove)
{
	PSTE	pste;
	PCM	pcm;
	
	pste = pcon->argste;
	VGenLegalMoves(pcon, pste);
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++) {
		char	asz[16];

		VCmToSz(pcm, asz);
		if (!strcmp(asz, szMove)) {
			VMakeMove(pcon, pste, pcm);
			Assert(!FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq,
				pste->coUs ^ 1));
			memcpy(pste, pste + 1, sizeof(STE));
			VFixPi(pcon);		// Remove dead pieces for good.
			VFixSte(pcon);
			pcon->gc.argcm[pcon->gc.ccm] = *pcm;
			pcon->gc.arghashk[++pcon->gc.ccm] = pste->hashkPc;
			pcon->gc.arghashkFull[pcon->gc.ccm] = HashkFull(pcon, pste);
			return fTRUE;
		}
	}
	return fFALSE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This backs up one move in the current game, and deletes all trace of the
//	backed up move.

//	It works by going back to the beginning and going forward.

//	This routine is a little evil because it assumes that parts of the current
//	game context aren't blown out by "FInitCon".

void VUndoMove(PCON pcon)
{
	int	i;
	GAME_CONTEXT	gc;
	BOOL	f;

	gc = pcon->gc;
	if (!gc.ccm)
		return;
	f = FInitCon(pcon, gc.aszFen);
	Assert(f);
	for (i = 0, gc.ccm--; i < gc.ccm; i++) {
		char	asz[16];
		
		VCmToSz(&gc.argcm[i], asz);
		f = FAdvance(pcon, asz);
		Assert(f);
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This checks to see if the side to move at the root of "pcon" is
//	stalemated.

BOOL FStalemated(PCON pcon)
{
	PSTE	pste;
	
	pste = pcon->argste;
	if (FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq, pste->coUs ^ 1))
		return fFALSE;
	VGenLegalMoves(pcon, pste);
	return (pste->pcmFirst == (pste + 1)->pcmFirst);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This will be used to figure out which move to search first.  It is very
//	important to try to search the best move first, since this will produce
//	a huge reduction in tree size.

//	Best moves are typically winning captures and moves that have been shown
//	to be good here in the past (PV move and hash table move).

void VSort(PSTE pste, PCM pcm)
{
	PCM	pcmI;
	PCM	pcmBest = pcm;
	CM	cm;
	
	for (pcmI = pcm + 1; pcmI < (pste + 1)->pcmFirst; pcmI++)
		if (pcmI->cmk > pcmBest->cmk)
			pcmBest = pcmI;
	cm = *pcm;
	*pcm = *pcmBest;
	*pcmBest = cm;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	A simple routine that checks for draws based upon the game history, and by
//	that I mean repetition draws and 50-move draws.  It does nothing about
//	stalemate, insuffient material, etc.
//
//	This uses "full" hash keys to check for 3x repetition, in order to avoid
//	declaring a draw when a position has appeard three times, but with
//	different castling and/or en-passant possibilities.

void VDrawCheck(PCON pcon)
{
	PSTE pste = &pcon->argste[0];
	int	i;
	int	cReps;

	Assert(pcon->smode == smodeTHINK);
	if ((pste->valPnUs == 0) && (pste->valPnThem == 0)) {
		//
		//	If there are no pawns, claim a draw due to insufficient material
		//	if there are no pieces, one side has a bare king and the other
		//	has a minor, or if both sides have bishops (same color).
		//
		if ((pste->valPcUs == 0) && (pste->valPcThem == 0)) {
lblIns:		VPrSendResult("1/2-1/2", "Insufficient material");
			return;
		}
		if ((pste->valPcUs == valMINOR) && (pste->valPcThem == 0))
			goto lblIns;
		if ((pste->valPcUs == 0) && (pste->valPcThem == valMINOR))
			goto lblIns;
		if ((pste->valPcUs == valMINOR) && (pste->valPcThem == valMINOR)) {
			int	argco[coMAX];
			int	co;

			for (co = coWHITE; co <= coBLACK; co++) {
				int	ipi;
				int	rnk;
				int	fil;
				
				for (ipi = 0; ipi < pcon->argcpi[co]; ipi++)
					if ((!pcon->argpi[co][ipi].fDead) &&
						(pcon->argpi[co][ipi].pc == pcBISHOP))
						break;
				if (ipi == pcon->argcpi[co])
					break;
				rnk = RnkFromIsq(pcon->argpi[co][ipi].isq);
				fil = FilFromIsq(pcon->argpi[co][ipi].isq);
				argco[co] = !((rnk ^ fil) & 1);
			}
			if ((co > coBLACK) && (argco[coWHITE] == argco[coBLACK]))
				goto lblIns;
		}
	}
	if (pcon->argste[0].plyFifty >= 50 * 2) {
		VPrSendResult("1/2-1/2", "Draw by 50-move rule");
		return;
	}
	cReps = 1;
	for (i = 0; i < pcon->gc.ccm; i++)
		if (pcon->gc.arghashkFull[i] ==
			pcon->gc.arghashkFull[pcon->gc.ccm])
			if (++cReps >= 3) {
				VPrSendResult("1/2-1/2", "Draw by repetition");
				return;
			}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

char const * c_argszUsage[] = {
	"Usage: %s [flags]",
	"  -?        | Usage",
	"  -b<B>     | Override INI file \"UseBook\" with B",
	"  -bf<F>    | Override INI file \"BookFile\" with F",
	"  -bs<N>    | Override INI file \"MaxBook\" with N",
	"  -dc       | Dump book in compact format (then exit)",
	"  -df       | Dump book in fuller format (then exit)",
	"  -hp<N>    | Override INI file \"MaxPawnHash\" with N",
	"  -ht<N>    | Override INI file \"MaxHash\" with N",
	"  -p        | Tell the engine to reduce its system priority.",
	"  -r<B>     | Override INI file \"Resign\" with B",
	"  -t<F> <N> | Profile FEN F for N seconds",
	NULL,
};

void VUsage(int argc, char * argv[])
{
	int	i;
	
	for (i = 0; c_argszUsage[i] != NULL; i++)
		VPrSendComment(c_argszUsage[i], argv[0]);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

PCON PconInitEngine(int argc, char * argv[])
{
	static	CON s_con;
	PCON pcon = &s_con;
	char	aszBook[512];	// Opening book currently loaded.
	int	cbMaxHash;			// Number of transposition hash element bytes
							//  at most (actual number will be <= this).
	int	cbMaxBook;			// Number of transposition hash element bytes
							//  at most (actual number will be <= this).
	int	cbMaxPawnHash;		// Number of pawn hash bytes at most (actual
							//  number will be <= this).
	int	bdm;				// Book dump mode.
	BOOL	f;
	int	i;
	char	aszProfileFen[256];
	TM	tmProfile;

	bdm = bdmNONE;
	VGetProfileSz("BookFile", "gerbil.opn", aszBook, sizeof(aszBook));
#ifdef	LOSERS
	pcon->fUseBook = IGetProfileInt("UseBook", fFALSE);
#else
	pcon->fUseBook = IGetProfileInt("UseBook", fTRUE);
#endif
	cbMaxHash = IGetProfileInt("MaxHash", 10000000);
	cbMaxPawnHash = IGetProfileInt("MaxPawnHash", 1000000);
	cbMaxBook = IGetProfileInt("MaxBook", 262144);
	pcon->fResign = IGetProfileInt("Resign", fTRUE);
	pcon->fLowPriority = fFALSE;
	aszProfileFen[0] = '\0';
	//
	//	Process command line switches after setting appropriate defaults.
	//
	for (i = 1; i < argc; i++) {
		char * sz = argv[i];

		switch (*sz++) {
		case '/':
		case '-':
			switch (*sz++) {
			case '?':
lblError:		VUsage(argc, argv);
				return NULL;
			case 'b':
				switch (*sz++) {
				case 'f':		// -bf[F] "BookFile"
					if (*sz != '\0')
						strcpy(aszBook, sz);
					else if (i + 1 == argc)
						goto lblError;
					else if ((argv[++i][0] != '-') &&
						(argv[i][0] != '/'))
						strcpy(aszBook, argv[i]);
					else
						goto lblError;
					break;
				case 's':		// -bs[D] "MaxBook"
					if (*sz != '\0') {
						if (!isdigit(*sz))
							goto lblError;
						cbMaxBook = atoi(sz);
					} else if (i + 1 == argc)
						goto lblError;
					else if (isdigit(argv[++i][0]))
						cbMaxBook = atoi(argv[i]);
					else
						goto lblError;
					break;
				default:		// -b[D] "UseBook"
					if (*--sz != '\0') {
						if (!isdigit(*sz))
							goto lblError;
						pcon->fUseBook = atoi(sz);
					} else if (i + 1 == argc)
						goto lblError;
					else if (isdigit(argv[++i][0]))
						pcon->fUseBook = atoi(argv[i]);
					else
						goto lblError;
					break;
				}
				break;
			case 'd':
				switch (*sz++) {
				case 'f':
					bdm = bdmFULL;
					break;
				case 'c':
					bdm = bdmCOMPACT;
					break;
				default:
					goto lblError;
				}
				break;
			case 'h':
				switch (*sz++) {
				case 'p':		// -hp[D] "MaxPawnHash"
					if (*sz != '\0') {
						if (!isdigit(*sz))
							goto lblError;
						cbMaxPawnHash = atoi(sz);
					} else if (i + 1 == argc)
						goto lblError;
					else if (isdigit(argv[++i][0]))
						cbMaxPawnHash = atoi(argv[i]);
					else
						goto lblError;
					break;
				case 't':		// -hp[D] "MaxHash"
					if (*sz != '\0') {
						if (!isdigit(*sz))
							goto lblError;
						cbMaxHash = atoi(sz);
					} else if (i + 1 == argc)
						goto lblError;
					else if (isdigit(argv[++i][0]))
						cbMaxHash = atoi(argv[i]);
					else
						goto lblError;
					break;
				default:
					goto lblError;
				}
				break;
			case 'p':			// -p "Priority"
				pcon->fLowPriority = fTRUE;
				break;
			case 'r':			// -r<B> "Resign"
				if (*sz != '\0') {
					if (!isdigit(*sz))
						goto lblError;
					pcon->fResign = atoi(sz);
				} else if (i + 1 == argc)
					goto lblError;
				else if (isdigit(argv[++i][0]))
					pcon->fResign = atoi(argv[i]);
				else
					goto lblError;
				break;
			case 't':			// -t<F> <N> "Profile Fen"
				if (*sz != '\0')
					strcpy(aszProfileFen, sz);
				else if (i + 1 == argc)
					goto lblError;
				else if ((argv[++i][0] != '-') && (argv[i][0] != '/'))
					strcpy(aszProfileFen, argv[i]);
				else
					goto lblError;
				if (i + 1 == argc)
					goto lblError;
				else if (!isdigit(argv[++i][0]))
					goto lblError;
				tmProfile = atol(argv[i]) * 1000;
				break;
			default:
				goto lblError;
			}
			break;
		default:
			goto lblError;
		}
	}
	if (bdm == bdmNONE)	// Avoid writing output if I'm going to dump the book.
		VPrSendComment("UseBook: %s", c_argszNoYes[pcon->fUseBook]);
	VInitAttackData();
	VInitHashk();
#if !defined(LOSERS)
	VInitMiddlegamePawnTable();
#endif
	if ((pcon->fUseBook) && (!FGenBook(pcon, aszBook, cbMaxBook, bdm)))
		return NULL;
	if (!FInitHashe(pcon, cbMaxHash))
		return NULL;
	if (!FInitHashp(pcon, cbMaxPawnHash))
		return NULL;
	VReseed();
	pcon->smode = smodeIDLE;
	f = FInitCon(pcon, s_aszFenDefault);
	Assert(f);
	VPrSetPost(pcon, fFALSE);
	VPrSetPonder(pcon, fTRUE);
	VSetTimeControl(pcon, 1, 2000, 0);	// Set to play 2 seconds per move if
										//  the user is foolish enough to try
										//  to run the program from the
										//  command line.
	if (aszProfileFen[0]) {
		if (!FPrSetboard(pcon, aszProfileFen)) {
			VPrSendComment("Can't initialize profile FEN.");
			exit(1);
		}
		VPrSetTimeControl(pcon, 1, tmProfile, 0);
		VPrSetPost(pcon, fFALSE);
		VPrSetPonder(pcon, fFALSE);
		VPrSendComment("Profile for %ld sec", tmProfile / 1000);
		VPrThink(pcon, tmProfile, 0);
		exit(1);
	}
	return pcon;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	These two functions look for specific moves in the move list, in order to
//	increase their sort key so they will sort first.

void VFindPvCm(PSTE pste)
{
	PCM	pcm;
	
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++)
		if ((pcm->isqFrom == pste->cmPv.isqFrom) &&
			(pcm->isqTo == pste->cmPv.isqTo) &&
			(pcm->cmf == pste->cmPv.cmf)) {
			pcm->cmk |= cmkPV;
			return;
		}
}

void VFindHash(PSTE pste, PHASH phash)
{
	PCM	pcm;
	
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++)
		if ((pcm->isqFrom == phash->isqFrom) &&
			(pcm->isqTo == phash->isqTo) &&
			((int)(pcm->cmf & cmfPR_MASK) == phash->pcTo)) {
			pcm->cmk |= cmkHASH;
			return;
		}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
