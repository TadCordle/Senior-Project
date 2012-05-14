
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
#include <stdarg.h>
#include <ctype.h>

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

RGHASHP s_rghashp;			// Pawn hash table.
unsigned	s_chashpMac;	// Number of elements in the table, minus one.  I
							//  do the minus one thing in order to avoid an
							//  inner-loop subtract, which is no big deal, but
							//  every little bit helps.

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

#ifdef	DEBUG

//	I sat here and thought about how to write this routine, for about five
//	minutes.  I could have written something that would have required
//	modification any time the pawn structure stuff is messed with, but I
//	didn't like that idea, since the code I've used in the past for this
//	routine gets kind of annyong to change.

//	On the other hand, this program is supposed to be simple and direct.

//	Finally, I could not resist doing something I have never done -- write
//	something that works like "printf".

//	The format string is comprised of commands.  The commands are as follows:

//		digit ('0'..'9')		The next argument is an array of "digit"
//								bitmaps, to be displayed vertically, with
//								"*" for any set bit and "." for any zero bit.
//								The argument after this is a string pointing
//								to the column header.

//		'!'						The next argument is an array of two bitmaps,
//								which should be displayed as one bitmap with
//								"P" if a bit from the first map is set, and
//								"p" if a bit from the second map is set, and
//								"." if neither bit is set.  There is nothing
//								special if both maps are set, that comes up
//								"P".  The argument after this is a string
//								pointing to the column header.

//	The column header strings should be <= 8 characters long.

//	Here's an example call:

//		VDumpPawnMaps("!1", argbmLoc, "Loc", &bmDoubled, "Doubled");

//	This would be somewhat different, but would work:

//		VDumpPawnMaps("21", argbmLoc, "Loc", &bmDoubled, "Doubled");

//	There's an example line somewhere in the code below, unless I accidentally
//	deleted it.  You can comment it out and mess with it, if you decide you
//	want to add more pawn bitmaps and dump them.

//	I'm glad I wrote this routine, rather than just assuming that my code
//	worked.  It found a bunch of bugs.

void VDumpPawnMaps(char * szFmt, ...)
{
	va_list	pvaArg;
	char * sz;
	int	cbmMax;
	int ibm;
	
	//	Display headers and figure out how many bitmaps in the largest array.
	//
	cbmMax = 0;
	va_start(pvaArg, szFmt);
	for (sz = szFmt; *sz != '\0'; sz++) {
		char *	szArg;
		PBM	pbmArg;
		int	cbm;

		if (isdigit(*sz))
			cbm = *sz - '0';
		else {
			Assert(*sz == '!');
			cbm = 1;
		}
		if (cbm > cbmMax)
			cbmMax = cbm;					// <-- New largest.
		pbmArg = va_arg(pvaArg, PBM);		// Pull bitmap pointer.
		szArg = va_arg(pvaArg, char *);		// Pull column header.
		if (sz != szFmt)
			putchar(' ');
		printf("%-8s", szArg);				// Display arg.
	}
	putchar('\n');
	//
	//	Display the bitmaps.  The largest argument I got was "cbmMax", so I
	//	will loop that many times, displaying one row of bitmaps each time.
	//
	for (ibm = 0; ibm < cbmMax; ibm++) {
		int	rnk;

		if (ibm != 0)
			putchar('\n');
		//
		//	Display a row of bitmaps.
		//
		for (rnk = rnk8; rnk >= rnk1; rnk--) {
			//
			//	Display one rank from all the bitmaps.
			//
			va_start(pvaArg, szFmt);
			for (sz = szFmt; *sz != '\0'; sz++) {
				char *	szArg;
				PBM	pbmArg;
				int	fil;
				int	cbm;

				if (sz != szFmt)
					putchar(' ');
				cbm = (isdigit(*sz)) ? *sz - '0' : 1;
				pbmArg = va_arg(pvaArg, PBM);
				szArg = va_arg(pvaArg, char *);
				if (ibm >= cbm)			// This sometimes leaves trailing
					printf("        ");	//  spaces, but it's not worth fixing.
				else		// Display one rank from one bitmap.
					for (fil = filA; fil <= filH; fil++) {
						int	isq;
						BM	bm;

						isq = Isq64FromRnkFil(rnk, fil);
						bm.qw = (U64)1 << isq;
						if (*sz != '!')
							putchar((bm.qw & pbmArg[ibm].qw) ? '*' : '.');
						else if (bm.qw & pbmArg[coWHITE].qw)
							putchar('P');
						else
							putchar((bm.qw & pbmArg[coBLACK].qw) ? 'p' : '.');
					}
			}
			putchar('\n');
		}
	}
}

