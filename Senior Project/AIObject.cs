using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senior_Project
{
    class AIObject
    {
        #region Variables

        Board board;
        int maxX, maxY;
        int thiscode, othercode;
        int[,] positioncheck = new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, 
                                     { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, 
                                     { 0, 2 }, { 2, 2 }, { 2, 0 }, { 2, -2 }, 
                                     { 0, -2 }, { -2, -2 }, { -2, 0 }, {-2, 2 } };

        #endregion

        public AIObject(Board b, int ct)
        {
            board = b;
            maxX = b.board.GetUpperBound(0);
            maxY = b.board.GetUpperBound(1);
            thiscode = ct;
            othercode = thiscode == 1 ? 2 : 1;
        }

        public void MakeMove()
        {
            Move bestMove;

            List<Move> bestOffensiveMoves = FindBestOffensiveMoves(thiscode);
            if (bestOffensiveMoves.Count == 0)
                return;

            if (bestOffensiveMoves.Count > 1)
            {
                List<Move> bestDefensiveMoves = new List<Move>();

                int lost = 0;
                int worstloss = 9;
                foreach (Move m in bestOffensiveMoves)
                {
                    if (m.isjump)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            if (!(m.xfrom + positioncheck[i, 0] <= maxX && m.xfrom + positioncheck[i, 0] >= 0 &&
                                  m.yfrom + positioncheck[i, 1] <= maxX && m.yfrom + positioncheck[i, 1] >= 0))
                                continue;

                            if (board[m.xfrom + positioncheck[i, 0], m.yfrom + positioncheck[i, 1]].code == othercode)
                            {
                                lost = board.Convert(m.xfrom, m.yfrom, thiscode, thiscode);
                                break;
                            }
                        }
                        if (lost <= worstloss)
                        {
                            if (lost < worstloss)
                                bestDefensiveMoves.RemoveRange(0, bestDefensiveMoves.Count);
                            bestDefensiveMoves.Add(m);
                            worstloss = lost;
                        }
                    }
                    else
                        bestDefensiveMoves.Add(m);
                }
                bestMove = bestDefensiveMoves[(new Random()).Next(bestDefensiveMoves.Count)];
            }
            else
                bestMove = bestOffensiveMoves[0];

            if (bestMove.gain == 1)
            {
                List<Move> bestEnemyMoves = FindBestOffensiveMoves(othercode);
                foreach (Move m in bestEnemyMoves)
                {
                    if (m.gain >= 1 && m.isjump)
                        bestMove = new Move(0, 0, m.xto, m.yto, false, 1);
                }
            }

            ExecuteMove(bestMove);
        }

        private List<Move> FindBestOffensiveMoves(int code)
        {
            int ocode = code == 1 ? 2 : 1;
            List<Move> list = new List<Move>();
            int greatest = -1;
            bool jump = false;

            foreach (GamePiece gp in board.board)
            {
                if (gp.code == code)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        if (gp.x + positioncheck[i, 0] <= maxX && gp.x + positioncheck[i, 0] >= 0 &&
                            gp.y + positioncheck[i, 1] <= maxY && gp.y + positioncheck[i, 1] >= 0)
                        {
                            if (board[gp.x + positioncheck[i, 0], gp.y + positioncheck[i, 1]].code != 0)
                                continue;
                        }
                        else
                            continue;

                        int gain = board.Convert(gp.x + positioncheck[i, 0], gp.y + positioncheck[i, 1], ocode, ocode);
                        if (i < 8) gain += 1;

                        if (gain >= greatest)
                        {
                            if (gain == greatest && !jump)
                                continue;

                            if (gain > greatest)
                                list.RemoveRange(0, list.Count);
                            if (i >= 8)
                                jump = true;
                            greatest = gain;
                            list.Add (new Move(gp.x, gp.y, gp.x + positioncheck[i, 0], gp.y + positioncheck[i, 1], i >= 8, gain));
                        }
                    }
                }
            }

            return list;
        }

        private void ExecuteMove(Move move)
        {
            if (move.isjump)
                board[move.xfrom, move.yfrom].code = 0;
            board[move.xto, move.yto].code = thiscode;
            board.Convert(move.xto, move.yto, thiscode, othercode);
        }

        private class Move
        {
            public int xfrom, yfrom, xto, yto, gain;
            public bool isjump;

            public Move(int xf, int yf, int xt, int yt, bool jump, int g)
            {
                xfrom = xf;
                yfrom = yf;
                xto = xt;
                yto = yt;
                isjump = jump;
                gain = g;
            }
        }
    }
}
