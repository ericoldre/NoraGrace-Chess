using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sinobyl.Engine
{

    public enum ChessResult
    {
        Draw = 0, WhiteWins = 1, BlackWins = -1
    }

    public enum ChessResultReason
    {
        NotDecided = 0,
        Checkmate = 1, Resign = 2, OutOfTime = 3, Adjudication = 4, //win reasons
        Stalemate = 5, FiftyMoveRule = 6, InsufficientMaterial = 7, MutualAgreement = 8, Repetition = 9, //draw reasons
        Unknown = 10, IllegalMove = 11
    }


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

    //public class ChessPGNComment
    //{
    //    private int _movenum;
    //    private string _text;
    //    public ChessPGNComment(int movenum, string text)
    //    {
    //        _movenum = movenum;
    //        _text = text;
    //    }
    //    public int MoveNum
    //    {
    //        get
    //        {
    //            return _movenum;
    //        }
    //    }
    //    public string Text
    //    {
    //        get
    //        {
    //            return _text;
    //        }
    //    }
    //}


    //public class ChessPGNHeader
    //{
        
    //    private readonly string _key;
    //    private readonly string _value;
    //    public ChessPGNHeader(string key, string val)
    //    {
    //        _key = key;
    //        _value = val;
    //    }

        
    //    public string Key
    //    {
    //        get
    //        {
    //            return _key;
    //        }
    //    }
    //    public string Value
    //    {
    //        get
    //        {
    //            return _value;
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        return "[" + this.Key + " \"" + this.Value + "\"]";
    //    }


    //}
	public class ChessPGN
	{

		
		private readonly List<ChessMove> _moves = new List<ChessMove>();
		private ChessResult? _result;
		private ChessResultReason _resultReason;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<int, string> _comments = new Dictionary<int,string>();



        public ChessPGN(IEnumerable<KeyValuePair<string, string>> headers, IEnumerable<ChessMove> moves, ChessResult? result, IEnumerable<KeyValuePair<int, string>> comments, ChessResultReason reason)
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
        public ChessPGN(IEnumerable<KeyValuePair<string, string>> headers, ChessBoard board)
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
			_resultReason = ChessResultReason.NotDecided;
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

		public List<ChessMove> Moves
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
                    return ChessFEN.FENStart;
                }
			}
		}

		public ChessResult? Result
		{
			get
			{
				return _result;
			}
		}
		public ChessResultReason ResultReason
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
            StringWriter writer = new StringWriter(sb);
            Write(writer);
            return sb.ToString();
		}

		public void Write(TextWriter writer, int MaxLineLen = 90)
		{
			ChessBoard board = new ChessBoard(this.StartingPosition);

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
			if (_headers["Result"] == null)
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
			foreach (ChessMove move in _moves)
			{
				if (iswhite)
				{
					int ifullmove = (imove / 2) + 1;
					sbMoves.Append(ifullmove.ToString() + ". ");
				}
				sbMoves.Append(move.Write(board) + " ");
				board.MoveApply(move);
                string comment = this.Comments[imove - 1];
				if (comment != null)
				{
					sbMoves.Append("{");
					sbMoves.Append(comment);
					sbMoves.Append("} ");
				}
				iswhite = !iswhite;
				imove++;
			}
			//result
			if (_result == ChessResult.WhiteWins)
			{
				sbMoves.Append("1-0 ");
			}
			else if (_result == ChessResult.BlackWins)
			{
				sbMoves.Append("0-1 ");
			}
			else if (_result == ChessResult.Draw)
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

        
        public static IEnumerable<ChessPGN> AllGames(StreamReader reader)
        {
            while (true)
            {
                ChessPGN pgn = NextGame(reader);
                if (pgn == null) { break; }
                yield return pgn;
            }
        }

		public static ChessPGN NextGame(string PGNString)
		{
			MemoryStream memory = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(PGNString));
			StreamReader reader = new StreamReader(memory);
			return NextGame(reader);
		}
		public static ChessPGN NextGame(StreamReader reader)
		{
			ChessPGNHeaders headers = new ChessPGNHeaders();
            Dictionary<int, string> comments = new Dictionary<int, string>();
			ChessMoves moves = new ChessMoves();
			ChessResult? result = null;
			ChessResultReason reason = ChessResultReason.NotDecided;
			ChessBoard board = new ChessBoard();


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
								board = new ChessBoard(header.Value);
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
								result = ChessResult.Draw;
								if (board.IsDrawByStalemate()) { reason = ChessResultReason.Stalemate; }
								else if (board.IsDrawByRepetition()) { reason = ChessResultReason.Repetition; }
								else if (board.IsDrawBy50MoveRule()) { reason = ChessResultReason.FiftyMoveRule; }
								else { reason = ChessResultReason.MutualAgreement; }

								gamedone = true;
							}
						}
						else if (token.Trim() == "1-0")
						{
							if (OkToProcessResults)
							{
								result = ChessResult.WhiteWins;
								if (board.IsMate()) { reason = ChessResultReason.Checkmate; }
								else { reason = ChessResultReason.Resign; }
								gamedone = true;
							}
						}
						else if (token.Trim() == "0-1")
						{
							if (OkToProcessResults)
							{
								result = ChessResult.BlackWins;
								if (board.IsMate()) { reason = ChessResultReason.Checkmate; }
								else { reason = ChessResultReason.Resign; }
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
							ChessMove move = ChessMoveInfo.Parse(board, token);
							board.MoveApply(move);
							moves.Add(move);

							//check for mate.
							if (board.IsMate())
							{
								if (board.WhosTurn == ChessPlayer.White)
								{
									result = ChessResult.BlackWins;
									reason = ChessResultReason.Checkmate;
								}
								else
								{
									result = ChessResult.WhiteWins;
									reason = ChessResultReason.Checkmate;
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
				return new ChessPGN(headers, moves, result, comments, reason);
			}
			else
			{
				return null;
			}

		}
	}

    public static class ExtensionsChessPGN
    {
        public static string Summary(this IEnumerable<ChessPGN> PGNs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var winnerGroup in PGNs.GroupBy(pgn => new { Winner = pgn.Result == ChessResult.WhiteWins ? pgn.White : pgn.Result == ChessResult.BlackWins ? pgn.Black : "Draw" }))
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

        public static void ResultsForPlayer(this IEnumerable<ChessPGN> PGNs, string playerName, out int wins, out int losses, out int draws)
        {
            wins=0;
            losses=0;
            draws=0;
            foreach (ChessPGN pgn in PGNs)
            {
                if (playerName == pgn.White)
                {
                    if (pgn.Result == ChessResult.WhiteWins) { wins++; }
                    else if (pgn.Result == ChessResult.BlackWins) { losses++; }
                    else if (pgn.Result == ChessResult.Draw) { draws++; }
                }
                else if (playerName == pgn.Black)
                {
                    if (pgn.Result == ChessResult.WhiteWins) { losses++; }
                    else if (pgn.Result == ChessResult.BlackWins) { wins++; }
                    else if (pgn.Result == ChessResult.Draw) { draws++; }
                }
            }
        }

    
    }

}
