using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senior_Project
{
	class Tad_MiniMaxAI : AI
	{
		private GameTree tree;
		
		public Tad_MiniMaxAI(Board b, int c) 
			: base(b, c)
		{
		}

		public override void MakeMove()
		{
			tree = new GameTree(board, aicode);
			int max = int.MinValue;
			Move bestMove = new Move(0, 0, 0, 0, false);
			foreach (GameTree.Node n in tree.root.children.Values)
				if (n.value > max)
				{
					max = n.value;
					bestMove = n.move;
				}
			ExecuteMove(board, aicode, bestMove);
		}

		// Executes a move to the board
		public static int ExecuteMove(Board b, int c, Move move)
		{
			if (move.isjump)
				b[move.xfrom, move.yfrom] = 0;
			b[move.xto, move.yto] = c;
			return b.Convert(move.xto, move.yto, c, c == 1 ? 2 : 1) + (move.isjump ? 0 : 1);
		}
	}

	class GameTree
	{
		public Node root;
		public int thiscode;
		public int othercode;
		const int MAXDEPTH = 3;
		int[][] positioncheck = new int[][] {
									new[] { 0, 1 },  new[] { 1, 1 },   new[] { 1, 0 },  new[] { 1, -1 }, 
                                    new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, 
                                    new[] { 0, 2 },  new[] { 2, 2 },   new[] { 2, 0 },  new[] { 2, -2 }, 
                                    new[] { 0, -2 }, new[] { -2, -2 }, new[] { -2, 0 }, new[] {-2, 2 } }; // Relative move positions

		public GameTree(Board workingBoard, int code)
		{
			thiscode = code;
			othercode = code == 1 ? 2 : 1;
			root = new Node(workingBoard);
			BuildTree(0, root, code);
		}

		private void BuildTree(int level, Node current, int code)
		{
			// If looked enough into the future or hit a terminal state, calculate value
			//	and return
			if (level == MAXDEPTH || current.state.GameOver)
			{
				int thisCount = current.state.Count(thiscode);
				int otherCount = current.state.Count(othercode);
				if (current.state.GameOver)
					if (thisCount > otherCount)
						current.value = int.MaxValue;
					else
						current.value = int.MinValue;
				return;
			}

			// Find all possible moves for code on the working board
			var ownedPieces = from GamePiece g in current.state
							  where g.Code == code
							  select g;
			int gain = 0;
			foreach (GamePiece gp in ownedPieces)
			{
				for (int i = 0; i < 16; i++)
				{
					// Make sure move is valid
					if (gp.x + positioncheck[i][0] < Board.SIZE_X && gp.x + positioncheck[i][0] >= 0 &&
						gp.y + positioncheck[i][1] < Board.SIZE_Y && gp.y + positioncheck[i][1] >= 0)
					{
						if (current.state[gp.x + positioncheck[i][0], gp.y + positioncheck[i][1]] != 0)
							continue;
					}
					else
						continue;

					// Execute move on working board and save move and state
					Move move = new Move(gp.x, gp.y, gp.x + positioncheck[i][0], gp.y + positioncheck[i][1], i >= 8);
					Board transform = (Board)current.state.Clone();
					gain = Tad_MiniMaxAI.ExecuteMove(transform, code, move) * code == thiscode ? 1 : -1; // Calculate gain / loss made by move

					Node newnode = new Node(transform);
					newnode.move = move;

					current.children[move] = newnode;
				}
			}

			// Increase level and add children
			level++;
			foreach (Node n in current.children.Values)
				BuildTree(level, n, code == 1 ? 2 : 1);

			int max = int.MinValue;
			foreach (Node n in current.children.Values)
				if (n.value > max)
					max = n.value;
			current.value = gain + max;
		}

		public sealed class Node
		{
			public Board state;
			public int value;
			public Move move;
			public Dictionary<Move, Node> children;

			public Node(Board state)
			{
				this.state = state;
				children = new Dictionary<Move, Node>();
			}
		}
	}

	// Class representing a move
	class Move
	{
		public int xfrom, yfrom, xto, yto;
		public bool isjump;

		public Move(int xf, int yf, int xt, int yt, bool jump)
		{
			xfrom = xf;
			yfrom = yf;
			xto = xt;
			yto = yt;
			isjump = jump;
		}
	}
}
