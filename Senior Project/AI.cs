using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senior_Project
{
    abstract class AI
    {
        protected Board board; // Board that the AI makes moves on
        protected int aicode; // Code of the AI

        public AI(Board b, int ct)
        {
            this.board = b;
            this.aicode = ct;
        }

        public abstract void MakeMove();
    }
}
