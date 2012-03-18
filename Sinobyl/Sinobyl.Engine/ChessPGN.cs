using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sinobyl.Engine
{
	/// <summary>
	/// Summary description for ChessPGN.
	/// </summary>
	/// 





	public class ChessPGNHeaders : List<ChessPGNHeader>
	{

		public ChessPGNHeader this[string key]
		{
			get
			{
				
				foreach (ChessPGNHeader header in this)
				{
					if (header.Key.ToUpper() == key.ToUpper())
					{
						return header;
					}
				}
				return null;
			}
		}


		public void Add(string key, string value)
		{
			Add(new ChessPGNHeader(key, value));
		}
	}

	public class ChessPGNComment
	{
		private int _movenum;
		private string _text;
		public ChessPGNComment(int movenum, string text)
		{
			_movenum = movenum;
			_text = text;
		}
		public int MoveNum
		{
			get
			{
				return _movenum;
			}
		}
		public string Text
		{
			get
			{
				return _text;
			}
		}
	}
	public class ChessPGNHeader
	{
		private readonly string _key;
		private readonly string _value;
		public ChessPGNHeader(string key, string val)
		{
			_key = key;
			_value = val;
		}
		public ChessPGNHeader(string headerline)
		{
			string pattern = @"\[(?<key>[\w]+)\s+\""(?<value>[\S\s]+)\""\]";
			Regex regex = new Regex(pattern);
			MatchCollection matches = regex.Matches(headerline);
			if (matches.Count != 1)
			{
				throw new ChessException("not a valid pgn header: " + headerline);
			}
			System.Text.RegularExpressions.Match match = matches[0];
			_key = match.Groups["key"].Value;
			_value = match.Groups["value"].Value;
		}
		public string Key
		{
			get
			{
				return _key;
			}
		}
		public string Value
		{
			get
			{
				return _value;
			}
		}
		public override string ToString()
		{
			return "[" + this.Key + " \"" + this.Value + "\"]";
		}


	}
	public class ChessPGN
	{

		
		private readonly ReadOnlyCollection<ChessMove> _moves;
		private ChessResult? _result;
		private ChessResultReason _resultReason;
		private ChessPGNHeaders _headers;
		private List<ChessPGNComment> _comments;

		public ChessPGN(ChessPGNHeaders headers, ChessMoves moves, ChessResult? result, List<ChessPGNComment> comments, ChessResultReason reason)
		{
			_headers = headers;
			_moves = new ReadOnlyCollection<ChessMove>(moves);
			_result = result;
			_comments = comments;
			_resultReason = reason;
		}
		public ChessPGN(ChessPGNHeaders headers, ChessBoard board)
		{
			_headers = headers;
			_moves = new ReadOnlyCollection<ChessMove>(board.HistoryMoves);
			_result = null;
			_resultReason = ChessResultReason.NotDecided;
			_comments = new List<ChessPGNComment>();
		}

		public ChessPGN(ChessGame game)
		{
			_headers = new ChessPGNHeaders();
			_headers.Add(new ChessPGNHeader("White", game.PlayerWhite.Name));
			_headers.Add(new ChessPGNHeader("Black", game.PlayerBlack.Name));
			_headers.Add(new ChessPGNHeader("Date", DateTime.Now.ToShortDateString()));

			//_headers.Add(new ChessPGNHeader("TotalNodes", ChessSearch.CountTotalAINodes.ToString()));
			//_headers.Add(new ChessPGNHeader("TotalEvals", ChessEval.TotalAIEvals.ToString()));
			//_headers.Add(new ChessPGNHeader("TotalAITime", ChessSearch.CountTotalAITime.ToString()));
			//double nodesPerSec = ChessSearch.CountTotalAINodes / ChessSearch.CountTotalAITime.TotalSeconds;

			//_headers.Add(new ChessPGNHeader("Nodes/Sec", nodesPerSec.ToString()));


			_moves = new ReadOnlyCollection<ChessMove>(game.MoveHistory());
			_result = game.Result;
			_resultReason = game.ResultReason;
			_comments = new List<ChessPGNComment>();


		}
		public ReadOnlyCollection<ChessMove> Moves
		{
			get
			{
				return _moves;
			}
		}
		public List<string> HeaderKeys
		{
			get
			{
				List<string> retval = new List<string>();
				foreach (ChessPGNHeader header in _headers)
				{
					retval.Add(header.Key);
				}
				return retval;
			}
		}
		public string HeaderValue(string key)
		{
			ChessPGNHeader h = null;
			foreach(ChessPGNHeader h1 in _headers)
			{
				if (h1.Key.ToLower() == key.ToLower()) { h = h1; }
			}
			
			if (h == null)
			{
				return "";
			}
			else
			{
				return h.Value;
			}
		}
		public string StartingPosition
		{
			get
			{
				string retval = this.HeaderValue("FEN");
                if (retval == "") { retval = ChessFEN.FENStart; }
				return retval;
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
				return this.HeaderValue("White");
			}
		}
		public string Black
		{
			get
			{
				return this.HeaderValue("Black");
			}
		}
		public int? WhiteElo
		{
			get
			{
				int retval;
				if (int.TryParse(this.HeaderValue("WhiteElo"), out retval))
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
				if (int.TryParse(this.HeaderValue("BlackElo"), out retval))
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
				if (DateTime.TryParse(this.HeaderValue("Date"), out ret))
				{
					return ret;
				}
				else if (DateTime.TryParse(this.HeaderValue("EventDate"), out ret))
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
				return this.HeaderValue("Site");
			}
		}
		public string EventName
		{
			get
			{
				return this.HeaderValue("Event");
			}
		}
		public ChessPGNComment CommentForPly(int iPly)
		{
			foreach (ChessPGNComment comment in this._comments)
			{
				if (comment.MoveNum == iPly)
				{
					return comment;
				}
			}
			return null;
		}


		public override string ToString()
		{
			return this.ToString(45);
		}

		public string ToString(int MaxLineLen)
		{
			ChessBoard board = new ChessBoard(this.StartingPosition);

			StringBuilder sbMoves = new StringBuilder();
			StringBuilder sbHeaders = new StringBuilder();

			//result
			string sResult = "*";
			switch (_result)
			{
				case ChessResult.WhiteWins: sResult = "1-0"; break;
				case ChessResult.BlackWins: sResult = "0-1"; break;
				case ChessResult.Draw: sResult = "1/2-1/2"; break;
				case null: sResult = "*"; break;
			}

			//headers
			foreach (ChessPGNHeader header in _headers)
			{
				sbHeaders.Append(header.ToString() + Environment.NewLine);
			}
			if (_headers["Result"] == null)
			{
				sbHeaders.Append(new ChessPGNHeader("Result", sResult).ToString() + Environment.NewLine);
			}
			sbHeaders.Append(Environment.NewLine);

			//write any comments before moves
			ChessPGNComment comment = this.CommentForPly(0);
			if (comment != null)
			{
				sbMoves.Append("{");
				sbMoves.Append(comment.Text);
				sbMoves.Append("} ");
			}

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
				sbMoves.Append(move.ToString(board) + " ");
				board.MoveApply(move);
				comment = this.CommentForPly(imove);
				if (comment != null)
				{
					sbMoves.Append("{");
					sbMoves.Append(comment.Text);
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
			return sbHeaders.ToString() + sbMoves.ToString();
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
			List<ChessPGNComment> comments = new List<ChessPGNComment>();
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
			while (!gamedone && (line = reader.ReadLine()) != null)
			{
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
							ChessPGNHeader header = new ChessPGNHeader("[" + token + "]");
							headers.Add(header);
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
							comments.Add(new ChessPGNComment(moves.Count, token));
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
							ChessMove move = new ChessMove(board, token);
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


}
