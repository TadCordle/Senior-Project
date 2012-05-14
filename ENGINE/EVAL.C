
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

#include "engine.h"

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	These are piece-square tables.  The award bonuses (or penalties) in
//	centipawns if a piece is sitting on a particular square.  These numbers
//	are a huge part of the eval.

int	const c_argvalPos[pcMAX][coMAX][csqMAX] = {
	//
	//	White pawn.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0, // a1..h1
	2,	3,	4,	0,	0,	4,	3,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	4,	6,	12,	12,	12,	4,	6,	4,	0,	0,	0,	0,	0,	0,	0,	0,
	4,	7,	18,	25,	25,	16,	7,	4,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	11,	18,	27,	27,	16,	11,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	15,	24,	32,	32,	24,	15,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	15,	24,	32,	32,	24,	15,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0, // a8..h8
	//
	//	Black pawn.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	15,	24,	32,	32,	24,	15,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	15,	24,	32,	32,	24,	15,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	11,	18,	27,	27,	16,	11,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	4,	7,	18,	25,	25,	16,	7,	4,	0,	0,	0,	0,	0,	0,	0,	0,
	4,	6,	12,	12,	12,	4,	6,	4,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	3,	4,	0,	0,	4,	3,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	White knight.
	//
	-7,	-3,	1,	3,	3,	1,	-3,	-7,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	6,	14,	20,	20,	14,	6,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	14,	22,	26,	26,	22,	14,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	8,	18,	26,	30,	30,	26,	18,	8,	0,	0,	0,	0,	0,	0,	0,	0,
	8,	18,	30,	32,	32,	30,	18,	8,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	14,	28,	32,	32,	28,	14,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	6,	14,	20,	20,	14,	6,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	-7,	-3,	1,	3,	3,	1,	-3,	-7,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black knight.
	//
	-7,	-3,	1,	3,	3,	1,	-3,	-7,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	6,	14,	20,	20,	14,	6,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	14,	28,	32,	32,	28,	14,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	8,	18,	30,	32,	32,	30,	18,	8,	0,	0,	0,	0,	0,	0,	0,	0,
	8,	18,	26,	30,	30,	26,	18,	8,	0,	0,	0,	0,	0,	0,	0,	0,
	6,	14,	22,	26,	26,	22,	14,	6,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	6,	14,	20,	20,	14,	6,	2,	0,	0,	0,	0,	0,	0,	0,	0,
	-7,	-3,	1,	3,	3,	1,	-3,	-7,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	White bishop.
	//
	16,	16,	16,	16,	16,	16,	16,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	26,	29,	31,	31,	31,	31,	29,	26,	0,	0,	0,	0,	0,	0,	0,	0,
	26,	28,	32,	32,	32,	32,	28,	26,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	26,	32,	32,	32,	32,	26,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	26,	32,	32,	32,	32,	26,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	28,	32,	32,	32,	32,	28,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	29,	31,	31,	31,	31,	29,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	16,	16,	16,	16,	16,	16,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black bishop.
	//
	16,	16,	16,	16,	16,	16,	16,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	29,	31,	31,	31,	31,	29,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	28,	32,	32,	32,	32,	28,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	26,	32,	32,	32,	32,	26,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	26,	32,	32,	32,	32,	26,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	26,	28,	32,	32,	32,	32,	28,	26,	0,	0,	0,	0,	0,	0,	0,	0,
	26,	29,	31,	31,	31,	31,	29,	26,	0,	0,	0,	0,	0,	0,	0,	0,
	16,	16,	16,	16,	16,	16,	16,	16,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	White rook.
	//
	0,	0,	0,	3,	3,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	10,	10,	10,	10,	10,	10,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black rook.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	10,	10,	10,	10,	10,	10,	10,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	0,	0,	0,	0,	0,	0,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	3,	3,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	White queen.
	//
	-2,	-2,	-2,	0,	0,	-2,	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	2,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	2,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black queen.
	//
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	2,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	2,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	-2,	-2,	-2,	0,	0,	-2,	-2,	-2,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	White king.
	//
	3,	3,	8,	-12,-8,	-12,10,	5,	0,	0,	0,	0,	0,	0,	0,	0,
	-5,	-5,	-12,-12,-12,-12,-5,	-5,	0,	0,	0,	0,	0,	0,	0,	0,
	-7,	-15,-15,-15,-15,-15,-15,-7,	0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black king.
	//
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-20,-20,-20,-20,-20,-20,-20,-20,0,	0,	0,	0,	0,	0,	0,	0,
	-7,	-15,-15,-15,-15,-15,-15,-7,	0,	0,	0,	0,	0,	0,	0,	0,
	-5,	-5,	-12,-12,-12,-12,-5,	-5,	0,	0,	0,	0,	0,	0,	0,	0,
	3,	3,	8,	-12,-8,	-12,10,	5,	0,	0,	0,	0,	0,	0,	0,	0,
};