#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	I have 64-bit maps, but my "isq" value is 0..127.  That's bad, but I can
//	easily map it by using this array, which converts from a 7-bit isq to a
//	6-bit isq.

int const c_argisq64[] = {
	0,	1,	2,	3,	4,	5,	6,	7,	0,	0,	0,	0,	0,	0,	0,	0,
	8,	9,	10,	11,	12,	13,	14,	15,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	17,	18,	19,	20,	21,	22,	23,	0,	0,	0,	0,	0,	0,	0,	0,
	24,	25,	26,	27,	28,	29,	30,	31,	0,	0,	0,	0,	0,	0,	0,	0,
	32,	33,	34,	35,	36,	37,	38,	39,	0,	0,	0,	0,	0,	0,	0,	0,
	40,	41,	42,	43,	44,	45,	46,	47,	0,	0,	0,	0,	0,	0,	0,	0,
	48,	49,	50,	51,	52,	53,	54,	55,	0,	0,	0,	0,	0,	0,	0,	0,
	56,	57,	58,	59,	60,	61,	62,	63,	0,	0,	0,	0,	0,	0,	0,	0,
};

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Penalties for pawn defects.

#define	valDOUBLED_ISO	12	// Doubled isolated pawn.
#define	valDOUBLED		8	// Normal doubled pawn, not strictly isolated.
#define	valISOLATED		7	// Strictly isolated pawn (no pawns of this color
							//  on either adjacent file).

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This gets the pawn structure info from the hash table if it's there,
//	otherwise it generates it.

//	In most cases, the stuff is in the hash table.  The hash table is big
//	enough, and the pawn structures don't change that much.

//	Since the pawn computation stuff isn't done that much, I don't take
//	special pains to make sure that it's fast.  It's better to be clear and
//	accurate.  It would be very possible to generate the defect values via
//	less complicated data structures, and fewer passes over the data, but
//	the excess time shoudn't matter.

//	On the other hand, the whole thing stays flexible so it should be easy
//	to add to.

//	This routine returns values from the perspective of the side to move,
//	and stores values in the hash table from white's perspective.

//	There are a lot of other pawn features that can be detected, but doubled
//	and isolated pawns should be good enough for now.  The trick is to play
//	solidly enough that the program doesn't drop pawns a few moves over its
//	horizon.

//	An obvious addition is *anything* that would help with the passed pawn
//	problem, which is always severe in a simple program.  The program could
//	also benefit from an understanding of backwardness and artificial
//	isolation.  Additionally, the values given for doubled and isolated pawns
//	could vary depending upon where the pawns are on the board, and the
//	condition of the opponent's pawns (a doubled isolated pawn on an open file
//	is worse than one on a closed file).

//	In a heavier implementation, some of the pawn maps might end up being
//	stored in the hash table itself, which is another level of complication.

