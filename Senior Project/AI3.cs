using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Senior_Project
{
	sealed class ICanSeeForever : AI
	{
		private const int SEARCH_DEPTH = 4;
		private const double MAX_PLY_TIME = 1.5;

		private static readonly int[][] positioncheck = new int[][] {
			new[] { 0, 1 },  new[] { 1, 1 },   new[] { 1, 0 },  new[] { 1, -1 }, 
            new[] { 0, -1 }, new[] { -1, -1 }, new[] { -1, 0 }, new[] { -1, 1 }, 
            new[] { 0, 2 },  new[] { 2, 2 },   new[] { 2, 0 },  new[] { 2, -2 }, 
            new[] { 0, -2 }, new[] { -2, -2 }, new[] { -2, 0 }, new[] {-2, 2 } };

		private Tree<Move, AIBoard> gameTree = new Tree<Move, AIBoard>();
		private Dictionary<AIBoard, StateInfo> transTable = new Dictionary<AIBoard, StateInfo>(4096);
		private HashSet<AIBoard> repetitionCheck = new HashSet<AIBoard>();

		private Random rnd = new Random();

		private AIBoard workingBoard;

		private int moveNum;

		private DateTime startT;
		private DateTime depthStart;

#if DEBUG
		private AI3DebugForm debug;

		private long nodeCnt = 0, transHits = 0, transCuts = 0, evalCalls = 0;
		
#endif

		#region Variables
		int notaicode;
		#endregion

		// Constructor
		public ICanSeeForever(Board b, int ct)
			: base(b, ct)
		{
			notaicode = ct == 1 ? 2 : 1;

			moveNum = 0;
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
			transTable.Clear();
			repetitionCheck.Clear();

			moveNum++;

			// Set up root of tree.
			this.gameTree.Root = new Tree<Move, AIBoard>.Node();
			this.workingBoard = new AIBoard(board);

			// TODO - 
			int best = int.MinValue;
			int adjusted_depth = SEARCH_DEPTH;
			Move move = null;
			startT = DateTime.Now;
			for (int d = 1; d <= adjusted_depth; d++)
			{
				// Initialize state.
				List<Move> pv = new List<Move>();
				depthStart = DateTime.Now;
#if DEBUG
				nodeCnt = 0; transHits = 0; transCuts = 0; evalCalls = 0;
#endif

				// Perform bounded depth-first search for the best move score.
				best = -_search(this.gameTree.Root, d, -int.MaxValue, int.MaxValue, true, pv);

				// If we finished our last ply fast enough (<MAX_PLY_TIME), and we're not in mid-game, go for another.
				// If the game's over, though, don't bother.
				if (d == adjusted_depth && moveNum > 25 && (DateTime.Now - depthStart).TotalSeconds < MAX_PLY_TIME
					&& best < 10000 && best > -10000)
					adjusted_depth++;

				// WE GOT CALLED TO MAKE A MOVE BUT HAVE NO MOVES TO MAKE. WAT.
				if (this.gameTree.Root.Count == 0)
					return;

				if (pv.Count > 0)
					move = pv[0];
#if DEBUG
				_trace("[SEARCH] d={0}, n={1}, tHit={2} ({3}%), tCut={4} ({5}%), e={6} ({7}%)", d, nodeCnt, transHits,
					Math.Round((double) transHits / nodeCnt, 2) * 100, transCuts,
					Math.Round((double) transCuts / nodeCnt, 2) * 100, evalCalls,
					Math.Round((double) evalCalls / nodeCnt, 2) * 100);
				_trace("         t={0}s ({1} n/s), PV={2}", Math.Round((DateTime.Now - depthStart).TotalMilliseconds / 1000, 3),
					Math.Round(nodeCnt / ((DateTime.Now - depthStart).TotalMilliseconds / 1000), 0),
					pv.Aggregate("", new Func<string, Move, string>((a, b) => a + b + " ")).Trim());
#endif
			}

			Debug.Assert(move != null, "No move! Endgame condition without our notification?");

#if DEBUG
			_trace("[EXEC] {0}", move);
#endif
			_executeMove(this.board, move, aicode, notaicode);
		}

		// Performs an iterative deepening depth-first search, combining the concepts of
		// minimax with alpha-beta elimination and some memoization to efficiently compute
		// the net gain/loss of any given series of moves.
		//
		// α is the minimum score that the maximizing player is assured of.
		// β is the maximum score that the minimizing player is assured of.
		private int _search(Tree<Move, AIBoard>.Node node, int depth, int α, int β, bool me, List<Move> pv)
		{
			int pcode = me ? aicode : notaicode, notpcode = !me ? aicode : notaicode;
			StateInfo.ValueType valType = StateInfo.ValueType.Alpha;
			List<Move> moves = new List<Move>();
			Move cachedBest = null;

			// The following only applies if we're not at the root.
			if (!object.ReferenceEquals(this.gameTree.Root, node))
			{
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

							if (this.workingBoard[m.xfrom, m.yfrom] == pcode && this.workingBoard[m.xto, m.yto] == 0)
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
				this.workingBoard.PopState();
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
		private static int _score(AIBoard b, int code, int othercode)
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
			int perimeter = 0, pieces = b.Count(code), vulnpieces = 0;

			// First find all friendly pieces
			foreach (var gp in b.GetPieces(code))
			{
				// For each space next to this piece empty, add to the perimeter sum.
				perimeter += b.EmptyAdjacent(gp.Item1, gp.Item2);

				// If piece can be captured, add to vulnerable pieces.
				if (b.IsVulnerable(gp.Item1, gp.Item2, othercode))
					vulnpieces++;
			}

			// Bring together the scores and apply math to generate the best representative score of the board
			return 25 * pieces - 8 * vulnpieces - 2 * perimeter; // Mess around with coefficients until ai works
		}

		/// <summary>
		/// Makes a list of all the moves player of code 'code' can make'.
		/// </summary>
		// XXX: TODO - optimize this.
		private static IEnumerable<Move> _findMoves(AIBoard board, int code)
		{
			int ocode = code == 1 ? 2 : 1;

			// Go through all possible moves of each piece of code 'code' on the board
			foreach (var gp in board.GetPieces(code))
			{
				for (int i = 0; i < 16; i++)
				{
					int xto = gp.Item1 + positioncheck[i][0], yto = gp.Item2 + positioncheck[i][1];

					if (xto >= Board.SIZE_X || xto < 0 || yto >= Board.SIZE_Y || yto < 0)
						continue;
					if (board[xto, yto] != 0)
						continue;

					// Count conversions
					int gain = board.Gain(xto, yto, ocode);
					if (i < 8) gain += 1;

					yield return new Move(gp.Item1, gp.Item2, xto, yto, i >= 8, gain);
					//yield return new Move(gp.x, gp.y, xto, yto, i >= 8, 0); // Conversion count is never used, so 0 is used as a placeholder
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

			this.workingBoard.PushState();

			if (m.isjump)
				this.workingBoard[m.xfrom, m.yfrom] = 0;
			this.workingBoard[m.xto, m.yto] = aicode;
			this.workingBoard.Convert(m.xto, m.yto, aicode, othercode);
		}

		/// <summary>
		/// Add or update a entry in the hashtable.
		/// </summary>
		private void _updateOrAddHash(AIBoard cur, int code, int depth, StateInfo.ValueType flag, int value, Move best)
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
			entry.Flag = flag;
			entry.Value = value;

			if (best != null || entry.Best == null)
				entry.Best = best;
		}

#if DEBUG
		private void _trace(string s, params object[] a)
		{
			_trace(string.Format(s, a));
		}

		private void _trace(string s)
		{
			debug.AddTrace(s);
		}
#endif

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
			private static ulong[] adjMasks = new ulong[64]; // Bit masks for adjacent pieces. It's similar to piece conversions, so they're generated together.
			private static ulong[] vulnMasks = new ulong[64]; // Bit masks for calculating vulnerability. If an enemy piece is detected in this area, the piece is considered vulnerable.

			// When the board[] array is projected as a single 4-bit number, this converts
			// each bit to its corresponding code. 0001 = 1 => code 0, 0010 = 2 => code 1,
			// 0100 = 4 => code 2, 1000 = 8 => code 3.
			private static int[] bitboardToCode = new int[] { -999, 0, 1, -999, 2, -999, -999, -999, 3 };

			private ulong[] board = new ulong[4];
			private ulong boardHash = 0UL;

			private Stack<Tuple<ulong, ulong, ulong, ulong, ulong>> stateStack = new Stack<Tuple<ulong, ulong, ulong, ulong, ulong>>();

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

				// The AI requires knowledge of adjacent pieces:
				// - X -
				// X P X
				// - X -
				// Since this is similar to the conversion pattern, this is generated first, and used to build the conversion pattern.
				//
				// A piece converts in a pattern looking like this:
				// X X X
				// X P X
				// X X X
				// The appropriate bits to be set are, therefore, bit 1, 2, 3, (8 + 1), (8 + 3), (16 + 1), (16 + 2), (16 + 3).
				// Relative to its index (9) or (1, 1), the bits are (9 - 8), (9 - 7), (9 - 6), 9, (9 + 2), (9 + 8), (9 + 9), (9 + 10).
				// Dynamically generating these masks is trivial. Some care must be taken for the left and right cols, since those potentially can "wrap" around to the opposite side.
				for (int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
					{
						int idx = y * 8 + x;

						adjMasks[idx] =
							/*   */ (1UL.LS(idx - 7)) |
							(x > 0 ? 1UL.LS(idx + 0) : 0) |
							(x < 7 ? 1UL.LS(idx + 2) : 0) |
							/*   */ (1UL.LS(idx + 9));

						convMasks[idx] = adjMasks[idx] |
							(x > 0 ? 1UL.LS(idx - 8) : 0) |
							(x < 7 ? 1UL.LS(idx - 6) : 0) |
							(x > 0 ? 1UL.LS(idx + 8) : 0) |
							(x < 7 ? 1UL.LS(idx + 10) : 0);
					}

				// A piece can possibly move in a pattern looking like this:
				// X - X - X
				// - X X X -
				// X X P X X
				// - X X X -
				// X - X - X
				// The bits, relative to its index (27) or (3, 3), are (27 - 26), (27 - 24), (27 - 22), (27 - 17), (27 - 16), (27 - 15),
				// (27 - 1), (27 + 1), (27 + 7), (27 + 8), (27 + 9), (27 + 14), (27 + 16), (27 + 18).
				// Again, care must be taken to exclude any left/right cols that aren't applicable.
				for (int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
					{
						int idx = y * 8 + x;

						moveMasks[idx] =
							(x > 1 ? 1UL.LS(idx - 26) : 0) | (1UL.LS(idx - 24)) | (x < 6 ? 1UL.LS(idx - 22) : 0) |
							(x > 0 ? 1UL.LS(idx - 17) : 0) | (1UL.LS(idx - 16)) | (x < 7 ? 1UL.LS(idx - 15) : 0) |
							(x > 1 ? 1UL.LS(idx - 02) : 0) | (x > 0 ? 1UL.LS(idx - 01) : 0) | (x < 7 ? 1UL.LS(idx + 01) : 0) | (x < 6 ? 1UL.LS(idx + 02) : 0) |
							(x > 0 ? 1UL.LS(idx + 07) : 0) | (1UL.LS(idx + 08)) | (x < 7 ? 1UL.LS(idx + 09) : 0) |
							(x > 1 ? 1UL.LS(idx + 14) : 0) | (1UL.LS(idx + 16)) | (x < 6 ? 1UL.LS(idx + 18) : 0);
					}

				// A piece is considered vulnerable if an enemy piece can convert it. To check for that, we combine all the move masks with the
				// spaces adjacent to a given space to form a composite "vulnerability mask."
				for (int y = 0; y < 8; y++)
					for (int x = 0; x < 8; x++)
					{
						int idx = y * 8 + x;

						for (int i = 0; i < 8; i++)
						{
							int xi = x + positioncheck[i][0], yi = y + positioncheck[i][1];
							if (xi < 0 || xi >= 8 || yi < 0 || yi >= 8)
								continue;

							vulnMasks[idx] |= moveMasks[yi * 8 + xi];
						}
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
				get
				{
					int seqVal = y * 8 + x; // Calculate 0-63 indexing.
					ulong bitmask = 1UL << (y * 8 + x); // Find the bit position that represents the space.

					// Projects each ulong as a 4-bit number, with the LSB corresponding to code = 0, and 
					// MSB corresponding to code = 3, then collapsing that into a code using the array.
					return bitboardToCode[
						(board[0] & bitmask) >> seqVal |
						(board[1] & bitmask) >> seqVal << 1 |
						(board[2] & bitmask) >> seqVal << 2 |
						(board[3] & bitmask) >> seqVal << 3];
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

			// Gets whether not the game is over.
			public bool GameOver
			{
				get
				{
					return board[0].PopCount() == 0 || !HasMovesLeft(1) ||
						!HasMovesLeft(2);
				}
			}

			public long LongHashCode { get { return (long) this.boardHash; } }

			// Given the opponent's code, calculate the potential gain if a friendly piece were to move to (x, y).
			public int Gain(int x, int y, int othercode)
			{
				return (board[othercode] & convMasks[y * 8 + x]).PopCount();
			}

			// Given the opponent's code, calculate if a piece is vulnerable.
			public bool IsVulnerable(int x, int y, int othercode)
			{
				return (board[othercode] & vulnMasks[y * 8 + x]) != 0;
			}

			// Given a location, return the number of empty spaces directly adjacent to the piece.
			public int EmptyAdjacent(int x, int y)
			{
				return (board[0] & adjMasks[y * 8 + x]).PopCount();
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

			// Gets the x and y of all pieces with a given code.
			public IEnumerable<Tuple<int, int>> GetPieces(int code)
			{
				ulong subseg = board[code];

				for (int idx = 0; idx < 64; idx++)
				{
					if ((subseg & 0x1) == 0x1)
						yield return Tuple.Create(idx % 8, idx / 8);

					subseg >>= 1;
				}
			}

			// Saves the current board state.
			public void PushState()
			{
				stateStack.Push(Tuple.Create(board[0], board[1], board[2], board[3], boardHash));
			}
			// Restores the most recently saved board state.
			public void PopState()
			{
				var t = stateStack.Pop();
				board = new ulong[] { t.Item1, t.Item2, t.Item3, t.Item4 };
				boardHash = t.Item5;
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
