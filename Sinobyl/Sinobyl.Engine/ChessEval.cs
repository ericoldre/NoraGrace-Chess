using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinobyl.Engine
{
    public interface IChessEval
    {
        int EvalFor(ChessBoard board, ChessPlayer who);
        int DrawScore { get; set; }
    }

    public class ChessEval: IChessEval
    {

        public const int MaxValue = int.MaxValue - 100;
        public const int MinValue = -MaxValue;

        protected readonly ChessEvalPawns _evalPawns;
        public readonly IChessEvalMaterial _evalMaterial;

        public readonly PhasedScore[][] _pcsqPiecePos = new PhasedScore[ChessPieceInfo.LookupArrayLength][];
        public readonly PhasedScore[][] _mobilityPieceTypeCount = new PhasedScore[ChessPieceTypeInfo.LookupArrayLength][];
        public readonly PhasedScore _matBishopPair;

        public readonly int[] _endgameMateKingPcSq;


        protected readonly ChessEvalSettings _settings;

        protected readonly PhasedScore RookFileOpen;
        protected readonly PhasedScore RookFileHalfOpen;

        public readonly int KingAttackCountValue = 0;
        public readonly int KingAttackWeightValue = 0;
        public readonly int KingAttackWeightCutoff = 0;
        public readonly int KingRingAttack = 0;
        public readonly int KingRingAttackControlBonus = 0;
        public readonly int[] KingQueenTropismFactor;

        public static readonly ChessEval Default = new ChessEval();

        public static int TotalEvalCount = 0;

        public int DrawScore { get; set; }
        
        private static Bitboard[] _kingSafetyRegion;
        private static int[] _kingAttackerWeight;

        static ChessEval()
        {
            _kingSafetyRegion = new Bitboard[64];
            foreach (var kingPos in ChessPositionInfo.AllPositions)
            {
                var rank = kingPos.ToRank();
                if (rank == Rank.Rank1) { rank = Rank.Rank2; }
                if (rank == Rank.Rank8) { rank = Rank.Rank7; }
                var file = kingPos.ToFile();
                if (file == File.FileA) { file = File.FileB; }
                if (file == File.FileH) { file = File.FileG; }
                var adjusted = rank.ToPosition(file);
                _kingSafetyRegion[(int)kingPos] = Attacks.KingAttacks(adjusted) | adjusted.ToBitboard();

                System.Diagnostics.Debug.Assert(_kingSafetyRegion[(int)kingPos].BitCount() == 9);

            }

            _kingAttackerWeight = new int[7];
            _kingAttackerWeight[(int)ChessPieceType.Pawn] = 1;
            _kingAttackerWeight[(int)ChessPieceType.Knight] = 2;
            _kingAttackerWeight[(int)ChessPieceType.Bishop] = 2;
            _kingAttackerWeight[(int)ChessPieceType.Rook] = 3;
            _kingAttackerWeight[(int)ChessPieceType.Queen] = 4;
            _kingAttackerWeight[(int)ChessPieceType.King] = 1;
            

        }

        public ChessEval()
            : this(ChessEvalSettings.Default())
        {

        }

        public ChessEval(ChessEvalSettings settings, IChessEvalMaterial evalMaterial = null)
        {
            _settings = settings.CloneDeep();

            //setup pawn evaluation
            _evalPawns = new ChessEvalPawns(_settings, 10000);
            _evalMaterial = evalMaterial ?? new ChessEvalMaterial2(_settings);
            
            //bishop pairs
            _matBishopPair = PhasedScoreInfo.Create(settings.MaterialBishopPair.Opening,  settings.MaterialBishopPair.Endgame);


            //setup piecesq tables
            foreach (ChessPiece piece in ChessPieceInfo.AllPieces)
            {
                _pcsqPiecePos[(int)piece] = new PhasedScore[64];
                foreach (ChessPosition pos in ChessPositionInfo.AllPositions)
                {
                
                    if (piece.PieceToPlayer() == ChessPlayer.White)
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreInfo.Create(
                            settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos],
                            settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Endgame][pos]);
                    }
                    else
                    {
                        _pcsqPiecePos[(int)piece][(int)pos] = PhasedScoreInfo.Create(
                            -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Opening][pos.Reverse()],
                            -settings.PcSqTables[piece.ToPieceType()][ChessGameStage.Endgame][pos.Reverse()]);
                    }
                    
                
                }
            }



            //setup mobility arrays

            foreach (ChessPieceType pieceType in ChessPieceTypeInfo.AllPieceTypes)
            {
                _mobilityPieceTypeCount[(int)pieceType] = new PhasedScore[28];
                for (int attacksCount = 0; attacksCount < 28; attacksCount++)
                {
                    var mob = settings.Mobility;
                    var opiece = mob[pieceType];

                    int startVal = (attacksCount - opiece[ChessGameStage.Opening].ExpectedAttacksAvailable) * opiece[ChessGameStage.Opening].AmountPerAttackDefault;
                    int endVal = (attacksCount - opiece[ChessGameStage.Endgame].ExpectedAttacksAvailable) * opiece[ChessGameStage.Endgame].AmountPerAttackDefault;

                    _mobilityPieceTypeCount[(int)pieceType][attacksCount] = PhasedScoreInfo.Create(startVal, endVal);
                }
            }

            //initialize pcsq for trying to mate king in endgame, try to push it to edge of board.
            _endgameMateKingPcSq = new int[64];
            foreach (var pos in ChessPositionInfo.AllPositions)
            {
                List<int> distToMid = new List<int>();
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.D4));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.D5));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.E4));
                distToMid.Add(pos.DistanceToNoDiag(ChessPosition.E5));
                var minDist = distToMid.Min();
                _endgameMateKingPcSq[(int)pos] = minDist * 50;
            }

            RookFileOpen = PhasedScoreInfo.Create(settings.RookFileOpen, settings.RookFileOpen / 2);
            RookFileHalfOpen = PhasedScoreInfo.Create(settings.RookFileOpen / 2, settings.RookFileOpen / 4);

            KingAttackCountValue = settings.KingAttackCountValue;
            KingAttackWeightValue = settings.KingAttackWeightValue;
            KingAttackWeightCutoff = settings.KingAttackWeightCutoff;
            KingRingAttack = settings.KingRingAttack;
            KingRingAttackControlBonus = settings.KingRingAttackControlBonus;

            KingQueenTropismFactor = new int[25];
            for (int d = 0; d <= 24; d++)
            {
                int min = 4;
                int max = 12;
                double minFactor = settings.KingAttackFactor;
                double maxFactor = minFactor + settings.KingAttackFactorQueenTropismBonus;
                
                if (d >= max) { KingQueenTropismFactor[d] = (int)Math.Round(100 * minFactor); continue; }
                if (d <= min) { KingQueenTropismFactor[d] = (int)Math.Round(100 * maxFactor); continue; }

                var each = (1f / (max - min)) * (maxFactor - minFactor);
                double thisFactor = minFactor + (each * (max - d));

                KingQueenTropismFactor[d] = KingQueenTropismFactor[d] = (int)Math.Round(100 * thisFactor);


                //double pct = minFactor + (maxFactor * ((d - min) / (max - min)));

            }

        }

        public void PcSqValuesAdd(ChessPiece piece, ChessPosition pos, ref PhasedScore value)
        {
            value = value.Add(_pcsqPiecePos[(int)piece][(int)pos]);
        }
        public void PcSqValuesRemove(ChessPiece piece, ChessPosition pos, ref PhasedScore value)
        {
            value = value.Subtract(_pcsqPiecePos[(int)piece][(int)pos]);
        }

        public int EvalFor(ChessBoard board, ChessPlayer who)
        {
            int retval = Eval(board);
            if (who == ChessPlayer.Black) { retval = -retval; }
            return retval;

        }

        public ChessEvalInfo _evalInfo = new ChessEvalInfo();
        public virtual int Eval(ChessBoard board)
        {
            return EvalLazy(board, _evalInfo, null, ChessEval.MinValue, ChessEval.MaxValue);
        }

        public int EvalLazy(ChessBoard board, ChessEvalInfo evalInfo, ChessEvalInfo prevEvalInfo, int alpha, int beta)
        {
            System.Diagnostics.Debug.Assert(alpha >= MinValue);
            System.Diagnostics.Debug.Assert(beta <= MaxValue);

            evalInfo.Reset();


            //material
            EvalMaterialResults material = _evalMaterial.EvalMaterialHash(board);

            //pawns
            PawnInfo pawns = this._evalPawns.PawnEval(board);
            System.Diagnostics.Debug.Assert(pawns.WhitePawns == board[ChessPiece.WPawn]);
            System.Diagnostics.Debug.Assert(pawns.BlackPawns == board[ChessPiece.BPawn]);

            evalInfo.MaterialPawnsApply(board, material, pawns, this.DrawScore);

            if (prevEvalInfo != null)
            {
                evalInfo.ApplyPreviousEval(board, prevEvalInfo);
                System.Diagnostics.Debug.Assert(evalInfo.LazyAge > 0);

                int fuzzyLazyScore = evalInfo.Score;
                int margin = evalInfo.LazyAge * 50;
                if (fuzzyLazyScore + margin < alpha)
                {
                    return alpha;
                }
                if (fuzzyLazyScore - margin > beta)
                {
                    return beta;
                }
            }

            EvalAdvanced(board, evalInfo, material, pawns);

            return evalInfo.Score;
        }

        public void EvalAdvanced(ChessBoard board, ChessEvalInfo evalInfo, EvalMaterialResults material, PawnInfo pawns)
        {
            
            TotalEvalCount++;

            //mark that advanced eval terms are from this ply
            evalInfo.LazyAge = 0;

            //set up 
            var attacksWhite = evalInfo.Attacks[(int)ChessPlayer.White];
            var attacksBlack = evalInfo.Attacks[(int)ChessPlayer.Black];

            attacksWhite.PawnEast = board[ChessPiece.WPawn].ShiftDirNE();
            attacksWhite.PawnWest = board[ChessPiece.WPawn].ShiftDirNW();
            attacksBlack.PawnEast = board[ChessPiece.BPawn].ShiftDirSE();
            attacksBlack.PawnWest = board[ChessPiece.BPawn].ShiftDirSW();

            attacksWhite.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.White));
            attacksBlack.King = Attacks.KingAttacks(board.KingPosition(ChessPlayer.Black));

            EvaluateMyPieces(board, ChessPlayer.White, evalInfo);
            EvaluateMyPieces(board, ChessPlayer.Black, evalInfo);

            EvaluateMyKingAttack(board, ChessPlayer.White, evalInfo);
            EvaluateMyKingAttack(board, ChessPlayer.Black, evalInfo);


            evalInfo.PawnsPassed = this._evalPawns.EvalPassedPawns(board, evalInfo.Attacks, pawns.PassedPawns, pawns.Candidates, evalInfo.Workspace); ;


            //shelter storm;
            if (material.DoShelter)
            {
                evalInfo.ShelterStorm = PhasedScoreInfo.Create(_settings.PawnShelterFactor * pawns.EvalShelter(
                    whiteKingFile: board.KingPosition(ChessPlayer.White).ToFile(),
                    blackKingFile: board.KingPosition(ChessPlayer.Black).ToFile(),
                    castleFlags: board.CastleRights), 0);
            }
            else
            {
                evalInfo.ShelterStorm = 0;
            }
            
            //test to see if we are just trying to force the king to the corner for mate.
            PhasedScore endGamePcSq = 0;
            if (UseEndGamePcSq(board, ChessPlayer.White, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq;
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
            }
            else if (UseEndGamePcSq(board, ChessPlayer.Black, out endGamePcSq))
            {
                evalInfo.PcSq = endGamePcSq.Negate();
                evalInfo.Attacks[0].Mobility = 0;
                evalInfo.Attacks[1].Mobility = 0;
            }


        }

        protected int EvaluateMyKingAttack(ChessBoard board, ChessPlayer me, ChessEvalInfo info)
        {
            //king attack info should have counts and weight of everything but pawns and kings.

            var him = me.PlayerOther();
            var myAttacks = info.Attacks[(int)me];
            var hisAttacks = info.Attacks[(int)him];

            var hisKingZone = _kingSafetyRegion[(int)board.KingPosition(him)];

            int retval = 0;
            int c;
            if (myAttacks.KingAttackerCount >= 2 && myAttacks.KingAttackerWeight >= KingAttackWeightCutoff)
            {
                //if we don't at least have a decent attack, just give credit for building count of attackers and move on.

                //add in pawns to king attack.
                Bitboard myInvolvedPawns = Bitboard.Empty;
                Bitboard myPawns = board[me] & board[ChessPieceType.Pawn];

                if (me == ChessPlayer.White)
                {
                    myInvolvedPawns |= hisKingZone.ShiftDirSE() & myPawns;
                    myInvolvedPawns |= hisKingZone.ShiftDirSW() & myPawns;
                }
                else
                {
                    myInvolvedPawns |= hisKingZone.ShiftDirNE() & myPawns;
                    myInvolvedPawns |= hisKingZone.ShiftDirNW() & myPawns;
                }

                if (myInvolvedPawns != Bitboard.Empty)
                {
                    c = myInvolvedPawns.BitCount();
                    myAttacks.KingAttackerCount += c;
                    myAttacks.KingAttackerWeight += c * _kingAttackerWeight[(int)ChessPieceType.Pawn];
                }

                //add in my king to the attack.
                if ((Attacks.KingAttacks(board.KingPosition(me)) & hisKingZone) != Bitboard.Empty)
                {
                    myAttacks.KingAttackerCount++;
                    myAttacks.KingAttackerWeight += _kingAttackerWeight[(int)ChessPieceType.King];
                }

                //add bonus for piece involvement over threshold;
                retval += (myAttacks.KingAttackerWeight - KingAttackWeightCutoff) * KingAttackWeightValue;

                //now calculate bonus for attacking squares directly surrounding king;
                Bitboard kingAdjecent = Attacks.KingAttacks(board.KingPosition(him));
                while (kingAdjecent != Bitboard.Empty)
                {
                    ChessPosition pos = BitboardInfo.PopFirst(ref kingAdjecent);
                    Bitboard posBB = pos.ToBitboard();
                    if ((posBB & myAttacks.All()) != Bitboard.Empty)
                    {
                        retval += KingRingAttack; //attacking surrounding square in some aspect.

                        int myCount = myAttacks.AttackCountTo(pos);
                        int hisCount = hisAttacks.AttackCountTo(pos);
                        if (myCount > hisCount)
                        {
                            retval += KingRingAttackControlBonus * (myCount - hisCount);
                        }
                    }
                }
            }

            retval += KingAttackCountValue * myAttacks.KingAttackerCount;

            retval = (retval * KingQueenTropismFactor[myAttacks.KingQueenTropism]) / 100;
            myAttacks.KingAttackerScore = retval;

            return retval;

        }
        protected int EvaluateMyPieces(ChessBoard board, ChessPlayer me, ChessEvalInfo info)
        {
            int retval = 0;
            PhasedScore mobility = 0;
            var him = me.PlayerOther();
            var myAttacks = info.Attacks[(int)me];
            var hisAttacks = info.Attacks[(int)him];

            var hisKing = board.KingPosition(him);
            var hisKingZone = _kingSafetyRegion[(int)hisKing];


            Bitboard myPieces = board[me];
            Bitboard pieceLocationsAll = board.PieceLocationsAll;
            Bitboard pawns = board[ChessPieceType.Pawn];

            Bitboard slidersAndKnights = myPieces &
               (board[ChessPieceType.Knight]
               | board[ChessPieceType.Bishop]
               | board[ChessPieceType.Rook]
               | board[ChessPieceType.Queen]);

            Bitboard MobilityTargets = ~myPieces & ~(hisAttacks.PawnEast | hisAttacks.PawnWest);

            Bitboard myDiagSliders = myPieces & (board[ChessPieceType.Bishop] | board[ChessPieceType.Queen]);
            Bitboard myHorizSliders = myPieces & (board[ChessPieceType.Rook] | board[ChessPieceType.Queen]);

            while (slidersAndKnights != Bitboard.Empty) //foreach(ChessPosition pos in slidersAndKnights.ToPositions())
            {
                ChessPosition pos = BitboardInfo.PopFirst(ref slidersAndKnights);

                ChessPieceType pieceType = board.PieceAt(pos).ToPieceType();

                //generate attacks
                Bitboard slidingAttacks = Bitboard.Empty;

                switch (pieceType)
                {
                    case ChessPieceType.Knight:
                        slidingAttacks = Attacks.KnightAttacks(pos);
                        if (myAttacks.Knight != Bitboard.Empty)
                        {
                            myAttacks.Knight2 |= slidingAttacks;
                        }
                        else
                        {
                            myAttacks.Knight |= slidingAttacks;
                        }
                        break;
                    case ChessPieceType.Bishop:
                        slidingAttacks = Attacks.BishopAttacks(pos, pieceLocationsAll & ~myHorizSliders);
                        myAttacks.Bishop |= slidingAttacks;
                        break;
                    case ChessPieceType.Rook:
                        slidingAttacks = Attacks.RookAttacks(pos, pieceLocationsAll & ~myDiagSliders);
                        if (myAttacks.Rook != Bitboard.Empty)
                        {
                            myAttacks.Rook2 |= slidingAttacks;
                        }
                        else
                        {
                            myAttacks.Rook |= slidingAttacks;
                        }
                        if ((pos.ToFile().ToBitboard() & pawns & myPieces) == Bitboard.Empty)
                        {
                            if ((pos.ToFile().ToBitboard() & pawns) == Bitboard.Empty)
                            {
                                mobility = mobility.Add(RookFileOpen);
                            }
                            else
                            {
                                mobility = mobility.Add(RookFileHalfOpen);
                            }
                        }
                        break;
                    case ChessPieceType.Queen:
                        slidingAttacks = Attacks.QueenAttacks(pos, pieceLocationsAll & ~(myDiagSliders | myHorizSliders));
                        myAttacks.Queen |= slidingAttacks;

                        myAttacks.KingQueenTropism = hisKing.DistanceTo(pos) + hisKing.DistanceToNoDiag(pos);
                        break;
                }

                // calc mobility score
                int mobilityCount = (slidingAttacks & MobilityTargets).BitCount();
                mobility = mobility.Add(_mobilityPieceTypeCount[(int)pieceType][mobilityCount]);

                //see if involved in a king attack
                if((hisKingZone & slidingAttacks) != Bitboard.Empty)
                {
                    myAttacks.KingAttackerCount++;
                    myAttacks.KingAttackerWeight += _kingAttackerWeight[(int)pieceType];
                }
                
            }
            
            //decide if we
            if (myAttacks.KingAttackerCount >= 2 && myAttacks.KingAttackerWeight >= 5)
            {

            }


            myAttacks.Mobility = mobility;
            return retval;
        }

        protected virtual float CalcStartWeight(int basicMaterialCount)
        {
            //full material would be 62
            if (basicMaterialCount >= 56)
            {
                return 1;
            }
            else if (basicMaterialCount <= 10)
            {
                return 0;
            }
            else
            {
                int rem = basicMaterialCount - 10;
                float retval = (float)rem / 46f;
                return retval;
            }
        }

        protected bool UseEndGamePcSq(ChessBoard board, ChessPlayer winPlayer, out PhasedScore newPcSq)
        {
            ChessPlayer losePlayer = winPlayer.PlayerOther();
            if (
                board.PieceCount(losePlayer, ChessPieceType.Pawn) == 0
                && board.PieceCount(losePlayer, ChessPieceType.Queen) == 0
                && board.PieceCount(losePlayer, ChessPieceType.Rook) == 0
                && (board.PieceCount(losePlayer, ChessPieceType.Bishop) + board.PieceCount(losePlayer, ChessPieceType.Knight) <= 1))
            {
                if(board.PieceCount(winPlayer, ChessPieceType.Queen) > 0
                    || board.PieceCount(winPlayer, ChessPieceType.Rook) > 0
                    || board.PieceCount(winPlayer, ChessPieceType.Bishop) + board.PieceCount(winPlayer, ChessPieceType.Bishop) >= 2)
                {
                    ChessPosition loseKing = board.KingPosition(losePlayer);
                    ChessPosition winKing = board.KingPosition(winPlayer);
                    newPcSq = PhasedScoreInfo.Create(0, _endgameMateKingPcSq[(int)loseKing] - (winKing.DistanceTo(loseKing) * 25));
                    return true;
                }
            }
            newPcSq = 0;
            return false;
        }

        
    }

    
    public class ChessEvalAttackInfo
    {
        public Bitboard PawnEast;
        public Bitboard PawnWest;

        public Bitboard Knight;
        public Bitboard Knight2;
        public Bitboard Bishop;

        public Bitboard Rook;
        public Bitboard Rook2;
        public Bitboard Queen;
        public Bitboard King;

        public PhasedScore Mobility;

        public int KingQueenTropism;
        public int KingAttackerWeight;
        public int KingAttackerCount;
        public int KingAttackerScore;

        public void Reset()
        {
            PawnEast = Bitboard.Empty;
            PawnWest = Bitboard.Empty;
            Knight = Bitboard.Empty;
            Knight2 = Bitboard.Empty;
            Bishop = Bitboard.Empty;
            Rook = Bitboard.Empty;
            Rook2 = Bitboard.Empty;
            Queen = Bitboard.Empty;
            King = Bitboard.Empty;
            Mobility = 0;
            KingQueenTropism = 24; //init to far away.
            KingAttackerWeight = 0;
            KingAttackerCount = 0;
            KingAttackerScore = 0;
        }

        public Bitboard All()
        {
            return PawnEast | PawnWest | Knight | Knight2 | Bishop | Rook | Rook2 | Queen | King;
        }

        public int AttackCountTo(ChessPosition pos)
        {
            return 0
                + (int)(((ulong)PawnEast >> (int)pos) & 1)
                + (int)(((ulong)PawnWest >> (int)pos) & 1)
                + (int)(((ulong)Knight >> (int)pos) & 1)
                + (int)(((ulong)Knight2 >> (int)pos) & 1)
                + (int)(((ulong)Bishop >> (int)pos) & 1)
                + (int)(((ulong)Rook >> (int)pos) & 1)
                + (int)(((ulong)Rook2 >> (int)pos) & 1)
                + (int)(((ulong)Queen >> (int)pos) & 1)
                + (int)(((ulong)King >> (int)pos) & 1);
        }

        public ChessEvalAttackInfo Reverse()
        {
            return new ChessEvalAttackInfo()
            {
                PawnEast = PawnEast.Reverse(),
                PawnWest = PawnWest.Reverse(),
                Knight = Knight.Reverse(),
                Knight2 = Knight2.Reverse(),
                Bishop = Bishop.Reverse(),
                Rook = Rook.Reverse(),
                Rook2 = Rook2.Reverse(),
                Queen = Queen.Reverse(),
                King = King.Reverse()
            };
        }
    }


}