//	The king is a weird piece because in the engame its eval is approximately
//	opposite of its eval in the middlegame or opening.  At the start of the
//	game, you want to hide your king in the corner, and at the end, you want
//	to run to the middle.

//	This problem is taken care of by having two tables.  If you try to make
//	things work with the middlegame table, the program will play like crap in
//	endings.

int	const c_argvalEKing[coMAX][csqMAX] = {
	//
	//	White king.
	//
	0,	0,	1,	2,	2,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	2,	4,	5,	5,	4,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	6,	7,	7,	6,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	10,	10,	10,	10,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	12,	15,	15,	12,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	7,	10,	12,	12,	10,	7,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	2,	4,	5,	5,	4,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black king.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	2,	4,	5,	5,	4,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	7,	10,	12,	12,	10,	7,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	12,	15,	15,	12,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	10,	10,	10,	10,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	4,	6,	7,	7,	6,	4,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	2,	4,	5,	5,	4,	2,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	1,	2,	2,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
};

#define	sideK	0
#define	sideC	1
#define	sideQ	2

#define	sideMAX	3

//	I didn't use the #defines because that is hard to format.  This is an
//	array of char instead of int, which is a decision based upon space.
//
//	Note that c1 is considered a q-side square but f1 is not considered a
//	k-side square.  I want to do everything possible to:
//
//	1.	Convince the program to castle, and O-O-O, which puts the king on
//		c1, is a fine thing to do.
//
//	2.	Prevent the program from playing Kf1.  f1 is not a good square for
//		the king, and I'd rather it play O-O if it's going to move its king
//		into the k-side corner.

char const c_argside[csqMAX] = {
	2,	2,	2,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	2,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	1,	1,	1,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	1,	1,	1,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	1,	1,	1,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	1,	1,	1,	1,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	2,	1,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	2,	2,	2,	1,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
};

//	These are edits to the pawn positional values based upon king location.
//	This one assumes that we're dealing with a white king on the q-side.
//	Non-zero values will replace the values in the regular pawn table.

char const c_argvalShelter[csqMAX] = {
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	35,	35,	12,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	20,	20,	12,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	15,	15,	18,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	10,	11,	18,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
};

