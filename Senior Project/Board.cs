using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Senior_Project
{
	public sealed class Board : IEnumerable<GamePiece>, ICloneable
	{
		private static readonly long[][] hashkey = new long[SIZE_X * SIZE_Y][];
		public const int SIZE_X = 10, SIZE_Y = 10;

		private GamePiece[][] board;
		private long boardHash;

		public long LongHashCode { get { return boardHash; } }

		public bool GameOver
		{
			get
			{
				return this.Count(0) == 0 ||
					!this.HasMovesLeft(1) ||
					!this.HasMovesLeft(2);
			}
		}

		static Board()
		{
			Random r = new Random();

			for (int i = 0; i < SIZE_X * SIZE_Y; i++)
			{
				hashkey[i] = new long[4];

				for (int x = 0; x < 4; x++)
					hashkey[i][x] = r.Next() ^ ((long) r.Next() << 15) ^ ((long) r.Next() << 30) ^
						((long) r.Next() << 45) ^ ((long) r.Next() << 60);
			}
		}

		// Constructors
		public Board(string file)
			: this()
		{
			// Load the board from a text file
			StreamReader load;
			if (File.Exists(file))
				load = new StreamReader(file);
			else
			{
				MessageBox.Show("Load failed because the file " + file + " was not found.");
				return;
			}

			try
			{
				for (int i = 0; i < SIZE_X; i++)
				{
					string line = load.ReadLine();
					for (int j = 0; j < SIZE_Y; j++)
						board[i][j] = new GamePiece(i, j, int.Parse(line[j].ToString()));
				}
			}
			finally
			{
				load.Dispose();
				boardHash = this.GetLongHashCode();
			}
		}
		public Board()
		{
			board = new GamePiece[SIZE_X][];
			for (int i = 0; i < SIZE_X; i++)
			{
				board[i] = new GamePiece[SIZE_Y];
				for (int j = 0; j < SIZE_Y; j++)
					board[i][j] = new GamePiece(i, j, 0);
			}

			boardHash = this.GetLongHashCode();
		}

		public int this[int posr, int posc]
		{
			get { return board[posr][posc].Code; }
			set {
				// Unset old code, set new code.
				this.boardHash ^= hashkey[posr * Board.SIZE_Y + posc][board[posr][posc].Code];
				this.boardHash ^= hashkey[posr * Board.SIZE_Y + posc][value];

				board[posr][posc].Code = value;
			}
		}

		public GamePiece GetPieceAtPos(int posr, int posc)
		{
			return board[posr][posc];
		}

		// Draw the game board
		internal void Draw(Graphics g)
		{
			if (g == null)
				return;

			for (int i = 0; i < SIZE_Y; i++)
				for (int j = 0; j < SIZE_X; j++)
					if (board[j][i].Sprite != null)
						g.DrawImage(board[j][i].Sprite, IndexToCoord(j) + 1, IndexToCoord(i) + 1, 47, 47);
		}

		// Return the board's selected piece
		public GamePiece SelectedPiece()
		{
			foreach (GamePiece g in this)
				if (g.Selected)
					return g;

			return null;
		}

		// Change all spaces adjacent to space (x, y) with code c2 to code c1
		public int Convert(int x, int y, int c1, int c2)
		{
			int count = 0;

			if (c1 == c2)
			{
				for (int i = -1, xi = x + i; i <= 1; i++, xi = x + i)
					for (int j = -1, yj = y + j; j <= 1; j++, yj = j + y)
						if (xi < SIZE_X && xi >= 0 && yj < SIZE_Y && yj >= 0)
							if (board[xi][yj].Code == c2)
								count++;
			}
			else
			{
				for (int i = -1, xi = x + i; i <= 1; i++, xi = x + i)
					for (int j = -1, yj = y + j; j <= 1; j++, yj = j + y)
						if (xi < SIZE_X && xi >= 0 && yj < SIZE_Y && yj >= 0)
							if (board[xi][yj].Code == c2)
							{
								this.boardHash ^= hashkey[xi * Board.SIZE_Y + yj][c2];
								this.boardHash ^= hashkey[xi * Board.SIZE_Y + yj][c1];

								board[x + i][y + j].Code = c1;
								count++;
							}
			}

			return count;
		}

		// Return a count of all pieces on the board with code c
		public int Count(int c)
		{
			int count = 0;
			foreach (GamePiece gp in this)
				if (gp.Code == c)
					count++;
			return count;
		}

		// Checks if there are any pieces with code c that can move
		public bool HasMovesLeft(int c)
		{
			foreach (GamePiece gp in this)
			{
				if (gp.Code != c) continue;

				for (int i = -2; i <= 2; i++)
					for (int j = -2; j <= 2; j++)
						if (gp.MoveIsInRange(gp.x + i, gp.y + j))
							if (gp.x + i < SIZE_X && gp.x + i >= 0 && gp.y + j < SIZE_Y && gp.y + j >= 0)
								if (board[gp.x + i][gp.y + j].Code == 0)
									return true;
			}
			return false;
		}

		// Moves the selected piece and returns a value representing whether or not the move failed
		public bool MoveSelectedPiece(int xto, int yto, int pcode)
		{
			GamePiece gp = this.SelectedPiece();
			if (gp == null || gp.Code != pcode)
				return false;

			// Move or duplicate the selected piece
			if (!gp.MoveIsInRange(xto, yto) || board[xto][yto].Code != 0)
				return false;
			else
			{
				this[xto, yto] = pcode;
				// If position is far enough away, jump instead of duplicating
				if (!(Math.Abs(xto - gp.x) <= 1 && Math.Abs(yto - gp.y) <= 1))
					this[gp.x, gp.y] = 0;
				this.Convert(xto, yto, pcode, (pcode == 1 ? 2 : 1));
			}

			return true;
		}

		// Fills all of the empty spaces in a board with the pieces that can move there
		public void FillEmptySpaces(int code)
		{
			foreach (GamePiece g in this)
				if (g.Code == code)
					DuplicatePiece(g);
		}
		private void DuplicatePiece(GamePiece gp)
		{
			for (int i = -2; i <= 2; i++)
				for (int j = -2; j <= 2; j++)
				{
					if (i == 0 && j == 0 || !gp.MoveIsInRange(gp.x + i, gp.y + j))
						continue;

					if (gp.x + i < SIZE_X && gp.x + i >= 0 && gp.y + j < SIZE_Y && gp.y + j >= 0)
						if (board[gp.x + i][gp.y + j].Code == 0)
						{
							this[gp.x + i, gp.y + j] = gp.Code;
							DuplicatePiece(board[gp.x + i][gp.y + j]);
						}
				}
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
				return false;
			if (object.ReferenceEquals(this, obj))
				return true;
			if (!(obj is Board))
				return false;

			var b2 = obj as Board;

			if (b2.boardHash != this.boardHash)
				return false;

			for (int x = 0; x < Board.SIZE_X; x++)
				for (int y = 0; y < Board.SIZE_Y; y++)
					if (b2.board[x][y] != this.board[x][y])
						return false;

			return true;
		}
		public override int GetHashCode()
		{
			return (int) boardHash ^ (int) (boardHash >> 32);
		}
		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			for (int c = 0; c < Board.SIZE_Y; c++)
			{
				for (int r = 0; r < Board.SIZE_X; r++)
					sb.Append(board[r][c].Code);
				sb.AppendLine();
			}
			return sb.ToString();
		}
		public long GetLongHashCode()
		{
			long code = 0L;

			for (int r = 0; r < Board.SIZE_X; r++)
				for (int c = 0; c < Board.SIZE_Y; c++)
					code ^= Board.hashkey[r * Board.SIZE_Y + c][board[r][c].Code];

			return code;
		}

		// Conversions used for drawing purposes
		#region Conversions
		private const int MAX_INDEX = int.MaxValue / 48;

		// Converts array coordinates to pixel coordinates
		public static int IndexToCoord(int n)
		{
			if (n >= MAX_INDEX)
				throw new ArgumentException("Resultant pixel coordinate would overflow int.", "n");

			return (n * 48);
		}

		// Converts pixel coordinates to array coordinates
		public static int CoordToIndex(int n)
		{
			return (int) (n / 48);
		}

		#endregion

		#region Multidimensional->jagged compensation.
		public IEnumerator<GamePiece> GetEnumerator()
		{
			foreach (var row in board)
				foreach (var gp in row)
					yield return gp;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region ICloneable Members

		public object Clone()
		{
			Board b = new Board();
			for (int r = 0; r < SIZE_Y; r++)
				for (int c = 0; c < SIZE_X; c++)
					b.board[r][c].Code = this.board[r][c].Code;

			b.boardHash = this.boardHash;
			return b;
		}

		#endregion
	}
}