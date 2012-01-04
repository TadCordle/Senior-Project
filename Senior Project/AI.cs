
namespace Senior_Project
{
    public abstract class AI
    {
        protected Board board; // Board that the AI makes moves on
        protected int aicode; // Code of the AI

        protected AI(Board b, int ct)
        {
            this.board = b;
            this.aicode = ct;
        }

        public abstract void MakeMove();
    }
}