int ValPawns(PCON pcon, PSTE pste)
{
	PHASHP	phashp = &s_rghashp[pste->hashkPn & s_chashpMac];

	if (phashp->hashk != pste->hashkPn) {
		int	argval[coMAX];				// Total value of pawn features.
		BM	argbmLoc[coMAX];			// Bits set for pawns that exist.
		BM	bmDoubled;					// Bit set indicates doubled pawn.
		BM	bmIsolated;					// Bit set indicates isolated pawn.
		int	coUs;						// Loop counter.

		bmDoubled.qw = 0;
		bmIsolated.qw = 0;
		for (coUs = coWHITE; coUs <= coBLACK; coUs++) {
			BM	bmI;
			int	i;

			argval[coUs] = 0;
			//
			//	Collect locations.
			//
			argbmLoc[coUs].qw = 0;
			for (i = 0; i < pcon->argcpi[coUs]; i++) {
				PPI	ppi = &pcon->argpi[coUs][i];

				if ((ppi->pc != pcPAWN) || (ppi->fDead))
					continue;
				argbmLoc[coUs].qw |= (U64)1 << c_argisq64[ppi->isq];
			}
			//	Collect doubled pawn info.  I'm see if there is more than one
			//	pawn of a certain color on the a-file, if there is I will OR
			//	all of those pawns into the "bmDoubled" bitmap, then I'll try
			//	the b-file, and so on.
			//
			bmI.qw = (U64)0x0101010101010101;	// Start with a1...a8.
			for (;;) {
				BM	bmS;
				BM	bmT;

				//	Get all pawns of this color on this file.
				//
				bmS.qw = argbmLoc[coUs].qw & bmI.qw;
				//
				//	If more than one bit set in this map, all the pawns of
				//	this color on this file are doubled.
				//
				if (bmS.qw & (bmS.qw - 1))
					bmDoubled.qw |= bmS.qw;
				//
				//	I'm going to make a map of all the pawns on files adjacent
				//	to this file.  If there are no pawns in that map, all the
				//	pawns on this file are isolated.
				//
				bmT.qw = 0;
				if (!(bmI.qw & 0x01))
					bmT.qw |= bmI.qw >> 1;
				if (!(bmI.qw & 0x80))
					bmT.qw |= bmI.qw << 1;
				if (!(bmT.qw & argbmLoc[coUs].qw))
					bmIsolated.qw |= bmS.qw;
				//
				//	Set up to examine the next file.  0x80 is the h-file, so
				//	if we just examined it, bomb out of the loop, otherwise
				//	shift to the next file.
				//
				if (bmI.qw & 0x80)
					break;
				bmI.qw += bmI.qw;	// Examine file one to the right.
			}
		}
//#ifdef	DEBUG
//		VDumpPawnMaps("!11",
//			argbmLoc, "Loc",
//			&bmDoubled, "Doubled",
//			&bmIsolated, "Isolated");
//#endif
		for (coUs = coWHITE; coUs <= coBLACK; coUs++) {
			int	i;

			for (i = 0; i < pcon->argcpi[coUs]; i++) {
				PPI	ppi = &pcon->argpi[coUs][i];
				BM	bmLoc;

				if ((ppi->pc != pcPAWN) || (ppi->fDead))
					continue;
				bmLoc.qw = (U64)1 << c_argisq64[ppi->isq];
				if (bmLoc.qw & bmIsolated.qw) {
					if (bmLoc.qw & bmDoubled.qw)
						argval[coUs] -= valDOUBLED_ISO;
					else
						argval[coUs] -= valISOLATED;
				} else {
					if (bmLoc.qw & bmDoubled.qw)
						argval[coUs] -= valDOUBLED;
				}
			}
		}
		phashp->hashk = pste->hashkPn;
		phashp->val = argval[coWHITE] - argval[coBLACK];
	}
	return (pste->coUs == coWHITE) ? phashp->val : -phashp->val;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Zero all pawn hash memory.

void VClearHashp(void)
{
	memset(s_rghashp, 0, (s_chashpMac + 1) * sizeof(HASHP));
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	This allocates the pawn hash memory according to the max value passed in
//	by the user, then clears the table.

BOOL FInitHashp(PCON pcon, int cbMaxPawnHash)
{
	int	chashpMax;

	chashpMax = 1;
	for (;;) {
		if (chashpMax * 2 * (int)sizeof(HASHP) > cbMaxPawnHash)
			break;
		chashpMax *= 2;
	}
	VPrSendComment("%d bytes pawn hash memory", chashpMax * sizeof(HASHP));
	if ((s_rghashp = malloc(chashpMax * sizeof(HASHP))) == NULL) {
		VPrSendComment("Can't allocate pawn hash memory: %d bytes",
			chashpMax * sizeof(HASHP));
		return fFALSE;
	}
	s_chashpMac = chashpMax - 1;
	VClearHashp();
	return fTRUE;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
