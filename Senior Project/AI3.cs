using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Senior_Project
{
	sealed class ICanSeeForever : AI
	{
        private const int SEARCH_DEPTH = 4;

		private static readonly int[][] positioncheck = new int[][] {
			new[] { 0, 1 },  new[] { 1, 1 },   new[] { 1, 0 },  new[] { 1, -1 }, 
            new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, 
            new[] { 0, 2 },  new[] { 2, 2 },   new[] { 2, 0 },  new[] { 2, -2 }, 
            new[] { 0, -2 }, new[] { -2, -2 }, new[] { -2, 0 }, new[] {-2, 2 } };

		private Tree<Move, Board> gameTree = new Tree<Move, Board>();
        private Dictionary<Board, StateInfo> transTable = new Dictionary<Board, StateInfo>(4096);
		private HashSet<Board> repetitionCheck = new HashSet<Board>();
		private Stack<List<BoardTransform>> undoStack = new Stack<List<BoardTransform>>();

		private Random rnd = new Random();

		private Board workingBoard;
        private DateTime startT;

#if DEBUG
        private AI3DebugForm debug;

        private long nodeCnt = 0, transHits = 0, transCuts = 0, evalCalls = 0;
        private DateTime depthStart;
#endif

		#region Variables
		int othercode;
		#endregion

		// Constructor
		public ICanSeeForever(Board b, int ct)
			: base(b, ct)
		{
			othercode = ct == 1 ? 2 : 1;

#if DEBUG
            debug = new AI3DebugForm();
            debug.Show();
#endif
		}

        // Destructor
#if DEBUG
        ~ICanSeeForever()
        {
            if (!debug.IsDisposed)
                if (debug.InvokeRequired)
                    debug.Invoke(new Action(delegate() { debug.Close(); }));
                else
                    debug.Close();
        }
#endif

		// Makes the AI choose and execute a move
		public override void MakeMove()
		{
			//transTable.Clear();
			repetitionCheck.Clear();

			// Set up root of tree.
			this.gameTree.Root = new Tree<Move, Board>.Node();
            this.workingBoard = new Board(board);

			// XXX: TODO - Make this more iterative.
			int best = int.MinValue;
			Move move = null;
            startT = DateTime.Now;
			for (int d = 1; d <= SEARCH_DEPTH; d++)
			{
				// Initialize state.
                List<Move> pv = new List<Move>();
#if DEBUG
                nodeCnt = 0; transHits = 0; transCuts = 0; evalCalls = 0;
                depthStart = DateTime.Now;
#endif

                // Perform bounded depth-first search for the best move score.
				best = -_search(this.gameTree.Root, d, -int.MaxValue, int.MaxValue, true, pv);

				// WE GOT CALLED TO MAKE A MOVE BUT HAVE NO MOVES TO MAKE. WAT.
				if (this.gameTree.Root.Count == 0)
					return;

                if(pv.Count > 0) 
                    move = pv[0];
#if DEBUG
                _trace("[SEARCH] d={0}, n={1}, tHit={2}, tCut={3} ({4}%), e={5}", d, nodeCnt, transHits, transCuts,
                    Math.Round((double) transCuts / nodeCnt, 2) * 100, evalCalls);
                _trace("         t={0}s ({1} n/s), PV={2}", Math.Round((DateTime.Now - depthStart).TotalMilliseconds / 1000, 3),
                    Math.Round(nodeCnt / ((DateTime.Now - depthStart).TotalMilliseconds / 1000), 0),
                    pv.Aggregate("", new Func<string,Move,string>((a, b) => a + b + " ")).Trim());
#endif
			}

            Debug.Assert(move != null, "No move! Endgame condition without our notification?");

            _trace("[EXEC] {0}", move);
			_executeMove(this.board, move, aicode, othercode);
		}

		// Performs an iterative deepening depth-first search, combining the concepts of
		// minimax with alpha-beta elimination and some memoization to efficiently compute
		// the net gain/loss of any given series of moves.
		//
		// α is the minimum score that the maximizing player is assured of.
		// β is the maximum score that the minimizing player is assured of.
		private int _search(Tree<Move, Board>.Node node, int depth, int α, int β, bool me, List<Move> pv)
		{
			int pcode = me ? aicode : othercode, notpcode = !me ? aicode : othercode;
			StateInfo.ValueType valType = StateInfo.ValueType.Alpha;
			List<Move> moves = new List<Move>();
			Move cachedBest = null;

			// Check to see if we already explored this node currently, return draw score.
			if (this.repetitionCheck.Contains(this.workingBoard))
				return 0;

			// If we have a cached answer for the score of the current state at a certain depth,
			// see if we can use it.
			if (this.transTable.ContainsKey(this.workingBoard))
			{
				var val = this.transTable[this.workingBoard];

				if (val.CodeToMove == pcode)
				{
					if (val.Depth >= depth)
					{
						switch (val.Flag)
						{
							case StateInfo.ValueType.Exact:
								// If this was an exact value, we can return it no matter what.
								node.Value = val.Value;

#if DEBUG
                                transHits++; // Mark a transposition table hit.
                                transCuts++;
#endif

								return val.Value;
							case StateInfo.ValueType.Alpha:
								// Since alpha was stored, the value of the node was AT MOST this value.
								// If we can cut off, do that.
								if (val.Value <= α)
								{
									node.Value = α;

#if DEBUG
                                    transHits++; // Mark a transposition table hit.
                                    transCuts++;
#endif

									return α;
								}
								break;
							case StateInfo.ValueType.Beta:
								// Since beta was stored, the value of the node was AT LEAST this value.
								// If we can cut off, do that.
								if (val.Value >= β)
								{
									node.Value = β;

#if DEBUG
                                    transHits++; // Mark a transposition table hit.
                                    transCuts++;
#endif

									return β;
								}
								break;
						}
					}

					// If we have a cached entry, but weren't able to cut off based on it,
					// try to add the best move first, but only if it's still valid.
					if (val.Best != null)
					{
						var m = val.Best;

						if (this.workingBoard[m.xfrom, m.yfrom] == aicode && this.workingBoard[m.xto, m.yto] == 0)
						{
							cachedBest = m;

#if DEBUG
                            transHits++; // Mark a transposition table hit.
#endif

							moves.Add(m);
						}
					}
				}
			}

			// Next, check for an exit condition for our search function.
			// If our depth == 0, that means we are just applying our evaluation function.
			// Otherwise, we may have a game-over state.
			if (depth == 0 || this.workingBoard.GameOver)
			{
				// The 'goodness' of current state for our player will simply be his piece count.
				int score = _score(this.workingBoard, pcode, notpcode);

#if DEBUG
                evalCalls++; // Track a call to the evaluation function.
#endif

				// Cache the answer, and set variables.
				_updateOrAddHash(this.workingBoard, pcode, depth, StateInfo.ValueType.Exact, score, null);
				node.Value = score;

				return score;
			}

			// Get an ordered list of moves (and hence game tree nodes) to traverse.
			// Ideally, heuristics should be applied to find the best evaluation order, but
			// that is fancy stuff.
			var moveIter = moves.Concat(_findMoves(this.workingBoard, pcode));
			// If we have no best move ordering, collapse the move list and order by gain.
			//if (cachedBest == null)
			//{
			//	moveIter = moveIter.ToList();
			//	((List<Move>) moveIter).Sort((x, y) => -x.gain.CompareTo(y.gain));
			//	testset++;
			//}
			//tzestseztzeswt++;

			// If we're searching moves, add the current board to the exploration hash set.
			this.repetitionCheck.Add(this.workingBoard);

			// We want to choose the maximum valued node, i.e. the one which gets us the highest score.
			foreach (var move in moveIter)
			{
#if DEBUG
                nodeCnt++; // Increment explored node count.
#endif

				// Get the next node, and modify the board accordingly.
				#region var nextNode;
				var nextNode = node;
				if (node.HasLeaf(move))
					nextNode = node[move];
				//nextNode.Value = 0;
				else
					nextNode = node.AddLeaf(move);
				#endregion
				var boardHash = this.workingBoard.LongHashCode;
				_executeAndPushMove(move, pcode, notpcode);

				// Get the score of the child. Since we must account for perspective switches,
				// We negate the value and reverse/negate alpha and beta.
                var pv2 = new List<Move>();
				var moveVal = -_search(nextNode, depth - 1, -β, -α, !me, pv2);

				// We searched, nothing exploded, undo the move.
				_undoLastMove();
				Debug.Assert(this.workingBoard.LongHashCode == boardHash, "Move undo was unsuccessful!");

				// If moveVal > α, then this is a decently good move. For all we know, at least.
				// Save it as our alpha. Also save the fact that this is an exact node value, if we
				// update alpha.
				if (moveVal > α)
				{
					// If a move results in a score that is greater than or equal to β,
					// this whole node is trash, since the opponent is not going to let the side to
					// move achieve this position, because there is some choice the opponent can make that will avoid it.
					// This is a fail high.
					if (moveVal >= β)
					{
						_updateOrAddHash(this.workingBoard, pcode, depth, StateInfo.ValueType.Beta, β, move);

						// Before cutting off, mark the end of our traversal.
						this.repetitionCheck.Remove(this.workingBoard);
						return β;
					}

                    // Found a new best move, update the principal variation.
                    pv.Clear();
                    pv.Add(move);
                    pv.AddRange(pv2);

					valType = StateInfo.ValueType.Exact;
					cachedBest = move;

					α = moveVal;
				}
			}

			// Done searching moves, close the current board from the repetition checker.
			this.repetitionCheck.Remove(this.workingBoard);

			// The value of the node (the MAXIMUM of its child nodes [best move for me]) is α.
			_updateOrAddHash(this.workingBoard, pcode, depth, valType, α, cachedBest);
			if (valType == StateInfo.ValueType.Exact) node.Value = α;
			return α;
		}

		/// <summary>
		/// Heuristic evaluation of a board state for 'goodness'.
		/// </summary>
		private static int _score(Board b, int code, int othercode)
		{
			// If b is in a game over state, return a score that represents a win or loss
            if (b.GameOver)
            {
                if (b.Count(code) > b.Count(othercode))
                    return int.MaxValue - 10;
                else
                    return -(int.MaxValue - 10);
            }

			// There are 3 major components to a good board state:
			//	- A small perimeter (not many spaces next to friendly pieces, which indicates a good structure)
			//	- The number of pieces within capturing range of an enemy
			//	- The friendly piece count
			int perimeter = 0, pieces = 0, vulnpieces = 0;

			// First find all friendly pieces
			foreach (GamePiece gp in b)
			{
				if (gp.Code == code)
				{
					bool vulnerable = false;
					pieces++;

					// Next iterate through each space adjacent to the piece
					for (int i = 0; i < 8; i++)
					{
						if (!Board.SpaceInBounds(gp.x, gp.y, positioncheck[i][0], positioncheck[i][1]))
							continue;
						int spacex = gp.x + positioncheck[i][0];
						int spacey = gp.y + positioncheck[i][1];
						if (b[spacex, spacey] == 0) // If an empty space is found
						{
							// Add to perimter sum if space is not diagonal to piece
							if (Math.Abs(spacex) != Math.Abs(spacey))
								perimeter++;

							// If "vulnerable" flag is not activated, check whether piece is in capturing range
							if (!vulnerable)
								for (int j = 0; j < 16; j++)
								{
									if (!Board.SpaceInBounds(spacex, spacey, positioncheck[j][0], positioncheck[j][1]))
										continue;
									int inrangex = spacex + positioncheck[j][0];
									int inrangey = spacey + positioncheck[j][1];

									// If it is, set flag to true and add to vulnerable piece count
									if (b[spacex, spacey] == othercode)
									{
										vulnerable = true;
										vulnpieces++;
										break;
									}
								}
						}
					}
				}
			}

			// Bring together the scores and apply math to generate the best representative score of the board
			return 25 * pieces - 8 * vulnpieces - 2 * perimeter; // Mess around with coefficients until ai works
		}

		/// <summary>
		/// Makes a list of all the moves player of code 'code' can make'.
		/// </summary>
		// XXX: TODO - optimize this.
		private static IEnumerable<Move> _findMoves(Board board, int code)
		{
			int ocode = code == 1 ? 2 : 1;

			// Go through all possible moves of each piece of code 'code' on the board
			foreach (GamePiece gp in board)
			{
				if (gp.Code == code)
				{
					for (int i = 0; i < 16; i++)
					{
						int xto = gp.x + positioncheck[i][0], yto = gp.y + positioncheck[i][1];

						if (xto >= Board.SIZE_X || xto < 0 || yto >= Board.SIZE_Y || yto < 0)
							continue;
						if (board[xto, yto] != 0)
							continue;

                        // Count conversions
                        int gain = board.Convert(xto, yto, ocode, ocode);
                        if (i < 8) gain += 1;

                        yield return new Move(gp.x, gp.y, xto, yto, i >= 8, gain);
						//yield return new Move(gp.x, gp.y, xto, yto, i >= 8, 0); // Conversion count is never used, so 0 is used as a placeholder
					}
				}
			}
		}

		/// <summary>
		/// Executes a move to the board
		/// </summary>
		private static void _executeMove(Board board, Move move, int aicode, int othercode)
		{
			Debug.Assert(board[move.xfrom, move.yfrom] == aicode, "Invalid AI predictive move -- piece not owned!");
			Debug.Assert(board[move.xto, move.yto] == 0, "Invalid AI predictive move -- space not empty!");

			if (move.isjump)
				board[move.xfrom, move.yfrom] = 0;
			board[move.xto, move.yto] = aicode;
			board.Convert(move.xto, move.yto, aicode, othercode);
		}

		/// <summary>
		/// <para>Executes a move to the board, pushing it onto a stack to allow for easy undos.</para>
		/// <para>Three transforms occur:</para>
		/// <list type="bullet">
		/// <item><description>aicode -> 0, if move is a jump</description></item>
		/// <item><description>othercode -> aicode</description></item>
		/// <item><description>0 -> aicode</description></item>
		/// </list>
		/// <para>It is not currently possible to distinguish between pieces which should be
		/// restored to othercode vs 0, so the piece which is to be restored to 0 is always first.</para>
		/// </summary>
		private void _executeAndPushMove(Move m, int aicode, int othercode)
		{
			Debug.Assert(this.workingBoard[m.xfrom, m.yfrom] == aicode, "Invalid AI predictive move -- piece not owned!");
			Debug.Assert(this.workingBoard[m.xto, m.yto] == 0, "Invalid AI predictive move -- space not empty!");
			var pieceList = new List<BoardTransform>();

			if (m.isjump)
			{
				pieceList.Add(new BoardTransform() { X = m.xfrom, Y = m.yfrom, PrevCode = aicode });
				this.workingBoard[m.xfrom, m.yfrom] = 0;
			}

			pieceList.Add(new BoardTransform() { X = m.xto, Y = m.yto, PrevCode = 0 });
			this.workingBoard[m.xto, m.yto] = aicode;

			for (int i = -1, xi = m.xto + i; i <= 1; i++, xi = m.xto + i)
				for (int j = -1, yj = m.yto + j; j <= 1; j++, yj = m.yto + j)
					if (xi < Board.SIZE_X && xi >= 0 && yj < Board.SIZE_Y && yj >= 0)
						if (this.workingBoard[xi, yj] == othercode)
						{
							this.workingBoard[xi, yj] = aicode;

							pieceList.Add(new BoardTransform() { X = xi, Y = yj, PrevCode = othercode });
						}

			undoStack.Push(pieceList);
		}

		/// <summary>
		/// Undoes a move, following the reverse transform mappings from <see cref="_executeAndPushMove"/>
		/// </summary>
		private void _undoLastMove()
		{
			Debug.Assert(undoStack.Count > 0, "Undo stack empty, but tried to undo a move!");
			var pieceList = undoStack.Pop();

			foreach (var trans in pieceList)
				this.workingBoard[trans.X, trans.Y] = trans.PrevCode;
		}

		/// <summary>
		/// Add or update a entry in the hashtable.
		/// </summary>
		private void _updateOrAddHash(Board cur, int code, int depth, StateInfo.ValueType flag, int value, Move best)
		{
			if (!this.transTable.ContainsKey(cur))
			{
				this.transTable[cur] = new StateInfo() { CodeToMove = code, Depth = depth, Flag = flag, Value = value, Best = best };
				return;
			}

			var entry = this.transTable[cur];

			if (depth < entry.Depth) // Don't overwrite w/ shallower entries.
				return;

			entry.CodeToMove = code;
			entry.Depth = depth;
			entry.Best = best;
			entry.Flag = flag;
			entry.Value = value;

			if (best != null || entry.Best == null)
				entry.Best = best;
		}

        private void _trace(string s, params object[] a) {
#if DEBUG
            _trace(string.Format(s, a));
#endif
        }
        private void _trace(string s)
        {
#if DEBUG
            debug.AddTrace(s);
#endif
        }

		// Class representing a move
		public class Move
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

			public override int GetHashCode()
			{
				return (xfrom ^ (yfrom << 6) ^ (xto << 12) ^ (yto << 18) ^ (gain << 24)) | (isjump ? 1 << 30 : 0);
			}
			public override bool Equals(object obj)
			{
				if (object.ReferenceEquals(this, obj))
					return true;
				if (object.ReferenceEquals(null, obj))
					return false;
				if (!(obj is Move))
					return false;

				return this.GetHashCode() == obj.GetHashCode();
			}
			public override string ToString()
			{
				return string.Format("[{0},{1} -> {2},{3}; +{4}]", xfrom, yfrom, xto, yto, gain);
			}

			public static bool operator ==(Move one, object two)
			{
				if (object.ReferenceEquals(null, one))
					return object.ReferenceEquals(one, two);

				return one.Equals(two);
			}
			public static bool operator !=(Move one, object two)
			{
				return !(one == two);
			}
		}

		// Struct for transposition table stuff.
		private class StateInfo
		{
			public int CodeToMove { get; set; }
			public int Depth { get; set; }
			public ValueType Flag { get; set; }
			public int Value { get; set; }
			public Move Best { get; set; }

			public enum ValueType
			{
				Exact,
				Alpha,
				Beta
			}
		}

		// Struct for undoing board transforms.
		private struct BoardTransform
		{
			public int X { get; set; }
			public int Y { get; set; }
			public int PrevCode { get; set; }
		}

		// Game tree.
		// E = edge type (move)
		// S = State type (board)
		public sealed class Tree<E, S>
		{
			public Node Root { get; set; }

			public Tree()
			{
				this.Root = new Node();
			}

			public sealed class Node : IEnumerable<KeyValuePair<E, Node>>
			{
				private Dictionary<E, Node> leaves = new Dictionary<E, Node>();

				public int? Value { get; set; }
				public int Count { get { return leaves.Count; } }

				public Node this[E t]
				{
					get { return leaves[t]; }
					set { leaves[t] = value; }
				}

				public Node AddLeaf(E edge)
				{
					var n = new Node();
					leaves[edge] = n;
					return n;
				}

				public bool HasLeaf(E t)
				{
					return leaves.ContainsKey(t);
				}

				public KeyValuePair<E, Node> GetSingleMove()
				{
					foreach (var x in this.leaves)
						return x;
					throw new InvalidOperationException();
				}

				public override string ToString()
				{
					return Value + "; " + leaves.Count + " children";
				}

				public IEnumerator<KeyValuePair<E, Node>> GetEnumerator()
				{
					return this.leaves.GetEnumerator();
				}
				System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
				{
					return GetEnumerator();
				}
			}
		}

		// AI board class optimized for an 8x8 board.
		// Boards are organized as follows:
		// x = 0 1 2 3 4 5 6 7 
		//     n n n n n n n n 0 = y
		//     n n n n n n n n 1 = y
		//     n n n n n n n n 2 = y
		//     n n n n n n n n 3 = y
		//     n n n n n n n n 4 = y
		//     n n n n n n n n 5 = y
		//     n n n n n n n n 6 = y
		//     n n n n n n n n 7 = y
		public sealed class AIBoard
		{
			private static ulong[][] hashkey = new ulong[64][];
			private static ulong[] convMasks = new ulong[64]; // Bit masks for piece conversions.
			private static ulong[] moveMasks = new ulong[64]; // Bit masks for possible move positions.

			// When the board[] array is projected as a single 4-bit number, this converts
			// each bit to its corresponding code. 0001 = 1 => code 0, 0010 = 2 => code 1,
			// 0100 = 4 => code 2, 1000 = 8 => code 3.
			private static int[] bitboardToCode = new int[] { -999, 0, 1, -999, 2, -999, -999, -999, 3 };

			private ulong[] board = new ulong[4];
			private ulong boardHash = 0UL;

			// Initializes some important values.
			static AIBoard()
			{
				// Initialize hashing values.
				Random r = new Random();
				for (int i = 0; i < 64; i++)
				{
					hashkey[i] = new ulong[4];

					for (int x = 0; x < 4; x++)
						hashkey[i][x] = (ulong) r.Next() ^ ((ulong) r.Next() << 15) ^ ((ulong) r.Next() << 30) ^
							((ulong) r.Next() << 45) ^ ((ulong) r.Next() << 60);
				}

				// A piece converts in a pattern looking like this:
				// X X X
				// X P X
				// X X X
				// The appropriate bits to be set are, therefore, bit 1, 2, 3, (8 + 1), (8 + 3), (16 + 1), (16 + 2), (16 + 3).
				// Relative to its index (9) or (1, 1), the bits are (9 - 8), (9 - 7), (9 - 6), 9, (9 + 2), (9 + 8), (9 + 9), (9 + 10).
				// Dynamically generating these masks is trivial. Some care must be taken for the left and right cols, since those potentially can "wrap" around to the opposite side.
				for(int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
					{
						int idx = y * 8 + x;

						convMasks[idx] =
							(x > 0 ? 1UL.LS(idx - 8) : 0) |
							/*   */ (1UL.LS(idx - 7)) |
							(x < 7 ? 1UL.LS(idx - 6) : 0) |
							(x > 0 ? 1UL.LS(idx + 0) : 0) |
							(x < 7 ? 1UL.LS(idx + 2) : 0) |
							(x > 0 ? 1UL.LS(idx + 8) : 0) |
							/*   */ (1UL.LS(idx + 9)) |
							(x < 7 ? 1UL.LS(idx + 10) : 0);
					}

				// A piece can possibly move in a pattern looking like this:
				// X - X - X
				// - X X X -
				// - X P X -
				// - X X X -
				// X - X - X
				// The bits, relative to its index (27) or (3, 3), are (27 - 26), (27 - 24), (27 - 22), (27 - 17), (27 - 16), (27 - 15),
				// (27 - 1), (27 + 1), (27 + 7), (27 + 8), (27 + 9), (27 + 14), (27 + 16), (27 + 18).
				// Again, care must be taken to exclude any left/right cols that aren't applicable.
				for(int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
					{
						int idx = y * 8 + x;

						moveMasks[idx] =
							(x > 1 ? 1UL.LS(idx - 26) : 0) | (1UL.LS(idx - 24)) | (x < 6 ? 1UL.LS(idx - 22) : 0) |
							(x > 0 ? 1UL.LS(idx - 17) : 0) | (1UL.LS(idx - 16)) | (x < 7 ? 1UL.LS(idx - 15) : 0) |
							(x > 0 ? 1UL.LS(idx - 01) : 0) /*                */ | (x < 7 ? 1UL.LS(idx + 01) : 0) |
							(x > 0 ? 1UL.LS(idx + 07) : 0) | (1UL.LS(idx + 08)) | (x < 7 ? 1UL.LS(idx + 09) : 0) |
							(x > 1 ? 1UL.LS(idx + 14) : 0) | (1UL.LS(idx + 16)) | (x < 6 ? 1UL.LS(idx + 18) : 0);
					}
			}

			public AIBoard(Board b)
			{
				foreach (var p in b)
					this.board[p.Code] |= 1UL << (p.y * 8 + p.x);

				this.boardHash = GetLongHashCode();
			}

			// Do magic to transform a human-readable x-y pair to the bit-array backed thingy.
			public int this[int x, int y]
			{
				get {
					// Projects each ulong as a 4-bit number, with the LSB corresponding to code = 0, and 
					// MSB corresponding to code = 3, then collapsing that into a code using the array.
					return bitboardToCode[board[0] | board[1] << 1 | board[2] << 2 | board[3] << 3];
				}
				set
				{
					int seqVal = y * 8 + x; // Calculate 0-63 indexing.
					int oldCode = this[x, y]; // Calculate the old code.
					ulong bitmask = 1UL << seqVal; // Find the bit position that represents the space.

					// Unset old code, set new code.
					this.boardHash ^= hashkey[seqVal][oldCode];
					this.boardHash ^= hashkey[seqVal][value];

					// Flip bits.
					board[oldCode] ^= bitmask;
					board[value] ^= bitmask;
				}
			}

			// Given the opponent's code, calculate the potential gain if a friendly piece were to move to (x, y).
			public int Gain(int x, int y, int othercode)
			{
				return (board[othercode] & convMasks[y * 8 + x]).PopCount();
			}

			// Change all spaces adjacent to space (x, y) with code codefrom to code codeto
			// The original convert function accounted for only .63% of all exectution time, and is
			// therefore efficient enough for our needs.
			public int Convert(int x, int y, int codeto, int codefrom)
			{
				int count = 0;

				for (int i = -1, xi = x + i; i <= 1; i++, xi = x + i)
					for (int j = -1, yj = y + j; j <= 1; j++, yj = j + y)
						if (xi < 8 && xi >= 0 && yj < 8 && yj >= 0)
							if (this[xi, yj] == codefrom)
							//if ((board[codefrom] & (1UL << (yj * 8 + xi))) != 0) // "optimized?" version
							{
								this[xi, yj] = codeto;
								count++;
							}

				return count;
			}

			// Return a count of all pieces on the board with code c
			public int Count(int c)
			{
				return board[c].PopCount();
			}

			// Checks if there are any pieces with code c that can move
			public bool HasMovesLeft(int c)
			{
				ulong mask = 1UL;

				for (int idx = 0; idx < 64; idx++, mask <<= 1)
				{
					// We need a piece of c at idx, as well as there to be a free spot somewhere that piece can move.
					if ((board[c] & mask) != 0 && ((board[0] & moveMasks[idx]) != 0))
						return true;
				}

				return false;
			}

			public override bool Equals(object obj)
			{
				if (object.ReferenceEquals(null, obj))
					return false;
				if (object.ReferenceEquals(this, obj))
					return true;
				if (!(obj is AIBoard))
					return false;

				var b2 = obj as AIBoard;

				if (b2.boardHash != this.boardHash)
					return false;

				for (int i = 0; i < 4; i++)
					if (b2.board[i] != this.board[i])
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
				for (int c = 0; c < 8; c++)
				{
					for (int r = 0; r < 8; r++)
						sb.Append(this[r, c]);
					sb.AppendLine();
				}
				return sb.ToString();
			}
			public ulong GetLongHashCode()
			{
				ulong code = 0L;

				for (int x = 0; x < 8; x++)
					for (int y = 0; y < 8; y++)
						code ^= AIBoard.hashkey[y * 8 + x][this[x, y]];

				return code;
			}
		}
	}

	static class Extensions
	{
		// See wikipedia "Hamming Weight"
		public static int PopCount(this ulong i)
		{
			i = i - ((i >> 1) & 0x5555555555555555UL);
			i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
			return (int) (unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
		}

		// Corrects for the behavior of left-shift and negative values, as well as shifting modulo.
		public static ulong LS(this ulong l, int i)
		{
			if (i < -63 || i > 63)
				return 0;

			return i > 0 ? l << i : l >> -i;
		}

		public static int ChompToZero(this int i)
		{
			return i < 0 ? 0 : i;
		}
	}
}
