
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

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This is how my program implements my engine/Winboard protocol.  If you
//	want to use my code to hook your engine up to Winboard, all you should
//	have to do is write these functions.

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Tells the engine to think forever on this position.

void VPrAnalyze(void * pv)
{
	PCON pcon = pv;

	Assert(pcon->smode == smodeIDLE);
	VThink(pcon, tmANALYZE, tmANALYZE);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Tells the engine to think for "tmUs", then emit a move, and start
//	pondering if that flag is on.

void VPrThink(void * pv, unsigned long tmUs, unsigned long tmThem)
{
	PCON pcon = pv;

	Assert(pcon->smode == smodeIDLE);
	VThink(pcon, tmUs, tmThem);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Clears the current game and starts a new one based upon "szFen".

BOOL FPrSetboard(void * pv, char * szFen)
{
	PCON pcon = pv;

	Assert(pcon->smode == smodeIDLE);
	VClearHashe();
	VClearHashp();
	pcon->fDrawOffered = fFALSE;
	return FInitCon(pcon, szFen);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Returns TRUE or FALSE depending upon whether the engine is currently
//	pondering.

BOOL FPrPondering(void * pv)
{
	PCON pcon = pv;

	return (pcon->smode == smodePONDER);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This will eventually respond by sending book moves.  For now it does
//	nothing.

void VPrBk(void * pv)
{
	PCON pcon = pv;
	PCM	pcm;
	PSTE pste = &pcon->argste[0];

	VGenLegalMoves(pcon, pste);
	for (pcm = pste->pcmFirst; pcm < (pste + 1)->pcmFirst; pcm++) {
		HASHK	hashkFull;

		VMakeMove(pcon, pste, pcm);
		Assert(!FAttacked(pcon, pcon->argpi[pste->coUs][ipiKING].isq,
			pste->coUs ^ 1));
		hashkFull = HashkFull(pcon, pste + 1);
		VUnmakeMove(pcon, pste, pcm);
		if (FBnHashSet(hashkFull)) {
			char	asz[32];
			SAN	san;

			VCmToSan(pcon, pste, pcm, &san);
			VSanToSz(&san, asz);
			VPrSendBookLine(asz);
		}
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	A move is passed in, and if the engine is pondering, the engine goes
//	into normal "think" mode.

//	At the time the engine started pondering, "tmStart" was set.  This
//	routine will call "VSetTime", which will set the end time based upon
//	the time pondering started.

BOOL FPrPonderHit(void * pv, char * szMove, unsigned long tmUs,
	unsigned long tmThem)
{
	PCON pcon = pv;

	if (pcon->smode != smodePONDER)
		return fFALSE;
	if (strcmp(pcon->aszPonder, szMove))
		return fFALSE;
	pcon->smode = smodeTHINK;
	pcon->ss.tmUs = tmUs;
	pcon->ss.tmThem = tmThem;
	VSetTime(pcon);
	return fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Adds this move to the current game.

BOOL FPrAdvance(void * pv, char * szMove)
{
	PCON pcon = pv;

	Assert(pcon->smode == smodeIDLE);
	return FAdvance(pcon, szMove);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Tells the engine whether or not to post analysis while thinking.

void VPrSetPost(void * pv, BOOL fPost)
{
	PCON pcon = pv;

	pcon->fPost = fPost;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrSetTimeControl(void * pv, int cMoves,
	unsigned long tmBase, unsigned long tmIncr)
{
	PCON pcon = pv;

	VSetTimeControl(pcon, cMoves, tmBase, tmIncr);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrSetFixedDepthTimeControl(void * pv, int plyDepth)
{
	PCON pcon = pv;

	VSetFixedDepthTimeControl(pcon, plyDepth);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This initiates a request to move now.  The engine is thinking but may
//	be pondering.

void VPrMoveNow(void * pv)
{
	PCON pcon = pv;

	pcon->ss.tmEnd = pcon->ss.tmStart;
	pcon->ss.tmExtra = 0;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Back up in the game list.

void VPrUndoMove(void * pv)
{
	PCON pcon = pv;

	Assert(pcon->smode == smodeIDLE);
	VUndoMove(pcon);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Set ponder flag TRUE or FALSE.

void VPrSetPonder(void * pv, BOOL fPonder)
{
	PCON pcon = pv;

	pcon->fPonder = fPonder;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Abort the search immediately.

void VPrAbort(void * pv)
{
	PCON pcon = pv;

	pcon->fAbort = fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This does nothing in Gerbil.

void VPrResult(char * szResult, char * szWhy)
{
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Name of engine must be second line in here.

char * const c_argszHelp[] = {
	"",
#ifdef	LOSERS
	"Gerbil, Release 02 (Loser's)",
#else
	"Gerbil, Release 02",
#endif
	"Copyright (c) 2001, Bruce Moreland.  All rights reserved.",
	"",
	"This program is supposed to be run from Winboard.  Please see the file",
	"\"readme.txt\" for details.  If you would like to run in command line mode,",
	"you will have an extremely limited and unpleasant feature set.",
	"",
	"Before you do anything, type \"protover 2\" and press the enter key.",
	"",
	"The computer is playing black.  You can make moves in the form \"e2e4\",",
	"\"e1g1\", \"e7e8q\", etc.  Don't make a mistake.  If you want the computer to", 
	"play white, type \"go\".  If you want to turn off the analysis gibberish,",
	"type \"nopost\".  Type \"easy\" if you don't want it thinking on your time.",
	"\"new\" starts a new game.",
	"",
	"You'd better find a chess board unless you like to play blindfolded.",
	"",
	NULL,
};

void VPrBanner(void)
{
	int	i;

	for (i = 0; c_argszHelp[i] != NULL; i++)
		VPrSendComment(c_argszHelp[i]);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Returns the engine's name.  The engine's name must be the second string in
//	the "c_argszHelp" array above.

void VPrMyName(char * szName)
{
	strcpy(szName, c_argszHelp[1]);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This passes back the move I'm pondering on.

void VPrHint(void * pv)
{
	PCON pcon = pv;

	if (pcon->smode != smodePONDER)
		return;
	if (pcon->aszPonder[0] != '\0')
		VPrSendHint(pcon->aszPonder);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

BOOL FPrStatus(void * pv, PSTAT_REC psr)
{
	PCON pcon = pv;

	Assert(pcon->smode != smodeIDLE);
	psr->tmElapsed = TmNow() - pcon->ss.tmStart;
	psr->nodes = pcon->ss.nodes;
	psr->plyDepth = pcon->ss.plyDepth;
	psr->iMoveSearching = pcon->ss.ccmLegalMoves - pcon->ss.icmSearching;
	psr->cLegalMoves = pcon->ss.ccmLegalMoves;
	strcpy(psr->aszSearching, pcon->ss.aszSearching);
	return fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This engine ignores draw offers.

void VPrDrawOffered(void * pv)
{
	PCON pcon = pv;
	
	pcon->fDrawOffered = fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrOpponentComputer(void * pv)
{
	PCON pcon = pv;

	VPrSendComment("Opponent is a computer.");
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrOpponentName(void * pv, char * szName)
{
	PCON pcon = pv;
	
	VPrSendComment("Opponent is: %s", szName);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrRating(void * pv, int eloUs, int eloThem)
{
	PCON pcon = pv;

	VPrSendComment("My rating is %ld; opponent's rating is %ld.",
		eloUs, eloThem);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VPrIcs(void * pv, char * szIcs)
{
	PCON pcon = pv;

	VPrSendComment("I am playing on: %s", szIcs);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Inits engine and returns pointer to context.

void * PvPrEngineInit(int argc, char * argv[])
{
	return (void *)PconInitEngine(argc, argv);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
