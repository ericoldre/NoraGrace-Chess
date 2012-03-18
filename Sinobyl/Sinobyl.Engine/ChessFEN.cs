﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sinobyl.Engine
{
	public class ChessFEN
	{
		public readonly ChessPiece[] pieceat = new ChessPiece[64];
		public readonly ChessPlayer whosturn;
		public readonly bool castleWS;
		public readonly bool castleWL;
		public readonly bool castleBS;
		public readonly bool castleBL;
		public readonly ChessPosition enpassant;
		public readonly int fiftymove = 0;
		public readonly int fullmove = 0;

		public ChessFEN(ChessBoard board)
		{
			for (int pos = 0; pos < 64; pos++)
			{
				pieceat[pos] = board.PieceAt((ChessPosition)pos);
			}
			whosturn = board.WhosTurn;
			castleWS = board.CastleAvailWS;
			castleWL = board.CastleAvailWL;
			castleBS = board.CastleAvailBS;
			castleBL = board.CastleAvailBL;
			enpassant = board.EnPassant;
			fiftymove = board.FiftyMovePlyCount;
			fullmove = board.FullMoveCount;
		}
		private ChessFEN(ChessFEN fenOrig, bool reverse)
		{
			if (!reverse) { throw new Exception("this contructor only meant to do FEN reverse positions"); }
			for (int ipos = 0; ipos < 64; ipos++)
			{
                ChessPosition pos = (ChessPosition)ipos;
				if (pos.IsInBounds())
				{
					ChessPosition posRev = Chess.PositionReverse(pos);
                    ChessPiece pieceRev = fenOrig.pieceat[ipos].ToOppositePlayer();
					pieceat[(int)posRev] = pieceRev;
				}
			}
			this.whosturn = Chess.PlayerOther(fenOrig.whosturn);
			this.fullmove = fenOrig.fullmove;
			this.fiftymove = fenOrig.fiftymove;
			this.castleWS = fenOrig.castleBS;
			this.castleWL = fenOrig.castleBL;
			this.castleBS = fenOrig.castleWS;
			this.castleBL = fenOrig.castleWL;
			if (fenOrig.enpassant.IsInBounds())
			{
				this.enpassant = Chess.PositionReverse(fenOrig.enpassant);
			}
			else
			{
				this.enpassant = (ChessPosition.OUTOFBOUNDS);
			}

		}
		public ChessFEN(string sFEN)
		{

			//build the fen string regex
			StringBuilder sbPattern = new StringBuilder();
			sbPattern.Append(@"(?<R8>[\w]{1,8})/");
			sbPattern.Append(@"(?<R7>[\w]{1,8})/");
			sbPattern.Append(@"(?<R6>[\w]{1,8})/");
			sbPattern.Append(@"(?<R5>[\w]{1,8})/");
			sbPattern.Append(@"(?<R4>[\w]{1,8})/");
			sbPattern.Append(@"(?<R3>[\w]{1,8})/");
			sbPattern.Append(@"(?<R2>[\w]{1,8})/");
			sbPattern.Append(@"(?<R1>[\w]{1,8})");
			sbPattern.Append(@"\s+(?<Player>[wbWB]{1})");//player
			sbPattern.Append(@"\s+(?<Castle>[-KQkq]{1,4})");
			sbPattern.Append(@"\s+(?<Enpassant>[-\w]{1,2})");
			sbPattern.Append(@"\s+(?<FiftyMove>[-\d]+)");
			sbPattern.Append(@"\s+(?<FullMove>[-\d]+)");
			string pattern = sbPattern.ToString();

			//do the pattern match
			Regex regex = new Regex(pattern);
			MatchCollection matches = regex.Matches(sFEN);

			//make sure we found exactly 1 match
			if (matches.Count == 0) { throw new ChessException("No valid fen found"); }
			if (matches.Count > 1) { throw new ChessException("Multiple FENs in string"); }

			//grab the string data from the regular expression
			System.Text.RegularExpressions.Match match = matches[0];
			string[] sRanks = new string[8];
			foreach (ChessRank rank in Chess.AllRanks)
			{
				sRanks[int.Parse(Chess.RankToString(rank)) - 1] = match.Groups["R" + Chess.RankToString(rank)].Value;
			}

			string sPlayer = match.Groups["Player"].Value;
			string sCastle = match.Groups["Castle"].Value;
			string sEnpassant = match.Groups["Enpassant"].Value;
			string sFiftyMove = match.Groups["FiftyMove"].Value;
			string sFullMove = match.Groups["FullMove"].Value;

			//parse the string values into usable formats

			foreach (ChessPosition pos in Chess.AllPositions)
			{
				pieceat[(int)pos] = ChessPiece.EMPTY;
			}

			//set board piece positions
			foreach (ChessRank rank in Chess.AllRanks)
			{
				string srank = sRanks[int.Parse(Chess.RankToString(rank)) - 1];
				int ifile = 0;
				foreach (char c in srank.ToCharArray())
				{
					if (ifile > 7) { throw new ChessException("too many pieces in rank " + Chess.RankToString(rank)); }

					if ("1234567890".IndexOf(c) >= 0)
					{
						ifile += int.Parse(c.ToString());
					}
					else
					{
						pieceat[(int)Chess.AllFiles[ifile].ToPosition(rank)] = c.ParseAsPiece();
						ifile++;
					}
				}
			}

			//find the player
			if (sPlayer == "w")
			{
				whosturn = ChessPlayer.White;
			}
			else if (sPlayer == "b")
			{
				whosturn = ChessPlayer.Black;
			}
			else
			{
				throw new Exception(sPlayer + " is not a valid player");
			}
			
			//set available castling positions
			castleWS = false;
			castleWL = false;
			castleBS = false;
			castleBL = false;
			if (sCastle.IndexOf("K") >= 0)
			{
				castleWS = true;
			}
			if (sCastle.IndexOf("Q") >= 0)
			{
				castleWL = true;
			}
			if (sCastle.IndexOf("k") >= 0)
			{
				castleBS = true;
			}
			if (sCastle.IndexOf("q") >= 0)
			{
				castleBL = true;
			}

			//set enpassent location
			this.enpassant = (ChessPosition.OUTOFBOUNDS);
			if (sEnpassant != "-")
			{
				enpassant = sEnpassant.ParseAsPosition();
			}

			//set fifty move count
			fiftymove = 0;
			if (sFiftyMove != "-")
			{
				fiftymove = int.Parse(sFiftyMove);
			}

			//set fullmove count
			fullmove = 1;
			if (sFullMove != "-")
			{
				fullmove = int.Parse(sFullMove);
			}


		}
		public ChessFEN Reverse()
		{
			return new ChessFEN(this, true);
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new StringBuilder(50);
			for (int irank = 7; irank >= 0; irank--)
			{
				int emptycount = 0;
				for (int ifile = 0; ifile <= 7; ifile++)
				{
					ChessRank rank = Chess.AllRanks[irank];
					ChessFile file = Chess.AllFiles[ifile];
					ChessPiece piece = pieceat[(int)file.ToPosition(rank)];

					if (piece == ChessPiece.EMPTY)
					{
						emptycount++;
					}
					else
					{
						if (emptycount > 0)
						{
							sb.Append(emptycount.ToString());
							emptycount = 0;
						}
						sb.Append(Chess.PieceToString(piece));
					}
				}
				if (emptycount > 0)
				{
					sb.Append(emptycount.ToString());
					emptycount = 0;
				}
				if (irank > 0) { sb.Append("/"); }
			}

			//player
			if (whosturn == ChessPlayer.White)
			{
				sb.Append(" w ");
			}
			else
			{
				sb.Append(" b ");
			}

			//castle rights
			if (castleWS || castleWL || castleBS || castleBL)
			{
				if (castleWS) { sb.Append("K"); }
				if (castleWL) { sb.Append("Q"); }
				if (castleBS) { sb.Append("k"); }
				if (castleBL) { sb.Append("q"); }
			}
			else
			{
				sb.Append("-");
			}

			//enpassant
			if (enpassant.IsInBounds())
			{
				sb.Append(" " + Chess.PositionToString(enpassant));
			}
			else
			{
				sb.Append(" -");
			}

			//50 move
			sb.Append(" " + fiftymove.ToString());

			//fullmove
			sb.Append(" " + fullmove.ToString());

			return sb.ToString();

		}

		
		public override bool Equals(object obj)
		{
			ChessFEN o = obj as ChessFEN;
			if (o == null) { return false; }
			if (o.castleWS != this.castleWS) { return false; }
			if (o.castleWL != this.castleWL) { return false; }
			if (o.castleBS != this.castleBS) { return false; }
			if (o.castleBL != this.castleBL) { return false; }

			if (o.enpassant != this.enpassant) { return false; }
			if (o.fiftymove != this.fiftymove) { return false; }
			if (o.fullmove != this.fullmove) { return false; }
			if (o.whosturn != this.whosturn) { return false; }

			for (int i = 0; i < 120; i++)
			{
				if (o.pieceat[i] != this.pieceat[i]) { return false; }
			}

			return true;
		}
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}
	}
}