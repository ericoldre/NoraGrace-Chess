using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace NoraGrace.Engine
{


    public struct CutoffStats
    {
        public int PVNodes;
        public int FailLows;
        public int[] CutoffAfter;

        public int TotalCutoffs
        {
            get { return CutoffAfter.Sum(); }
        }
        public int TotalNodes
        {
            get { return TotalCutoffs + FailLows + PVNodes; }
        }

        public float CutoffPctFirst
        {
            get { return TotalCutoffs == 0 ? (float)0 : (float)CutoffAfter[1] / (float)TotalCutoffs; }
        }

        public float CutoffAvg
        {
            get
            {
                if (TotalCutoffs == 0) { return 0; }
                var totalMoves = CutoffAfter.Select((c, i) => c * i).Sum();
                return (float)totalMoves / (float)TotalCutoffs;
            }
        }

        //public float CutoffAvgAfter1
        //{
        //    get
        //    {
        //        if (TotalCutoffs == 0) { return 0; }
        //        var totalMoves = CutoffAfter.Select((c, i) => c * i).Sum();
        //        return (float)totalMoves / (float)TotalCutoffs;
        //    }
        //}

        //public float CutoffAverage
        //{
        //    get
        //    {

        //    }
        //}

        public float CutoffPct
        {
            get { return TotalNodes == 0 ? (float)0 : (float)TotalCutoffs / (float)TotalNodes; }
        }

        public float PVPct
        {
            get { return TotalNodes == 0 ? (float)0 : (float)this.PVNodes / (float)TotalNodes; }
        }

        public float FailLowPct
        {
            get { return TotalNodes == 0 ? (float)0 : (float)this.FailLows / (float)TotalNodes; }
        }

        public static CutoffStats[] AtDepth = new CutoffStats[50];

        static CutoffStats()
        {
            for (int i = 0; i < 50; i++)
            {
                AtDepth[i].CutoffAfter = new int[300];
            }
        }
    }


	public class Search
	{

		private static readonly int INFINITY = 32000;
		

		public static int CountTotalAINodes = 0;
		public static TimeSpan CountTotalAITime = new TimeSpan();
		public int CountAIValSearch = 0;
		public int CountAIQSearch = 0;


        public static int MateIn(int ply)
        {
            return 30000 - ply; //private static readonly int VALCHECKMATE = 30000;
        }

        public static bool IsMateScore(int score)
        {
            return Math.Abs(score) > MateIn(MAX_PLY);
        }

		#region helper classes
		public class Progress
		{
			public readonly int Depth;
			public readonly int Nodes;
			public readonly int Score;
			public readonly TimeSpan Time;
			public readonly ReadOnlyCollection<Move> PrincipleVariation;
            public readonly FEN FEN;
            public Progress(int a_depth, int a_nodes, int a_score, TimeSpan a_time, List<Move> a_pv, FEN a_fen)
			{
                if (a_pv == null) { throw new ArgumentNullException("a_pv"); }
                if (a_fen == null) { throw new ArgumentNullException("a_fen"); }
				Depth = a_depth;
				Nodes = a_nodes;
				Score = a_score;
				Time = a_time;
				PrincipleVariation = new ReadOnlyCollection<Move>(a_pv);
                FEN = a_fen;
			}
		}

		public class BlunderChance
		{
			public double BlunderPct { get; set; } //pct chance that a move is not seen
			public double BlunderDepth { get; set; } //higher the number the greater chance that the move will be seen if examining at depth
			public double BlunderReduceEnd { get; set; } //after above calculation, reduce by this chance
			public int BlunderSkipCount { get; set; } //moves at beginning of search that are immune to blunder check

			public BlunderChance()
			{
				BlunderPct = .0;
				BlunderDepth = 1;
				BlunderReduceEnd = .05;
				BlunderSkipCount = 20;
				
			}

			public double CalcBlunderPct(int depth, bool isRecapture, int moveNum)
			{
				//find chance of blunder at this depth.
				double pct = BlunderPct;
				for (int i = 1; i <= depth; i++)
				{
					pct = pct * (1 - BlunderDepth);
				}
				if (isRecapture) { pct = pct * .5; } //recapture on same sq is more obvious
				if (moveNum <= 4) { pct = pct * .5; } //first 4 ordered moves stand to be most obvious
				pct -= BlunderReduceEnd;
				if (pct < 0) { pct = 0; }

				return pct;

			}

			public bool DoBlunder(Int64 Zob, int depth, bool isRecapture, int moveNum, int alpha, int beta)
			{
                if (alpha < -Search.MateIn(5) || beta > Search.MateIn(5)) { return false; }
				if (moveNum < this.BlunderSkipCount) { return false; }

				
				double MoveChanceKey = ((double)(Zob & 255)) / 255;
				if (MoveChanceKey < 0) { MoveChanceKey = -MoveChanceKey; }
				double blunderchance = this.CalcBlunderPct(depth, isRecapture, moveNum);
				if (MoveChanceKey < blunderchance)
				{
					return true;
				}
				return false;
			}

		}

		public class Args
		{
			public FEN GameStartPosition { get; set; }
            public List<Move> GameMoves { get; set; }
			public int MaxDepth { get; set; }
			public int NodesPerSecond { get; set; }
			public TranspositionTable TransTable { get; set; }
			public BlunderChance Blunder { get; set; }
			public TimeSpan Delay { get; set; }
            public Evaluation.IChessEval Eval { get; set; }
            public int MaxNodes { get; set; }
            public int ContemptForDraw { get; set; }
            public ITimeManager TimeManager { get; set; }
            public SearchDepth ExtensionCheck { get; set; }
            public SearchDepth ExtensionPawn7th { get; set; }
            public bool ExtendSEEPositiveOnly { get; set; }
			public Args()
			{
                GameStartPosition = new FEN(FEN.FENStart);
                GameMoves = new List<Move>();
				MaxDepth = int.MaxValue;
				NodesPerSecond = int.MaxValue;
				Blunder = new BlunderChance();
				Delay = new TimeSpan(0);
                Eval = Evaluation.Evaluator.Default;
                MaxNodes = int.MaxValue;
                ContemptForDraw = 40;
                TimeManager = new TimeManagerAnalyze();
                ExtensionCheck = SearchDepthInfo.FromPly(1);
                ExtensionPawn7th = SearchDepthInfo.FromPly(1);
                ExtendSEEPositiveOnly = true;
			}
		}

		#endregion

		public event EventHandler<SearchProgressEventArgs> ProgressReported;

        const int MAX_PLY = 50;
        private Move[][] _pv = new Move[MAX_PLY + 1][];
        private int[] _pvLen = new int[MAX_PLY + 1];

		public readonly Args SearchArgs;
		private readonly Board board;
        private readonly Evaluation.IChessEval eval;
        private Move[] CurrentVariation = new Move[MAX_PLY + 1];
		private DateTime _starttime;
		private bool _aborting = false;
		private bool _returnBestResult = true; //if false return null
        private List<Move> _bestvariation = new List<Move>();
		private int _bestvariationscore = 0;

        private readonly MovePicker.Stack _moveBuffer = new MovePicker.Stack();
        private readonly Evaluation.ChessEvalInfoStack _evalInfoStack; 

        private readonly Dictionary<Move, int> _rootMoveNodeCounts = new Dictionary<Move, int>();
		private readonly Int64 BlunderKey = Rand64();
		
		public Search(Args args)
		{
            for (int i = 0; i < _pv.Length; i++)
            {
                _pv[i] = new Move[MAX_PLY + 1];
            }

            SearchArgs = args;
            eval = args.Eval;
            _evalInfoStack = new Evaluation.ChessEvalInfoStack(args.Eval as Evaluation.Evaluator);

            board = new Board(SearchArgs.GameStartPosition);
            

			foreach (Move histmove in SearchArgs.GameMoves)
			{
				board.MoveApply(histmove);
			}

		}

        private static Random rand = new Random();
        
        public static Int64 Rand64()
        {
            byte[] bytes = new byte[8];
            rand.NextBytes(bytes);
            Int64 retval = 0;
            for (int i = 0; i <= 7; i++)
            {
                //Int64 ibyte = (Int64)bytes[i]&256;
                Int64 ibyte = (Int64)bytes[i];
                retval |= ibyte << (i * 8);
            }
            return retval;
        }

		public void Abort()
		{
			Abort(true);
		}
		public void Abort(bool returnBestResult)
		{
			_aborting = true;
			_returnBestResult = returnBestResult;
		}

		public Progress Start()
		{

			_starttime = DateTime.Now;		

			Thread.Sleep(this.SearchArgs.Delay);

            //setup evaluation score for draw.
            eval.DrawScore = board.WhosTurn == Player.White ? -SearchArgs.ContemptForDraw : SearchArgs.ContemptForDraw;
			SearchArgs.TransTable.AgeEntries(2);
             
            SearchArgs.TimeManager.StopSearch += TimeManager_StopSearch;
            SearchArgs.TimeManager.StartSearch(board.FENCurrent);
             

			SearchDepth depth = SearchDepth.PLY;

			int MateScoreLast = 0;
			int MateScoreCount = 0;

            var maxDepth = SearchDepthInfo.FromPly(Math.Min(MAX_PLY, this.SearchArgs.MaxDepth));

            while (depth.Value() <= maxDepth.Value())
            {

                ValSearchRoot(depth);

                //if we get three consecutive depths with same mate score.. just move.
                if (_bestvariationscore > Search.MateIn(10) || _bestvariationscore < -Search.MateIn(10))
                {
                    if (MateScoreLast == _bestvariationscore)
                    {
                        MateScoreCount++;
                    }
                    MateScoreLast = _bestvariationscore;
                    if (MateScoreCount >= 3) { _aborting = true; }
                }
                else
                {
                    MateScoreCount = 0;
                    MateScoreLast = 0;
                }


                ////add check for draw on near horizon, in case of 50 move rule approaching, search will output giant amount of data to console if next move forces draw.
                if (depth.ToPly() > 10 && _bestvariation != null && _bestvariation.Count() < depth.ToPly() - 6 && _bestvariationscore == eval.DrawScore)
                {
                    _aborting = true;
                }


                if (_aborting) { break; }

                depth = depth.AddPly(1);
            }

			CountTotalAITime += (DateTime.Now - _starttime);

            SearchArgs.TimeManager.EndSearch();
            SearchArgs.TimeManager.StopSearch -= TimeManager_StopSearch;
            
			//return progress
			if (_returnBestResult)
			{
				Progress progress = new Progress(depth.ToPly(), CountAIValSearch, _bestvariationscore, DateTime.Now - _starttime, _bestvariation, this.board.FENCurrent);
				return progress;
			}
			else
			{
				return null;
			}
		}


        void TimeManager_StopSearch(object sender, EventArgs e)
        {
            _aborting = true;
        }

		private void ValSearchRoot(SearchDepth depth)
		{

            SearchArgs.TimeManager.StartDepth(depth.ToPly());

			//set trans table entries
			SearchArgs.TransTable.StoreVariation(board, _bestvariation);
            
			//get trans table move
			int tt_score = 0;
            Move tt_move = Move.EMPTY;
            SearchArgs.TransTable.QueryCutoff(board.ZobristBoard, depth.Value(), -INFINITY, INFINITY, out tt_move, out tt_score);
			

			bool in_check_before_move = board.IsCheck();

            //get all legal moves ordered first by TT move then by nodes needed to refute.
            var moves = MoveInfo.GenMovesLegal(board)
                .Select(m => new { move = m, nodes = _rootMoveNodeCounts.ContainsKey(m) ? _rootMoveNodeCounts[m] : 0 })
                .OrderBy(m => m.move == tt_move ? 0 : 1)
                .ThenByDescending(m => m.nodes)
                .Select(m => m.move)
                .ToArray();

			int alpha = -INFINITY;
			int beta = INFINITY;

            Move bestmove = Move.EMPTY;
            int move_num = 0;
            foreach (Move move in moves)
			{
				CurrentVariation[0] = move;

                board.MoveApply(move);

                if (board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    board.MoveUndo();
                    continue;
                }

                move_num++;
				int score = 0;

                SearchArgs.TimeManager.StartMove(move);
                int nodeCountBeforeMove = this.CountAIValSearch;

				if (depth.ToPly() <= 3)
				{
					//first couple nodes search full width
					score = -ValSearchPVS(depth - 1, 1, move_num, -beta, -alpha);
				}
				else if (_bestvariation != null && _bestvariation.Count > 0 && move == this._bestvariation[0])
				{
					//doing first move of deeper search

					score = ValSearchAspiration(depth, alpha, _bestvariationscore, 30, move_num);
				}
				else
				{
					//doing search of a move we believe is going to fail low
					score = ValSearchAspiration(depth, alpha, alpha, 0, move_num);
				}

                _rootMoveNodeCounts.Remove(move);
                _rootMoveNodeCounts.Add(move, this.CountAIValSearch - nodeCountBeforeMove);
                SearchArgs.TimeManager.EndMove(move);

				board.MoveUndo();

				if (_aborting) { break; }

				if (score >= beta)
				{
					//impossible with root search
				}
				if (score > alpha)
				{
					alpha = score;
					bestmove = move;

                    //save principle variation
                    int ply = 0;
                    _pv[ply][0] = move;
                    int subLen = _pvLen[ply + 1];
                    Array.Copy(_pv[ply + 1], 0, _pv[ply], 1, subLen);
                    _pvLen[ply] = subLen + 1;


					//save instance best info
					_bestvariation = GetLegalPV(this.board.FENCurrent, _pv[0]);
					_bestvariationscore = alpha;

					//store to trans table
                    SearchArgs.TransTable.Store(board.ZobristBoard, depth.Value(), TranspositionTable.EntryType.Exactly, alpha, move);

					//announce new best line if not trivial
                    
                    if (depth.ToPly() > 1 && this.ProgressReported != null)
                    {
                        Progress prog = new Progress(depth.ToPly(), this.CountAIValSearch, alpha, (DateTime.Now - _starttime), _bestvariation, this.board.FENCurrent);
                        OnProgressReported(new SearchProgressEventArgs(prog));
                    }

                    SearchArgs.TimeManager.NewPV(move);
				}
			}
            SearchArgs.TimeManager.EndDepth(depth.ToPly());
		}

        private List<Move> GetLegalPV(FEN fen, IEnumerable<Move> moves)
        {
            List<Move> retval = new List<Move>();
            Board board = new Board(fen);
            foreach (var move in moves)
            {
                //var legalMoves = ChessMoveInfo.GenMovesLegal(board).ToArray();
                if (MoveInfo.GenMovesLegal(board).Contains(move))
                {
                    retval.Add(move);
                    board.MoveApply(move);
                }
                else
                {
                    break;
                }
            }

            while (true)
            {
                Move move;
                int score;
                this.SearchArgs.TransTable.QueryCutoff(board.ZobristBoard, 0, int.MinValue, int.MaxValue, out move, out score);
                if (MoveInfo.GenMovesLegal(board).Contains(move))
                {
                    retval.Add(move);
                    board.MoveApply(move);
                }
                else
                {
                    break;
                }

                if (board.IsDrawByStalemate() || board.IsDrawByRepetition() || board.IsDrawBy50MoveRule())
                {
                    break;
                }
            }
            return retval;
        }

        public virtual void OnProgressReported(SearchProgressEventArgs args)
        {
            var eh = this.ProgressReported;
            if (eh != null)
            {
                eh(this, args);
            }
        }

		private int ValSearchAspiration(SearchDepth depth, int alpha, int estscore, int initWindow, int prev_move_num)
		{
			int windowAlpha = estscore - initWindow;
			int windowBeta = estscore + 1 + initWindow;
			int stage = 0;
			while (true)
			{

				//lower window can never be lower than alpha
				if (windowAlpha > alpha) { windowAlpha = alpha; }

                int score = -ValSearchPVS(depth - 1, 1, prev_move_num, -windowBeta, -windowAlpha);

				if (score <= alpha)
				{
					return alpha;
				}
				if (score > windowAlpha && score < windowBeta)
				{
					return score;
				}

                
                if (initWindow == 0 && score >= windowBeta) 
                {
                    //we are failing high on a non-pv node which is looking better than we thought. extend search time!
                    SearchArgs.TimeManager.FailingHigh();
                }

				//ok, we found a score better than alpha
				//but that falls outside the window we searched, widen the search in the right direction
				stage++;
				if (stage == 1)
				{
					if (score <= windowAlpha) { windowAlpha -= 40; }
					if (score >= windowBeta) { windowBeta += 40; }
				}
				if (stage == 2)
				{
					if (score <= windowAlpha) { windowAlpha -= 200; }
					if (score >= windowBeta) { windowBeta += 200; }
				}
				if (stage >= 3)
				{
					if (score <= windowAlpha) 
					{ 
						windowAlpha -= INFINITY; 
					}
					if (score >= windowBeta) 
					{ 
						windowBeta += INFINITY; 
					}
				}

			}
		}

        public int MarginFutilityPre(SearchDepth depth)
        {
            return 110 + (depth.ToPly() * 35);
        }

        public int MarginFutilityPost(SearchDepth depth)
        {
            return 90 + (depth.ToPly() * 35);
        }

        public int MarginRazor(SearchDepth depth)
        {
            return 170 + (depth.ToPly() * 70);
        }

        private int ValSearchPVS(SearchDepth depth, int ply, int prev_move_num, int alpha, int beta)
        {
            if (alpha == beta - 1)
            {
                return ValSearchMain(depth, ply, prev_move_num, alpha, beta);
            }
            else if (prev_move_num <= 1)
            {
                return ValSearchMain(depth, ply, prev_move_num, alpha, beta);
            }
            else
            {
                int scout = ValSearchMain(depth, ply, prev_move_num, beta - 1, beta);
                if (scout >= beta) { return beta; }
                return ValSearchMain(depth, ply, prev_move_num, alpha, beta);
            }
        }
        
		private int ValSearchMain(SearchDepth depth, int ply, int prev_move_num, int alpha, int beta)
		{
			//for logging
			CountTotalAINodes++;
			CountAIValSearch++;
            SearchArgs.TimeManager.NodeStart(CountAIValSearch); //will end up setting abort flag if over allotted time.
            
            bool isPvNode = (beta - alpha) > 1;

            //check for aborting conditions
            if (_aborting)
            {
                return 0;
            }

            //abort if over max amount of nodes.
            if (CountAIValSearch > this.SearchArgs.MaxNodes)
            {
                _aborting = true;
                return 0;
            }


			//init score variations
			int score = -INFINITY;

			//check for draw
            if (board.PositionRepetitionCount() > 1 || board.IsDrawBy50MoveRule())
			{
                return board.WhosTurn == Player.White ? eval.DrawScore : -eval.DrawScore;
			}



            //fall to queiscent search?
			if (depth.ToPly() <= 0 || ply > MAX_PLY) 
			{
				//MAY TRY: if last move was a null move, want to allow me to do any legal move, because one may be a quiet move that puts me above beta
				return ValSearchQ(ply, alpha, beta);
			}

            //trying to remember why this had to fit in HERE?
            _pvLen[ply] = 0;

            //check trans table
            Move tt_move = Move.EMPTY;
            if (SearchArgs.TransTable.QueryCutoff(board.ZobristBoard, depth.Value(), alpha, beta, out tt_move, out score))
            {
                return score;
            }

            //run static evaluation of current position.
            Evaluation.EvalResults init_info;
            int init_score = _evalInfoStack.EvalFor(ply, board, board.WhosTurn, out init_info, Evaluation.Evaluator.MinValue, Evaluation.Evaluator.MaxValue);

            //detect difference in positional score from previous ply and record max positional gain for previous move
            if(board.MovesSinceNull > 0)
            {
                var previousMove = board.HistMove(1);
                int previousMovePositionalGain = (init_info.PositionalScore - _evalInfoStack[ply - 1].PositionalScore) * (board.WhosTurn == Player.Black ? 1 : -1);
                _moveBuffer.History.RegisterPositionalGain(board.PieceAt(previousMove.To()), previousMove.To(), previousMovePositionalGain);
            }
            
			bool in_check_before_move = board.IsCheck();

            //post futility placeholder
            if (depth.ToPly() <= 5
                && !IsMateScore(beta)
                && !in_check_before_move
                && !isPvNode
                //&& prev_move_num >= 3
                && board.MovesSinceNull > 0
                && init_score > beta + MarginFutilityPost(depth))
            {
                return beta;
            }

            //razoring placeholder.
            if (depth.ToPly() <= 3
                && !in_check_before_move
                && !isPvNode
                && !IsMateScore(beta)
                && init_score + MarginRazor(depth) < alpha)
            {
                int razorAlpha = alpha - MarginRazor(depth);
                int razorScore = ValSearchQ(ply, razorAlpha, razorAlpha + 1);
                if (razorScore <= razorAlpha)
                {
                    return alpha;
                }
            }

			//try null move;
			if (depth.ToPly() > 1
            && !isPvNode
            && init_score > beta
			&& this.NullSearchOK()
			&& !IsMateScore(beta)
			&& (!in_check_before_move)
			&& board.MovesSinceNull > 0)
			{

				int nullr = depth.ToPly() >= 5 ? 3 : 2;
				board.MoveNullApply();
				int nullscore = -ValSearchPVS(depth.SubstractPly(nullr + 1), ply, 0, -beta, -beta + 1);

				if (nullscore >= beta && this.NullSearchVerify() && depth.ToPly() >= 5)
				{
					//doing a second null move restores position to where it was previous to
					//first null, but ensures that null move won't be done in 1st ply of verification
				
					board.MoveNullApply();

					nullscore = ValSearchPVS(depth.SubstractPly(nullr + 2), ply, 0, beta - 1, beta);
					board.MoveNullUndo();
				}
				board.MoveNullUndo();
				if (nullscore >= beta)
				{
					//record in trans table?
					//(board,depth_remaining,TRANSVALUEATLEAST,beta,0);
                    SearchArgs.TransTable.Store(board.ZobristBoard, depth.Value(), TranspositionTable.EntryType.AtLeast, beta, Move.EMPTY);
					return beta;
					//wouldDoNullCutoff = true;
				}
			}



            var plyMoves = _moveBuffer[ply];
            plyMoves.Initialize(board, tt_move, false);

			TranspositionTable.EntryType tt_entryType = TranspositionTable.EntryType.AtMost;

			score = -INFINITY;
			//bool haslegalmove = false;
			int legalMovesTried = 0;
            Move bestmove = Move.EMPTY;

            CheckInfo checkInfo = CheckInfo.Generate(board, board.WhosTurn.PlayerOther());

            ChessMoveData moveData;
            while ((moveData = plyMoves.NextMoveData()).Move != Move.EMPTY)
			{
                Move move = moveData.Move;    

				CurrentVariation[ply] = move;

                var moveGain = _moveBuffer.History.ReadMaxPositionalGain(board.PieceAt(move.From()), move.To());

                //futility check
                if (!isPvNode
                    && moveData.Flags == 0
                    && depth.ToPly() <= 3
                    && move.Promote() == Piece.EMPTY
                    && !in_check_before_move
                    && legalMovesTried > 0
                    && (init_score + moveGain + MarginFutilityPre(depth.SubstractPly(1)) < alpha)
                    && !move.CausesCheck(board, ref checkInfo)
                    )
                {
                    continue;
                }

				board.MoveApply(move);

				
				//check for illegal check
                if (board.IsCheck(board.WhosTurn.PlayerOther()))
				{
					board.MoveUndo();
					continue;
				}
				else
				{
					legalMovesTried++;
				}

                //decide if we want to extend or maybe reduce this node
                SearchDepth ext = 0;
                bool isCheck = board.IsCheck(board.WhosTurn);

                if (isCheck) { ext = this.SearchArgs.ExtensionCheck; }

                Position moveFrom = move.From();
                Position moveTo = move.To();
                Piece movePiece = board.PieceAt(moveFrom);
                
                bool isPawn7th = 
                    (movePiece == Piece.WPawn && moveTo.ToRank() == Rank.Rank7)
                    || (movePiece == Piece.BPawn && moveTo.ToRank() == Rank.Rank2);
                if (isPawn7th) { ext = ext.AddDepth(this.SearchArgs.ExtensionPawn7th); }


                bool isDangerous = moveData.Flags != 0 || isCheck || isPawn7th || ext > 0;

                if (this.SearchArgs.ExtendSEEPositiveOnly)
                {
                    if (moveData.SEE < 0) { ext = 0; }
                }
                
                

                bool doFullSearch = true;

                if (
                    true //SearchArgs.UseLMR == true
                    //&& beta == alpha + 1
                    && depth.ToPly() >= 3
                    && legalMovesTried > 3
                    && !isDangerous)
                {
                    doFullSearch = false;
                    score = -ValSearchPVS(depth.SubstractPly(2), ply + 1, legalMovesTried, -beta, -alpha);
                    if (score > alpha) { doFullSearch = true; }
                }

                if (doFullSearch)
                {
                    //do subsearch
                    score = -ValSearchPVS(depth.AddDepth(ext).SubstractPly(1), ply + 1, legalMovesTried, -beta, -alpha);
                }


				board.MoveUndo();

				if (_aborting) { return 0; }

				if (score >= beta)
				{
                    SearchArgs.TransTable.Store(board.ZobristBoard, depth.Value(), TranspositionTable.EntryType.AtLeast, beta, move);
                    CutoffStats.AtDepth[depth.ToPly()].CutoffAfter[legalMovesTried] += 1;
                    plyMoves.RegisterCutoff(board, moveData, depth);
					return score;
				}

				if (score > alpha)
				{
					tt_entryType = TranspositionTable.EntryType.Exactly;
					alpha = score;
					bestmove = move;

                    //save principal variation
                    _pv[ply][0] = move;
                    if (ply < MAX_PLY)
                    {
                        int subLen = _pvLen[ply + 1];
                        Array.Copy(_pv[ply + 1], 0, _pv[ply], 1, subLen);
                        _pvLen[ply] = subLen + 1;
                    }
                    else
                    {
                        _pvLen[ply] = 1;
                    }
                    
                    
                    
				}
                else
                {
                    plyMoves.RegisterFailLow(board, moveData, depth);
                }
			}

			if (legalMovesTried == 0) //no legal moves?
			{
				if (in_check_before_move)
				{
                    score = -Search.MateIn(ply); // VALCHECKMATE + ply;
				}
				else
				{
					score = 0;
				}
				if (score > alpha) { alpha = score; }
			}


            SearchArgs.TransTable.Store(board.ZobristBoard, depth.Value(), tt_entryType, alpha, bestmove);

            if (tt_entryType == TranspositionTable.EntryType.Exactly)
            {
                CutoffStats.AtDepth[depth.ToPly()].PVNodes += 1;
            }
            else
            {
                CutoffStats.AtDepth[depth.ToPly()].FailLows += 1;
            }

			return alpha;
		}

		private int ValSearchQ(int ply, int alpha, int beta)
		{
			CountTotalAINodes++;
			CountAIQSearch++;

            

			//int oldScore = eval.EvalFor(board, board.WhosTurn);

            Evaluation.EvalResults init_info;
            int init_score = _evalInfoStack.EvalFor(ply, board, board.WhosTurn, out init_info, alpha, beta);

            if (ply > MAX_PLY) { return init_score; }

			bool playerincheck = board.IsCheck();


			if (init_score >= beta && !playerincheck)
			{
				return beta;
			}
			if (init_score > alpha && !playerincheck)
			{
				alpha = init_score;
			}

            var plyMoves = _moveBuffer[ply];
            plyMoves.Initialize(board, Move.EMPTY, !playerincheck);

			//ChessMove.Comp moveOrderer = new ChessMove.Comp(board,ChessMoveInfo.Create(),false);
			//moves.Sort(moveOrderer);


			int tried_move_count = 0;
            ChessMoveData moveData;
            while ((moveData = plyMoves.NextMoveData()).Move != Move.EMPTY)
			{
                Move move = moveData.Move;

                //futility check
                if (!playerincheck)
                {
                    if (moveData.SEE < 0) { continue; }

                    var capPieceVal = board.PieceAt(move.To()).PieceValBasic();
                    if (init_score + capPieceVal < alpha) { continue; }
                }

                CurrentVariation[ply] = move;

				board.MoveApply(move);

                if (board.IsCheck(board.WhosTurn.PlayerOther()))
				{
					board.MoveUndo();
					continue;
				}


				tried_move_count++;

				int move_score = -ValSearchQ(ply + 1, -beta, -alpha);

				//check for blunder
                bool isRecapture = (move.To() == board.HistMove(2).To());
                if (tried_move_count > 1 && SearchArgs.Blunder.DoBlunder(board.ZobristBoard ^ this.BlunderKey, 0, isRecapture, tried_move_count, alpha, beta))
                {
                    if (!playerincheck) //weird behavior if doing blunder when player in check
                    {
                        board.MoveUndo();
                        continue;
                    }
                }

				board.MoveUndo();

				if (move_score >= beta)
				{
					return move_score;
				}
				if (move_score > alpha)
				{
					alpha = move_score;
				}
			}
			//trans_table_store(board,0,entrytype,alpha,0);
			if (playerincheck && tried_move_count == 0)
			{
                alpha = -Search.MateIn(ply);// VALCHECKMATE + ply;
			}
			
			return alpha;

		}

		private bool NullSearchOK()
		{
			if (board.PieceCount(Player.White, PieceType.Knight) == 0
            && board.PieceCount(Player.White, PieceType.Bishop) == 0
            && board.PieceCount(Player.White, PieceType.Rook) == 0
            && board.PieceCount(Player.White, PieceType.Queen) == 0)
			{
				return false;
			}
            if (board.PieceCount(Player.Black, PieceType.Knight) == 0
            && board.PieceCount(Player.Black, PieceType.Bishop) == 0
            && board.PieceCount(Player.Black, PieceType.Rook) == 0
            && board.PieceCount(Player.Black, PieceType.Queen) == 0)
			{
				return false;
			}
			return true;
		}
		private bool NullSearchVerify()
		{
            int wmat = (board.PieceCount(Player.White, PieceType.Knight) * 3)
                + (board.PieceCount(Player.White, PieceType.Bishop) * 3)
                + (board.PieceCount(Player.White, PieceType.Rook) * 5)
                + (board.PieceCount(Player.White, PieceType.Queen) * 9);

            int bmat = (board.PieceCount(Player.Black, PieceType.Knight) * 3)
                + (board.PieceCount(Player.Black, PieceType.Bishop) * 3)
                + (board.PieceCount(Player.Black, PieceType.Rook) * 5)
                + (board.PieceCount(Player.Black, PieceType.Queen) * 9);

			if (wmat < 9 || bmat < 9)
			{
				return true;
			}
			return false;
		}
	}
}
