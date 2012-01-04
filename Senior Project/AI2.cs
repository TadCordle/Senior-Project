using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senior_Project
{
    class AI2 : AI
    {
        #region Variables
        int maxX, maxY; // Bounds of the board
        int othercode; // AI's code and the opponent code
        int[,] positioncheck = new int[,] { { 0, 1 }, { 1, 1 }, { 1, 0 }, { 1, -1 }, 
                                     { 0, -1 }, { -1, -1 }, { -1, 0 }, { -1, 1 }, 
                                     { 0, 2 }, { 2, 2 }, { 2, 0 }, { 2, -2 }, 
                                     { 0, -2 }, { -2, -2 }, { -2, 0 }, {-2, 2 } }; // Relative move positions

        #endregion

        // Constructor
        public AI2(Board b, int ct) : base(b, ct)
        {
            maxX = b.board.GetUpperBound(0);
            maxY = b.board.GetUpperBound(1);
            othercode = ct == 1 ? 2 : 1;
        }

        // Makes the AI choose and execute a move
        public override void MakeMove()
        {
            Move bestMove;

            // Find a list of moves that make the most conversions
            List<Move> bestOffensiveMoves = FindBestOffensiveMoves(aicode);
            if (bestOffensiveMoves.Count == 0)
                return;

            // If there are more than one best moves, find the most defensive one
            if (bestOffensiveMoves.Count > 1)
            {
                List<Move> bestDefensiveMoves = FindBestDefensiveMoves(bestOffensiveMoves);

                // Take a random move from final list
                bestMove = bestDefensiveMoves[(new Random()).Next(bestDefensiveMoves.Count)];
            }
            else
                bestMove = bestOffensiveMoves[0];

            // If no great gain is made, block enemy's best move
            if (bestMove.gain == 1 && !bestMove.isjump)
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

        // Makes a list of the best offensive moves player of code 'code' can make
        private List<Move> FindBestOffensiveMoves(int code)
        {
            int ocode = code == 1 ? 2 : 1;
            List<Move> list = new List<Move>();
            int greatest = -1;
            bool jump = false;

            // Go through all possible moves of each pieve of code 'code' on the board
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

                        // Count conversions
                        int gain = board.Convert(gp.x + positioncheck[i, 0], gp.y + positioncheck[i, 1], ocode, ocode);
                        if (i < 8) gain += 1;

                        // Save best move and add it to a list
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

        // Takes the most defensive moves from a list of moves
        private List<Move> FindBestDefensiveMoves(List<Move> l)
        {
            List<Move> bestDefensiveMoves = new List<Move>();

            int lost = 0;
            int worstloss = 9;

            // Go through all the moves in the list
            foreach (Move m in l)
            {
                if (m.isjump)
                {
                    // Check number of pieces adjacent to open spot if move is a jump
                    for (int i = 0; i < 16; i++)
                    {
                        if (!(m.xfrom + positioncheck[i, 0] <= maxX && m.xfrom + positioncheck[i, 0] >= 0 &&
                              m.yfrom + positioncheck[i, 1] <= maxX && m.yfrom + positioncheck[i, 1] >= 0))
                            continue;

                        if (board[m.xfrom + positioncheck[i, 0], m.yfrom + positioncheck[i, 1]].code == othercode)
                        {
                            lost = board.Convert(m.xfrom, m.yfrom, aicode, aicode);
                            break;
                        }
                    }
                    // Save move with smallest loss
                    if (lost <= worstloss)
                    {
                        if (lost < worstloss)
                            bestDefensiveMoves.RemoveRange(0, bestDefensiveMoves.Count);
                        bestDefensiveMoves.Add(m);
                        worstloss = lost;
                    }
                }
                else
                    // Add move if a jump isn't made
                    bestDefensiveMoves.Add(m);
            }

            return bestDefensiveMoves;
        }

        // Executes a move to the board
        private void ExecuteMove(Move move)
        {
            if (move.isjump)
                board[move.xfrom, move.yfrom].code = 0;
            board[move.xto, move.yto].code = aicode;
            board.Convert(move.xto, move.yto, aicode, othercode);
        }

        // Class representing a move
        private struct Move
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
