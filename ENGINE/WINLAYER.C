
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
#include <windows.h>

#ifdef	DEBUG
static char const s_aszModule[] = __FILE__;
#endif

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

//	Returns system time in milliseconds.  If you are porting this away from
//	windows, this might cause you minor trouble, but you should be able to
//	find something.  High clock resolution is a plus, but if you don't have
//	it it's not absolutely fatal.

unsigned TmNow(void)
{
	return GetTickCount();
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VReseed(void)
{
	SYSTEMTIME	st;

	GetSystemTime(&st);
	srand((st.wHour ^ st.wMinute ^ st.wSecond ^ st.wMilliseconds) & 0x7FFF);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VGetIniFile(char * szOut, int cbOut)
{
	int	i;

	GetModuleFileName(GetModuleHandle(NULL), szOut, cbOut);
	for (i = strlen(szOut) - 1; i >= 0; i--)
		if (szOut[i] == '\\') {
			strcpy(szOut + i + 1, "gerbil.ini");
			break;
		}
	Assert(i >= 0);
}

static char const s_aszApp[] = "settings";

void VGetProfileSz(const char * szKey, const char * szDefault,
	char * szOut, int cbOut)
{
	char	aszPath[512];

	VGetIniFile(aszPath, sizeof(aszPath));
	GetPrivateProfileString(s_aszApp, szKey, szDefault,
		szOut, cbOut, aszPath);
}

int IGetProfileInt(const char * szKey, int iDefault)
{
	char	aszPath[512];

	VGetIniFile(aszPath, sizeof(aszPath));
	return GetPrivateProfileInt(s_aszApp, szKey, iDefault, aszPath);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-

void VLowPriority(void)
{
	SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_BELOW_NORMAL);
}

//	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-	-