int	c_argvalPawn[sideMAX][coMAX][csqMAX];

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VInitMiddlegamePawnTable(void)
{
	int	side;
	
	for (side = sideK; side <= sideQ; side++) {
		int	co;

		for (co = coWHITE; co <= coBLACK; co++) {
			memcpy(c_argvalPawn[side][co], c_argvalPos[pcPAWN][co],
				csqMAX * sizeof(int));
			if (side != sideC) {
				BOOL fFlipFil = (side == sideK);
				BOOL fFlipRnk = (co == coBLACK);
				int	rnk;
				int	fil;

				for (rnk = rnk2; rnk <= rnk7; rnk++)
					for (fil = filA; fil <= filH; fil++) {
						int	isq = IsqFromRnkFil(rnk, fil);

						if (c_argvalShelter[isq] != 0) {
							int	rnkEdit = (fFlipRnk) ? 7 - rnk : rnk;
							int	filEdit = (fFlipFil) ? 7 - fil : fil;
							int	isqEdit = IsqFromRnkFil(rnkEdit, filEdit);

							c_argvalPawn[side][co][isqEdit] =
								c_argvalShelter[isq];
						}
					}
			}
		}
	}
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

int const c_argdsqKing[coMAX][csqMAX] = {
	//
	//	White king.
	//
	//	b	c	d	e	f	g	h
	//
	0,	-1,	-1,	0,	0,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black king.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	-1,	-1,	0,	0,	1,	1,	0,	0,	0,	0,	0,	0,	0,	0,	0,
};

int const c_argdsqMinor[coMAX][csqMAX] = {
	//
	//	White minor.
	//
	//	b	c	d	e	f	g	h
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	-16,-16,0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	//
	//	Black minor.
	//
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	16,	16,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,
};

//	This eval function evaluates:
//
//	1.	Pawn structure, which is hashed and handled in another module.
//	2.	Piece location, which is usually related to central occupation or
//		in the case of pawns, simple advancement.
//	3.	Minors blocking d2/e2 pawns.
//	4.	King on f1/g1 with rook on g1/h1.
//	5.	Pawn shelter.
//	6.	Endgame king position values versus middlegame values.
//
//	Cases 3 and 4 are surprisingly important.
//
//	The piece-square tables are designed to induce the program to castle
//	behind a safe pawn structure, to move its central pawns, and to develop
//	its pieces.
//
//	There is also a chance that the program would feel an interest in breaking
//	its opponent's king position, but I don't have high hopes for this.

int ValEval(PCON pcon, PSTE pste)
{
	int	argval[coMAX];
	int	co;
	int	val;

	//	Get value for pawn structure.
	//
	val = ValPawns(pcon, pste);
	//
	//	Get material value.
	//
	val += pste->valPcUs + pste->valPnUs;
	val -= pste->valPcThem + pste->valPnThem;
	//
	//	Cut off if it appears that this is futile.
	//
	if (val - 300 >= pste->valBeta)		// We're already great?
		return pste->valBeta;
	if (val + 300 <= pste->valAlpha)	// We're already toast?
		return pste->valAlpha;
	//
	//	Otherwise loop through the pieces.
	//
	for (co = coWHITE; co <= coBLACK; co++) {
		int * rgvalPawn;
		int	i;

		//	This selects the appropriate pawn shelter piece-square table,
		//	depending upon where our king is, or if it's the middlegame.
		//
		if (pste->valPcThem <= 2 * valROOK + valMINOR)
			rgvalPawn = c_argvalPawn[sideC][co];
		else
			rgvalPawn = c_argvalPawn[c_argside[
				pcon->argpi[co][ipiKING].isq]][co];
		//
		//	Loop through the pieces, perhaps doing some special case code for
		//	some types of pieces, but otherwise simply summing piece-square
		//	values.
		//
		argval[co] = 0;
		for (i = 0; i < pcon->argcpi[co]; i++) {
			PPI	ppi = &pcon->argpi[co][i];

			if (!ppi->fDead) {
				int	dsq;

				switch (ppi->pc) {
				case pcPAWN:
					argval[co] += rgvalPawn[ppi->isq];
					break;
				case pcKING:
					if (pste->valPcThem <= 2 * valROOK + valMINOR)
						argval[co] += c_argvalEKing[ppi->co][ppi->isq];
					else {
						argval[co] += c_argvalPos[ppi->pc][ppi->co][ppi->isq];
						//
						//	This loop figures out if a king has been kicked to
						//	f1 or g1, and if so, it checks to see if the rook
						//	is still to the right of the king.  This is a big
						//	penalty.
						//
						//	It does the same thing on the q-side, mirrored.
						//
						if ((dsq = c_argdsqKing[co][ppi->isq]) != 0) {
							int	isq = ppi->isq + dsq;
							
							for (;;) {
								if ((pcon->argsq[isq].ppi != NULL) &&
									(pcon->argsq[isq].ppi->pc == pcROOK) &&
									(pcon->argsq[isq].ppi->co == co))
									argval[co] -= 30;
								isq += dsq;
								if (isq & 0x88)
									break;
							}
						}
					}
					break;
				case pcBISHOP:
				case pcKNIGHT:
					argval[co] += c_argvalPos[ppi->pc][ppi->co][ppi->isq];
					//
					//	Computers are by default very stupid about putting
					//	bishops and knights on d3 and e3 while there is still
					//	a pawn on d2 or e2.  I will detect this case and
					//	penalize heavily.
					//
					if ((dsq = c_argdsqMinor[co][ppi->isq]) != 0) {
						int	isq = ppi->isq + dsq;

						if ((pcon->argsq[isq].ppi != NULL) &&
							(pcon->argsq[isq].ppi->pc == pcPAWN) &&
							(pcon->argsq[isq].ppi->co == co))
							argval[co] -= 30;
					}
					break;
				default:
					argval[co] += c_argvalPos[ppi->pc][ppi->co][ppi->isq];
					break;
				}
			}
		}
	}
	val += argval[pste->coUs];
	val -= argval[pste->coUs ^ 1];
	return val;
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
