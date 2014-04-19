using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Sinobyl.Engine
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


	public class ChessSearch
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

		#region helper classes
		public class Progress
		{
			public readonly int Depth;
			public readonly int Nodes;
			public readonly int Score;
			public readonly TimeSpan Time;
			public readonly ReadOnlyCollection<ChessMove> PrincipleVariation;
            public readonly ChessFEN FEN;
			public Progress(int a_depth, int a_nodes, int a_score, TimeSpan a_time, ChessMoves a_pv, ChessFEN a_fen)
			{
                if (a_pv == null) { throw new ArgumentNullException("a_pv"); }
                if (a_fen == null) { throw new ArgumentNullException("a_fen"); }
				Depth = a_depth;
				Nodes = a_nodes;
				Score = a_score;
				Time = a_time;
				PrincipleVariation = new ReadOnlyCollection<ChessMove>(a_pv);
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

			public double CalcBlunderPct(int depth, bool isRecapture, int moveNum, int moveCount)
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

			public bool DoBlunder(Int64 Zob, int depth, bool isRecapture, int moveNum, int moveCount, int alpha, int beta)
			{
                if (alpha < -ChessSearch.MateIn(5) || beta > ChessSearch.MateIn(5)) { return false; }
				if (moveNum < this.BlunderSkipCount) { return false; }

				
				double MoveChanceKey = ((double)(Zob & 255)) / 255;
				if (MoveChanceKey < 0) { MoveChanceKey = -MoveChanceKey; }
				double blunderchance = this.CalcBlunderPct(depth, isRecapture,moveNum, moveCount);
				if (MoveChanceKey < blunderchance)
				{
					return true;
				}
				return false;
			}

		}

		public class Args
		{
			public ChessFEN GameStartPosition { get; set; }
			public ChessMoves GameMoves { get; set; }
			public int MaxDepth { get; set; }
			public int NodesPerSecond { get; set; }
			public ChessTrans TransTable { get; set; }
			public BlunderChance Blunder { get; set; }
			public TimeSpan Delay { get; set; }
            public IChessEval Eval { get; set; }
            public int MaxNodes { get; set; }
            public int ContemptForDraw { get; set; }
            public ITimeManager TimeManager { get; set; }
			public Args()
			{
                GameStartPosition = new ChessFEN(ChessFEN.FENStart);
				GameMoves = new ChessMoves();
				MaxDepth = int.MaxValue;
				NodesPerSecond = int.MaxValue;
				Blunder = new BlunderChance();
				Delay = new TimeSpan(0);
                Eval = ChessEval.Default;
                MaxNodes = int.MaxValue;
                ContemptForDraw = 40;
                TimeManager = new TimeManagerAnalyze();
			}
		}

		#endregion

		public event EventHandler<SearchProgressEventArgs> ProgressReported;

		public readonly Args SearchArgs;
		private readonly ChessBoard board;
		private readonly IChessEval eval;
		private ChessMove[] CurrentVariation = new ChessMove[50];
		private DateTime _starttime;
		private bool _aborting = false;
		private bool _returnBestResult = true; //if false return null
		private ChessMoves _bestvariation = new ChessMoves();
		private int _bestvariationscore = 0;
        private int[] _contemptForDrawForPlayer = new int[3];
        private readonly ChessMoveBuffer _moveBuffer = new ChessMoveBuffer();
        private ChessMove[] _currentPV = new ChessMove[50];

		private readonly Int64 BlunderKey = Rand64();
		
		public ChessSearch(Args args)
		{
			SearchArgs = args;
			board = new ChessBoard(SearchArgs.GameStartPosition);
            eval = args.Eval;
            
			foreach (ChessMove histmove in SearchArgs.GameMoves)
			{
				board.MoveApply(histmove);
			}

            if (board.WhosTurn == ChessPlayer.White)
            {
                _contemptForDrawForPlayer[(int)ChessPlayer.White] = args.ContemptForDraw;
                _contemptForDrawForPlayer[(int)ChessPlayer.Black] = -args.ContemptForDraw;
            }
            else
            {
                _contemptForDrawForPlayer[(int)ChessPlayer.White] = -args.ContemptForDraw;
                _contemptForDrawForPlayer[(int)ChessPlayer.Black] = args.ContemptForDraw;
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

		public Progress Search()
		{

			_starttime = DateTime.Now;		

			Thread.Sleep(this.SearchArgs.Delay);

			SearchArgs.TransTable.AgeEntries(2);

            SearchArgs.TimeManager.StopSearch += TimeManager_StopSearch;
            SearchArgs.TimeManager.RequestNodes += TimeManager_RequestNodes;
            SearchArgs.TimeManager.StartSearch();

			int depth = 1;

			int MateScoreLast = 0;
			int MateScoreCount = 0;


            while (depth <= this.SearchArgs.MaxDepth)
            {

                ValSearchRoot(depth);

                //if we get three consecutive depths with same mate score.. just move.
                if (_bestvariationscore > ChessSearch.MateIn(10) || _bestvariationscore < -ChessSearch.MateIn(10))
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
                if (depth > 10 && _bestvariation != null && _bestvariation.Count() < depth - 6 && _bestvariationscore == -_contemptForDrawForPlayer[(int)board.WhosTurn])
                {
                    _aborting = true;
                }


                if (_aborting) { break; }

                depth++;
            }

			CountTotalAITime += (DateTime.Now - _starttime);

            SearchArgs.TimeManager.StopSearch -= TimeManager_StopSearch;
            SearchArgs.TimeManager.RequestNodes -= TimeManager_RequestNodes;
            
			//return progress
			if (_returnBestResult)
			{
				Progress progress = new Progress(depth, CountAIValSearch, _bestvariationscore, DateTime.Now - _starttime, _bestvariation, this.board.FEN);
				return progress;
			}
			else
			{
				return null;
			}
		}


        void TimeManager_RequestNodes(object sender, TimeManagerRequestNodesEventArgs e)
        {
            e.NodeCount = this.CountAIValSearch;
        }

        void TimeManager_StopSearch(object sender, EventArgs e)
        {
            _aborting = true;
        }

		private void ValSearchRoot(int depth)
		{

            SearchArgs.TimeManager.StartDepth(depth);

			//set trans table entries
			SearchArgs.TransTable.StoreVariation(board, _bestvariation);
            
			//get trans table move
			int tt_score = 0;
            ChessMove tt_move = ChessMove.EMPTY;
            SearchArgs.TransTable.QueryCutoff(board.Zobrist, depth, -INFINITY, INFINITY, out tt_move, out tt_score);
			

			bool in_check_before_move = board.IsCheck();

            var plyMoves = _moveBuffer[0];
            plyMoves.Initialize(board);
            plyMoves.Sort(board, true, tt_move);

			int alpha = -INFINITY;
			int beta = INFINITY;

            ChessMove bestmove = ChessMove.EMPTY;

            foreach (ChessMove move in plyMoves.SortedMoves())
			{
				CurrentVariation[0] = move;

                for (int i = _currentPV.GetLowerBound(0); i <= _currentPV.GetUpperBound(0); i++)
                {
                    _currentPV[i] = ChessMove.EMPTY;
                }

                board.MoveApply(move);

                if (board.IsCheck(board.WhosTurn.PlayerOther()))
                {
                    board.MoveUndo();
                    continue;
                }

				int score = 0;

                SearchArgs.TimeManager.StartMove(move);
				
				if (depth <= 3)
				{
					//first couple nodes search full width
					score = -ValSearch(depth - 1, 1, -beta, -alpha);
				}
				else if (_bestvariation != null && _bestvariation.Count > 0 && move == this._bestvariation[0])
				{
					//doing first move of deeper search

					score = ValSearchAspiration(depth, alpha, _bestvariationscore, 30);
				}
				else
				{
					//doing search of a move we believe is going to fail low
					score = ValSearchAspiration(depth, alpha, alpha, 0);
				}

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

                    _currentPV[0] = move;

					//save instance best info
					_bestvariation = new ChessMoves(GetLegalPV(this.board.FEN, _currentPV));
					_bestvariationscore = alpha;

					//store to trans table
                    SearchArgs.TransTable.Store(board.Zobrist, depth, ChessTrans.EntryType.Exactly, alpha, move);

					//announce new best line if not trivial
                    Progress prog = new Progress(depth, this.CountAIValSearch, alpha, (DateTime.Now - _starttime), _bestvariation, this.board.FEN);
                    if (depth > 1 && this.ProgressReported != null)
                    {
                        OnProgressReported(new SearchProgressEventArgs(prog));
                    }

                    SearchArgs.TimeManager.NewPV(move);
				}
			}
            SearchArgs.TimeManager.EndDepth(depth);
		}

        private List<ChessMove> GetLegalPV(ChessFEN fen, IEnumerable<ChessMove> moves)
        {
            List<ChessMove> retval = new List<ChessMove>();
            ChessBoard board = new ChessBoard(fen);
            foreach (var move in moves)
            {
                //var legalMoves = ChessMoveInfo.GenMovesLegal(board).ToArray();
                if (ChessMoveInfo.GenMovesLegal(board).Contains(move))
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
                ChessMove move;
                int score;
                this.SearchArgs.TransTable.QueryCutoff(board.Zobrist, 0, int.MinValue, int.MaxValue, out move, out score);
                if (ChessMoveInfo.GenMovesLegal(board).Contains(move))
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

		private int ValSearchAspiration(int depth, int alpha, int estscore, int initWindow)
		{
			int windowAlpha = estscore - initWindow;
			int windowBeta = estscore + 1 + initWindow;
			int stage = 0;
			while (true)
			{

				//lower window can never be lower than alpha
				if (windowAlpha > alpha) { windowAlpha = alpha; }

				int score = -ValSearch(depth - 1, 1, -windowBeta, -windowAlpha);

				if (score <= alpha)
				{
					return alpha;
				}
				if (score > windowAlpha && score < windowBeta)
				{
					return score;
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


		private int ValSearch(int depth, int ply, int alpha, int beta)
		{
			//for logging
			CountTotalAINodes++;
			CountAIValSearch++;
            SearchArgs.TimeManager.NodeStart(CountAIValSearch); //will end up setting abort flag if over allotted time.

            if (_aborting)
            {
                return 0;
            }

            if (CountAIValSearch > this.SearchArgs.MaxNodes)
            {
                _aborting = true;
                return 0;
            }

			if ((CountAIValSearch & 0xFFF) == 0)
			{
				DateTime now = DateTime.Now;
				TimeSpan timeSpent = now - _starttime;
				TimeSpan amountOfTimeWeShouldHaveSpentToGetThisFar = TimeSpan.FromMilliseconds(1000 * (float)(CountAIQSearch + CountAIValSearch) / (float)SearchArgs.NodesPerSecond);

				if (amountOfTimeWeShouldHaveSpentToGetThisFar > timeSpent)
				{
					TimeSpan sleepFor = amountOfTimeWeShouldHaveSpentToGetThisFar - timeSpent;
					Thread.Sleep(sleepFor);
				}
				
			}

			//init score variations
			int score = -INFINITY;

			//check for draw
			if (board.IsDrawByRepetition() || board.IsDrawBy50MoveRule())
			{
                return -_contemptForDrawForPlayer[(int)board.WhosTurn];
			}

			if (depth <= 0) 
			{
				//MAY TRY: if last move was a null move, want to allow me to do any legal move, because one may be a quiet move that puts me above beta
				return ValSearchQ(ply, alpha, beta);
			}

			//check trans table
            ChessMove tt_move = ChessMove.EMPTY;
            if (SearchArgs.TransTable.QueryCutoff(board.Zobrist, depth, alpha, beta, out tt_move, out score))
			{
                ////if last two moves were null this is a verification search. 
                ////do not allow cutoff here as original search may have had null cutoff
                ////which is what we are trying to get around.
                //if (!board.LastTwoMovesNull())
                //{
                //    return score;
                //}
                return score;
				
			}

			bool in_check_before_move = board.IsCheck();

			//try null move;
			if (depth > 1
			&& this.NullSearchOK()
			&& beta < 10000
			&& (!in_check_before_move)
			&& board.MovesSinceNull > 0)
			{

				int nullr = depth>=5 ? 3 : 2;
				board.MoveNullApply();
				int nullscore = -ValSearch(depth - nullr - 1, ply, -beta, -beta + 1);

				if (nullscore >= beta && this.NullSearchVerify() && depth >= 5)
				{
					//doing a second null move restores position to where it was previous to
					//first null, but ensures that null move won't be done in 1st ply of verification
				
					board.MoveNullApply();

					nullscore = ValSearch(depth - nullr - 2, ply, beta - 1, beta);
					board.MoveNullUndo();
				}
				board.MoveNullUndo();
				if (nullscore >= beta)
				{
					//record in trans table?
					//(board,depth_remaining,TRANSVALUEATLEAST,beta,0);
                    SearchArgs.TransTable.Store(board.Zobrist, depth, ChessTrans.EntryType.AtLeast, beta, ChessMove.EMPTY);
					return beta;
					//wouldDoNullCutoff = true;
				}
			}



            var plyMoves = _moveBuffer[ply];
            plyMoves.Initialize(board);
            plyMoves.Sort(board, true, tt_move);

            //ChessMoves moves = new ChessMoves(ChessMove.GenMoves(board));
			//ChessMove.Comp moveOrderer = new ChessMove.Comp(board,tt_move,depth > 3);
			//moves.Sort(moveOrderer);


			ChessTrans.EntryType tt_entryType = ChessTrans.EntryType.AtMost;

			score = -INFINITY;
			//bool haslegalmove = false;
			int legalMovesTried = 0;
			int blunders = 0;
            ChessMove bestmove = ChessMove.EMPTY;


            ChessMove move;
            while ((move = plyMoves.NextMove()) != ChessMove.EMPTY)
			{
				CurrentVariation[ply] = move;

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

				

				//do subsearch
				score = -ValSearch(depth - 1, ply + 1, -beta, -alpha);


				//check for blunder
                bool isRecapture = (move.To() == board.HistMove(2).To());
                if (SearchArgs.Blunder.DoBlunder(board.Zobrist ^ this.BlunderKey, depth, isRecapture, legalMovesTried, plyMoves.MoveCount, alpha, beta))
                {
                    if (!in_check_before_move)  //weird behavior if doing blunder when player in check
                    {
                        blunders++;
                        board.MoveUndo();
                        continue;
                    }
                }

				board.MoveUndo();

				

				if (_aborting) { return 0; }

				if (score >= beta)
				{
                    SearchArgs.TransTable.Store(board.Zobrist, depth, ChessTrans.EntryType.AtLeast, beta, move);
                    CutoffStats.AtDepth[depth].CutoffAfter[legalMovesTried] += 1;
                    plyMoves.RegisterCutoff(board, move);
					return score;
				}
				if (score > alpha)
				{
					tt_entryType = ChessTrans.EntryType.Exactly;
					alpha = score;
					bestmove = move;

                    _currentPV[ply] = move;
                    
				}
			}

			if (legalMovesTried == 0) //no legal moves?
			{
				if (in_check_before_move)
				{
                    score = -ChessSearch.MateIn(ply); // VALCHECKMATE + ply;
				}
				else
				{
					score = 0;
				}
				if (score > alpha) { alpha = score; }
			}


            SearchArgs.TransTable.Store(board.Zobrist, depth, tt_entryType, alpha, bestmove);

            if (tt_entryType == ChessTrans.EntryType.Exactly)
            {
                CutoffStats.AtDepth[depth].PVNodes += 1;
            }
            else
            {
                CutoffStats.AtDepth[depth].FailLows += 1;
            }

			return alpha;
		}

		private int ValSearchQ(int ply, int alpha, int beta)
		{
			CountTotalAINodes++;
			CountAIQSearch++;

			int init_score = eval.EvalFor(board, board.WhosTurn);
			bool playerincheck = board.IsCheck();


			if (init_score > beta && !playerincheck)
			{
				return beta;
			}
			if (init_score > alpha && !playerincheck)
			{
				alpha = init_score;
			}

            var plyMoves = _moveBuffer[ply];
            plyMoves.Initialize(board, !playerincheck);
            plyMoves.Sort(board, false, ChessMove.EMPTY);

			//ChessMove.Comp moveOrderer = new ChessMove.Comp(board,ChessMoveInfo.Create(),false);
			//moves.Sort(moveOrderer);


			int tried_move_count = 0;
            ChessMove move;
            while ((move = plyMoves.NextMove()) != ChessMove.EMPTY)
			{

				//todo: fulitity check 
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
                if (tried_move_count > 1 && SearchArgs.Blunder.DoBlunder(board.Zobrist ^ this.BlunderKey, 0, isRecapture, tried_move_count, plyMoves.MoveCount, alpha, beta))
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

                    _currentPV[ply] = move;
				}
			}
			//trans_table_store(board,0,entrytype,alpha,0);
			if (playerincheck && tried_move_count == 0)
			{
                alpha = -ChessSearch.MateIn(ply);// VALCHECKMATE + ply;
			}
			
			return alpha;

		}

		private bool NullSearchOK()
		{
			if (board.PieceCount(ChessPiece.WKnight) == 0
			&& board.PieceCount(ChessPiece.WBishop) == 0
			&& board.PieceCount(ChessPiece.WRook) == 0
			&& board.PieceCount(ChessPiece.WQueen) == 0)
			{
				return false;
			}
			if (board.PieceCount(ChessPiece.BKnight) == 0
			&& board.PieceCount(ChessPiece.BBishop) == 0
			&& board.PieceCount(ChessPiece.BRook) == 0
			&& board.PieceCount(ChessPiece.BQueen) == 0)
			{
				return false;
			}
			return true;
		}
		private bool NullSearchVerify()
		{
			int wmat = (board.PieceCount(ChessPiece.WKnight) * 3)
				+ (board.PieceCount(ChessPiece.WBishop) * 3)
				+ (board.PieceCount(ChessPiece.WRook) * 5)
				+ (board.PieceCount(ChessPiece.WQueen) * 9);

			int bmat = (board.PieceCount(ChessPiece.BKnight) * 3)
				+ (board.PieceCount(ChessPiece.BBishop) * 3)
				+ (board.PieceCount(ChessPiece.BRook) * 5)
				+ (board.PieceCount(ChessPiece.BQueen) * 9);

			if (wmat < 9 || bmat < 9)
			{
				return true;
			}
			return false;
		}
	}
}
