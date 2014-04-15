using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public class ChessEvalMaterial
    {
        private readonly ChessEvalSettings _settings;
        private readonly Results[] _hash = new Results[500];
        public static int TotalEvalMaterialCount = 0;

        public ChessEvalMaterial(ChessEvalSettings settings)
        {
            _settings = settings;
        }

        public Results EvalMaterialHash(ChessBoard board)
        {
            long idx = board.ZobristMaterial % _hash.GetUpperBound(0);

            if (idx < 0) { idx = -idx; }

            Results retval = _hash[idx];
            if (retval != null && retval.ZobristMaterial == board.ZobristMaterial)
            {
                return retval;
            }

            retval = EvalMaterial(
                zob: board.ZobristMaterial,
                wp: board.PieceCount(ChessPiece.WPawn),
                wn: board.PieceCount(ChessPiece.WKnight),
                wb: board.PieceCount(ChessPiece.WBishop),
                wr: board.PieceCount(ChessPiece.WRook),
                wq: board.PieceCount(ChessPiece.WQueen),
                bp: board.PieceCount(ChessPiece.BPawn),
                bn: board.PieceCount(ChessPiece.BKnight),
                bb: board.PieceCount(ChessPiece.BBishop),
                br: board.PieceCount(ChessPiece.BRook),
                bq: board.PieceCount(ChessPiece.BQueen));

            _hash[idx] = retval;
            return retval;
        }

        public Results EvalMaterial(Int64 zob, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq)
        {
            TotalEvalMaterialCount++;

            int basicCount = 
                (wn * 3) + (bn * 3)
                + (wb * 3) + (bb * 3)
                + (wr * 5) + (br * 5)
                + (wq * 9) + (bq * 9);

            

            int startScore = (wp * _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Opening])
                + (wn * _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Opening])
                + (wb * _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Opening])
                + (wr * _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Opening])
                + (wq * _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Opening])
                - (bp * _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Opening])
                - (bn * _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Opening])
                - (bb * _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Opening])
                - (br * _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Opening])
                - (bq * _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Opening]);

            int endScore = (wp * _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Endgame])
                + (wn * _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Endgame])
                + (wb * _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Endgame])
                + (wr * _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Endgame])
                + (wq * _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Endgame])
                - (bp * _settings.MaterialValues[ChessPieceType.Pawn][ChessGameStage.Endgame])
                - (bn * _settings.MaterialValues[ChessPieceType.Knight][ChessGameStage.Endgame])
                - (bb * _settings.MaterialValues[ChessPieceType.Bishop][ChessGameStage.Endgame])
                - (br * _settings.MaterialValues[ChessPieceType.Rook][ChessGameStage.Endgame])
                - (bq * _settings.MaterialValues[ChessPieceType.Queen][ChessGameStage.Endgame]);

            if (wb > 1)
            {
                startScore += _settings.MaterialBishopPair[ChessGameStage.Opening];
                endScore += _settings.MaterialBishopPair[ChessGameStage.Endgame];
            }
            if (bb > 1)
            {
                startScore -= _settings.MaterialBishopPair[ChessGameStage.Opening];
                endScore -= _settings.MaterialBishopPair[ChessGameStage.Endgame];
            }

            int startWeight = CalcStartWeight(basicCount);

            //int score = (int)(((float)startScore * startWeight) + ((float)endScore * (1 - startWeight)));

            return new Results(zob, startWeight, startScore, endScore, basicCount);
            //return new Results(zob, startWeight, startScore, endScore, basicCount, wp,  wn,  wb,  wr,  wq,  bp,  bn,  bb,  br,  bq);
        }

        protected virtual int CalcStartWeight(int basicMaterialCount)
        {
            //full material would be 62
            if (basicMaterialCount >= 56)
            {
                return 100;
            }
            else if (basicMaterialCount <= 10)
            {
                return 0;
            }
            else
            {
                int rem = basicMaterialCount - 10;
                float retval = (float)rem / 46f;
                int retval2 = (int)Math.Round((retval * 100));
                return retval2;
            }
        }
        

        public class Results
        {
            public readonly Int64 ZobristMaterial;
            public readonly int StartWeight;
            public readonly int ScoreStart;
            public readonly int ScoreEnd;
            public readonly int BasicMaterialCount;
            //public readonly int Wp;
            //public readonly int Wn;
            //public readonly int Wb;
            //public readonly int Wr;
            //public readonly int Wq;
            //public readonly int Bp;
            //public readonly int Bn;
            //public readonly int Bb;
            //public readonly int Br;
            //public readonly int Bq;
            
            public Results(Int64 zobristMaterial, int startWeight, int scoreStart, int scoreEnd, int basicMateralCount /*, int wp, int wn, int wb, int wr, int wq, int bp, int bn, int bb, int br, int bq*/)
            {
                ZobristMaterial = zobristMaterial;
                StartWeight = startWeight;
                ScoreStart = scoreStart;
                ScoreEnd = scoreEnd;
                BasicMaterialCount = basicMateralCount;

                //Wp = wp;
                //Wn = wn;
                //Wb = wb;
                //Wr = wr;
                //Wq = wq;
                //Bp = bp;
                //Bn = bn;
                //Bb = bb;
                //Br = br;
                //Bq = bq;

            }

            public bool DoShelter
            {
                get { return this.StartWeight > 40; }
            }

        }
    }
}
