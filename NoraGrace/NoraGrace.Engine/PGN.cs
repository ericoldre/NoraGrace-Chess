using System;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NoraGrace.Engine
{


	public class ChessPGNHeaders : Dictionary<string,string>
	{
        public ChessPGNHeaders(): base(StringComparer.InvariantCultureIgnoreCase)
        {

        }

        public static bool TryParse(string headerLine, out KeyValuePair<string,string> header)
        {
            string pattern = @"\[(?<key>[\w]+)\s+\""(?<value>[\S\s]+)\""\]";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(headerLine);
            if (matches.Count != 1)
            {
                header = new KeyValuePair<string, string>();
                return false;
                //throw new ArgumentException("not a valid pgn header: " + headerline);
            }
            System.Text.RegularExpressions.Match match = matches[0];
            header = new KeyValuePair<string, string>(match.Groups["key"].Value, match.Groups["value"].Value);
            return true;
        }

        public static KeyValuePair<string, string> Parse(string headerLine)
        {
            KeyValuePair<string, string> retval;
            if (TryParse(headerLine, out retval))
            {
                return retval;
            }
            else
            {
                throw new ArgumentException("not a valid pgn header: " + headerLine, "headerLine");
            }
        }
        
	}

	public class PGN
	{

		
		private readonly List<Move> _moves = new List<Move>();
		private GameResult? _result;
		private GameResultReason _resultReason;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<int, string> _comments = new Dictionary<int,string>();



        public PGN(IEnumerable<KeyValuePair<string, string>> headers, IEnumerable<Move> moves, GameResult? result, IEnumerable<KeyValuePair<int, string>> comments, GameResultReason reason)
		{
            if (_headers != null)
            {
                foreach (var header in headers)
                {
                    _headers.Add(header.Key, header.Value);
                }
            }


            _moves.AddRange(moves);
			_result = result;
            if (comments != null)
            {
                foreach (var comment in comments)
                {
                    _comments.Add(comment.Key, comment.Value);
                }
            }
			_resultReason = reason;
		}
        public PGN(IEnumerable<KeyValuePair<string, string>> headers, Board board)
		{
            if (_headers != null)
            {
                foreach (var header in headers)
                {
                    _headers.Add(header.Key, header.Value);
                }
            }
            _moves.AddRange(board.HistoryMoves);
			_result = null;
			_resultReason = GameResultReason.NotDecided;
		}

        //public ChessPGN(ChessGame game)
        //{
        //    _headers = new ChessPGNHeaders();
        //    _headers.Add(new ChessPGNHeader("White", game.PlayerWhite.Name));
        //    _headers.Add(new ChessPGNHeader("Black", game.PlayerBlack.Name));
        //    _headers.Add(new ChessPGNHeader("Date", DateTime.Now.ToShortDateString()));

        //    //_headers.Add(new ChessPGNHeader("TotalNodes", ChessSearch.CountTotalAINodes.ToString()));
        //    //_headers.Add(new ChessPGNHeader("TotalEvals", ChessEval.TotalAIEvals.ToString()));
        //    //_headers.Add(new ChessPGNHeader("TotalAITime", ChessSearch.CountTotalAITime.ToString()));
        //    //double nodesPerSec = ChessSearch.CountTotalAINodes / ChessSearch.CountTotalAITime.TotalSeconds;

        //    //_headers.Add(new ChessPGNHeader("Nodes/Sec", nodesPerSec.ToString()));


        //    _moves = new ReadOnlyCollection<ChessMove>(game.MoveHistory());
        //    _result = game.Result;
        //    _resultReason = game.ResultReason;
        //    _comments = new List<ChessPGNComment>();


        //}

        public Dictionary<string, string> Headers
        {
            get
            {
                return _headers;
            }
        }

		public List<Move> Moves
		{
			get
			{
				return _moves;
			}
		}
        //public List<string> HeaderKeys
        //{
        //    get
        //    {
        //        List<string> retval = new List<string>();
        //        foreach (var header in _headers)
        //        {
        //            retval.Add(header.Key);
        //        }
        //        return retval;
        //    }
        //}
        //public string HeaderValue(string key)
        //{
        //    ChessPGNHeader h = null;
        //    foreach(var h1 in _headers)
        //    {
        //        if (h1.Key.ToLower() == key.ToLower()) { h = h1; }
        //    }
			
        //    if (h == null)
        //    {
        //        return "";
        //    }
        //    else
        //    {
        //        return h.Value;
        //    }
        //}
		public string StartingPosition
		{
			get
			{
                if (Headers.ContainsKey("FEN"))
                {
                    return Headers["FEN"];
                }
                else
                {
                    return FEN.FENStart;
                }
			}
		}

		public GameResult? Result
		{
			get
			{
				return _result;
			}
		}
		public GameResultReason ResultReason
		{
			get
			{
				return _resultReason;
			}
		}
		public string White
		{
			get
			{
				return this.Headers["White"];
			}
		}
		public string Black
		{
			get
			{
				return this.Headers["Black"];
			}
		}
		public int? WhiteElo
		{
			get
			{
				int retval;
				if (int.TryParse(this.Headers["WhiteElo"], out retval))
				{
					return retval;
				}
				else
				{
					return null;
				}
			}
		}
		public int? BlackElo
		{
			get
			{
				int retval;
				if (int.TryParse(this.Headers["BlackElo"], out retval))
				{
					return retval;
				}
				else
				{
					return null;
				}
			}
		}
		public DateTime? Date
		{
			get
			{
				DateTime ret;
				if (DateTime.TryParse(this.Headers["Date"], out ret))
				{
					return ret;
				}
				else if (DateTime.TryParse(this.Headers["EventDate"], out ret))
				{
					return ret;
				}
				else
				{
					return null;
				}
			}
		}
		public string Site
		{
			get
			{
				return this.Headers["Site"];
			}
		}

		public string EventName
		{
			get
			{
				return this.Headers["Event"];
			}
		}

        public Dictionary<int, string> Comments
        {
            get { return _comments; }
        }

        //public ChessPGNComment CommentForPly(int iPly)
        //{
        //    foreach (ChessPGNComment comment in this._comments)
        //    {
        //        if (comment.MoveNum == iPly)
        //        {
        //            return comment;
        //        }
        //    }
        //    return null;
        //}


		public override string ToString()
		{
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            Write(writer);
            return sb.ToString();
		}

		public void Write(System.IO.TextWriter writer, int MaxLineLen = 90)
		{
			Board board = new Board(this.StartingPosition);

			StringBuilder sbMoves = new StringBuilder();
			StringBuilder sbHeaders = new StringBuilder();

            ////result
            //string sResult = "*";
            //switch (_result)
            //{
            //    case ChessResult.WhiteWins: sResult = "1-0"; break;
            //    case ChessResult.BlackWins: sResult = "0-1"; break;
            //    case ChessResult.Draw: sResult = "1/2-1/2"; break;
            //    case null: sResult = "*"; break;
            //}

			//headers
			foreach (var header in _headers)
			{
				sbHeaders.AppendLine(string.Format(@"[{0} ""{1}""]", header.Key, header.Value));// header.ToString() + Environment.NewLine);
			}
			if (!_headers.ContainsKey("Result"))
			{
				//sbHeaders.Append(new ChessPGNHeader("Result", sResult).ToString() + Environment.NewLine);
                sbHeaders.Append(string.Format(@"[Result ""{0}""]", _headers["Result"]) + Environment.NewLine);
			}
			sbHeaders.Append(Environment.NewLine);

            ////write any comments before moves
            //ChessPGNComment comment = this.CommentForPly(0);
            //if (comment != null)
            //{
            //    sbMoves.Append("{");
            //    sbMoves.Append(comment.Text);
            //    sbMoves.Append("} ");
            //}

			//moves
			int imove = 1;
			bool iswhite = true;
			foreach (Move move in _moves)
			{
				if (iswhite)
				{
					int ifullmove = (imove / 2) + 1;
					sbMoves.Append(ifullmove.ToString() + ". ");
				}
				sbMoves.Append(move.Description(board) + " ");
				board.MoveApply(move);
                
                if (Comments.ContainsKey(imove - 1))
				{
                    string comment = this.Comments[imove - 1];
					sbMoves.Append("{");
					sbMoves.Append(comment);
					sbMoves.Append("} ");
				}
				iswhite = !iswhite;
				imove++;
			}
			//result
			if (_result == GameResult.WhiteWins)
			{
				sbMoves.Append("1-0 ");
			}
			else if (_result == GameResult.BlackWins)
			{
				sbMoves.Append("0-1 ");
			}
			else if (_result == GameResult.Draw)
			{
				sbMoves.Append("1/2-1/2 ");
			}
			else if (_result == null)
			{
				sbMoves.Append("* ");
			}

			//reformat move section to break into lines.
			string[] movewords = sbMoves.ToString().Split(' ');
			sbMoves = new StringBuilder();
			string currline = "";
			foreach (string word in movewords)
			{
				if (currline == "")
				{
					currline += word;
				}
				else if (currline.Length + word.Length > MaxLineLen)
				{
					sbMoves.Append(currline + Environment.NewLine);
					currline = word;
				}
				else
				{
					currline += " " + word;
				}
			}
			sbMoves.Append(currline + Environment.NewLine);
			//return result
            writer.Write(sbHeaders.ToString());
            writer.Write(sbMoves.ToString());
            writer.WriteLine();
			//return sbHeaders.ToString() + sbMoves.ToString();
		}


        public static IEnumerable<PGN> AllGames(System.IO.StreamReader reader)
        {
            while (true)
            {
                PGN pgn = NextGame(reader);
                if (pgn == null) { break; }
                yield return pgn;
            }
        }

        public static IEnumerable<PGN> AllGames(System.IO.FileInfo file)
        {
            using (var reader = new System.IO.StreamReader(file.FullName))
            {
                while (true)
                {
                    PGN pgn = NextGame(reader);
                    if (pgn == null) { break; }
                    yield return pgn;
                }
            }
        }

		public static PGN NextGame(string PGNString)
		{
            System.IO.MemoryStream memory = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(PGNString));
            System.IO.StreamReader reader = new System.IO.StreamReader(memory);
			return NextGame(reader);
		}
        public static PGN NextGame(System.IO.StreamReader reader)
		{
			ChessPGNHeaders headers = new ChessPGNHeaders();
            Dictionary<int, string> comments = new Dictionary<int, string>();
            List<Move> moves = new List<Move>();
			GameResult? result = null;
			GameResultReason reason = GameResultReason.NotDecided;
			Board board = new Board();


			int headerlevel = 0;
			int commentlevel = 0;
			bool processtok = false;
			bool gamedone = false;
			string token = "";


			string line;
			while (!gamedone || commentlevel != 0)
			{
                line = reader.ReadLine();
                if (line == null) 
                {
                    break; 
                }
				line += " ";
				char[] cArray = line.ToCharArray();
				foreach (char c in cArray)
				{
					if (c == '[')
					{
						//begin new header
						headerlevel++;
					}
					else if (c == ']' && headerlevel > 0)
					{
						//end header
						headerlevel--;
						if (headerlevel == 0)
						{
                            var header = ChessPGNHeaders.Parse("[" + token + "]");
							headers.Add(header.Key, header.Value);
							if (header.Key.ToUpper() == "FEN")
							{
								board = new Board(header.Value);
							}
							token = "";
						}
					}
					else if (headerlevel != 0)
					{
						//in header
						token += c.ToString();
					}
					else if (c == '(' || c == '{')
					{
						//begin comment
						commentlevel++;
					}
					else if (c == ')' || c == '}')
					{
						//end comment;
						commentlevel--;
						if (commentlevel == 0)
						{
							//comments.Add(new ChessPGNComment(moves.Count, token));
                            if (comments.ContainsKey(moves.Count - 1))
                            {
                                token = comments[moves.Count - 1] + " " + token;
                                comments.Remove(moves.Count - 1);
                            }
                            comments.Add(moves.Count - 1, token);
                            
						}
						token = "";
					}
					else if (commentlevel != 0)
					{
						token += c;
						//in comments don't do anything
					}
					else if (c == '.')
					{
						//a '.' means that we encountered a move number toss this away.
						token = "";
					}
					else if (c == ' ')
					{
						processtok = true;
					}
					else
					{
						token += c;
					}
					//
					if (processtok && token == "")
					{
						processtok = false;
					}
					else if (processtok)
					{
						processtok = false;
						bool OkToProcessResults = (headers.Count > 0 || moves.Count > 0);
						//must have some moves or headers before processing results
						//otherwise you MAY pick up a result on a new line after previous game

						if (token.Trim() == "1/2-1/2")
						{
							if (OkToProcessResults)
							{
								result = GameResult.Draw;
								if (board.IsDrawByStalemate()) { reason = GameResultReason.Stalemate; }
								else if (board.IsDrawByRepetition()) { reason = GameResultReason.Repetition; }
								else if (board.IsDrawBy50MoveRule()) { reason = GameResultReason.FiftyMoveRule; }
								else { reason = GameResultReason.MutualAgreement; }

								gamedone = true;
							}
						}
						else if (token.Trim() == "1-0")
						{
							if (OkToProcessResults)
							{
								result = GameResult.WhiteWins;
								if (board.IsMate()) { reason = GameResultReason.Checkmate; }
								else { reason = GameResultReason.Resign; }
								gamedone = true;
							}
						}
						else if (token.Trim() == "0-1")
						{
							if (OkToProcessResults)
							{
								result = GameResult.BlackWins;
								if (board.IsMate()) { reason = GameResultReason.Checkmate; }
								else { reason = GameResultReason.Resign; }
								gamedone = true;
							}
						}
						else if (token.Trim() == "*")
						{
							if (OkToProcessResults)
							{
								result = null;
								gamedone = true;
							}
						}
						else
						{
							Move move = MoveUtil.Parse(board, token);
							board.MoveApply(move);
							moves.Add(move);

							//check for mate.
							if (board.IsMate())
							{
								if (board.WhosTurn == Player.White)
								{
									result = GameResult.BlackWins;
									reason = GameResultReason.Checkmate;
								}
								else
								{
									result = GameResult.WhiteWins;
									reason = GameResultReason.Checkmate;
								}
								gamedone = true;
							}

						}
						token = "";
					}
				}
			}
			if (moves.Count > 0 || headers.Count > 0)
			{
				return new PGN(headers, moves, result, comments, reason);
			}
			else
			{
				return null;
			}

		}
	}

    public static class ExtensionsChessPGN
    {
        public static string Summary(this IEnumerable<PGN> PGNs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var winnerGroup in PGNs.GroupBy(pgn => new { Winner = pgn.Result == GameResult.WhiteWins ? pgn.White : pgn.Result == GameResult.BlackWins ? pgn.Black : "Draw" }))
            {
                sb.AppendLine(string.Format("{0,-20} Wins:{1,-4} AvgLen:{2,-5}", winnerGroup.Key.Winner, winnerGroup.Count(), winnerGroup.Average(pgn => pgn.Moves.Count())));
                foreach (var reasongroup in winnerGroup.GroupBy(pgn => pgn.ResultReason))
                {
                    sb.AppendLine(string.Format("\t{0,-20} Wins:{1,-4} AvgLen:{2,-5}", reasongroup.Key, reasongroup.Count(), reasongroup.Average(pgn => pgn.Moves.Count())));

                    foreach (var gameLenGroup in reasongroup.GroupBy(pgn => (pgn.Moves.Count / 10) * 10).OrderBy(pgn => pgn.Key))
                    {
                        sb.AppendLine(string.Format("\t\t {0}-{1} Wins:{2,-4} Min:{3} Max:{4}", gameLenGroup.Key, gameLenGroup.Key + 9, gameLenGroup.Count(), gameLenGroup.Min(pgn => pgn.Moves.Count()), gameLenGroup.Max(pgn => pgn.Moves.Count())));
                    }
                }
            }
            return sb.ToString();
        }

        public static void ResultsForPlayer(this IEnumerable<PGN> PGNs, string playerName, out int wins, out int losses, out int draws)
        {
            wins=0;
            losses=0;
            draws=0;
            foreach (PGN pgn in PGNs)
            {
                if (playerName == pgn.White)
                {
                    if (pgn.Result == GameResult.WhiteWins) { wins++; }
                    else if (pgn.Result == GameResult.BlackWins) { losses++; }
                    else if (pgn.Result == GameResult.Draw) { draws++; }
                }
                else if (playerName == pgn.Black)
                {
                    if (pgn.Result == GameResult.WhiteWins) { losses++; }
                    else if (pgn.Result == GameResult.BlackWins) { wins++; }
                    else if (pgn.Result == GameResult.Draw) { draws++; }
                }
            }
        }

    
    }

}
