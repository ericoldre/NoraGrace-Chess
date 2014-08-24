using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace NoraGrace.Engine.Evaluation
{

    public class Settings
    {

        #region helperclasses
        public class ChessEvalSettingsMobility
        {
            public int ExpectedAttacksAvailable { get; set; }
            public int AmountPerAttackDefault { get; set; }
        }

        public class PcSqDictionary
        {
            public int Offset { get; set; }
            public int Rank1 { get; set; }
            public int Rank2 { get; set; }
            public int Rank3 { get; set; }
            public int Rank4 { get; set; }
            public int Rank5 { get; set; }
            public int Rank6 { get; set; }
            public int Rank7 { get; set; }
            public int Rank8 { get; set; }

            public int FileAH { get; set; }
            public int FileBG { get; set; }
            public int FileCF { get; set; }
            public int FileDE { get; set; }

            public int Center4 { get; set; }
            public int CenterBorder { get; set; }
            public int OutsideEdge { get; set; }

            private const Bitboard bbCenter4 = Bitboard.D4 | Bitboard.D5 | Bitboard.E4 | Bitboard.E5;

            private const Bitboard bbCenterBorder = 
                Bitboard.C6 | Bitboard.D6 | Bitboard.E6 | Bitboard.F6
                | Bitboard.C5 | Bitboard.F5
                | Bitboard.C4 | Bitboard.F4
                | Bitboard.C3 | Bitboard.D3 | Bitboard.E3 | Bitboard.F3;

            private const Bitboard bbOutsiteEdge = Bitboard.Rank1 | Bitboard.Rank8 | Bitboard.FileA | Bitboard.FileH;

            public int this[Position pos]
            {
                get
                {
                    int retval = 0;
                    var rank = pos.ToRank();
                    var file = pos.ToFile();
                    switch (rank)
                    {
                        case Rank.Rank8:
                            retval += Rank8;
                            break;
                        case Rank.Rank7:
                            retval += Rank7;
                            break;
                        case Rank.Rank6:
                            retval += Rank6;
                            break;
                        case Rank.Rank5:
                            retval += Rank5;
                            break;
                        case Rank.Rank4:
                            retval += Rank4;
                            break;
                        case Rank.Rank3:
                            retval += Rank3;
                            break;
                        case Rank.Rank2:
                            retval += Rank2;
                            break;
                        case Rank.Rank1:
                            retval += Rank1;
                            break;
                    }
                    switch (file)
                    {
                        case File.FileA:
                        case File.FileH:
                            retval += FileAH;
                            break;
                        case File.FileB:
                        case File.FileG:
                            retval += FileBG;
                            break;
                        case File.FileC:
                        case File.FileF:
                            retval += FileCF;
                            break;
                        case File.FileD:
                        case File.FileE:
                            retval += FileDE;
                            break;
                    }

                    if (bbCenter4.Contains(pos)) { retval += Center4; }
                    if (bbCenterBorder.Contains(pos)) { retval += CenterBorder; }
                    if (bbOutsiteEdge.Contains(pos)) { retval += OutsideEdge; }

                    retval += Offset;
                    return retval;
                }
            }
        }

        #endregion

        #region DefaultEvalSettings

        public static Settings Default()
        {
            Settings retval = new Settings()
            {
                //MaterialValues = new ChessPieceTypeDictionary<ChessGameStageDictionary<int>>()
                //{
                //    Pawn = new ChessGameStageDictionary<int>() { Opening = 60, Endgame = 130 },
                //    Knight = new ChessGameStageDictionary<int>() { Opening = 385, Endgame = 380 },
                //    Bishop = new ChessGameStageDictionary<int>() { Opening = 390, Endgame = 415 },
                //    Rook = new ChessGameStageDictionary<int>() { Opening = 550, Endgame = 745 },
                //    Queen = new ChessGameStageDictionary<int>() { Opening = 1255, Endgame = 1325 },
                //    King = new ChessGameStageDictionary<int>() { Opening = 0, Endgame = 0 },
                //},
                //MaterialBishopPair = new ChessGameStageDictionary<int>() { Opening = 25, Endgame = 75 },

                MaterialValues = new ChessGameStageDictionary<MaterialSettingsPhase>()
                {
                    Opening = new MaterialSettingsPhase()
                    {
                        Pawn = new MaterialSettingsPhase.PawnSettings(){ First = 60, Eighth = 60, Curve = new Helpers.Point(){ X = .5, Y = .5}},
                        Knight = new MaterialSettingsPhase.PieceSettings() { BaseValue = 385, PairBonus = 0 },
                        Bishop = new MaterialSettingsPhase.PieceSettings() { BaseValue = 390, PairBonus = 25 },
                        Rook = new MaterialSettingsPhase.PieceSettings() { BaseValue = 550, PairBonus = 0 },
                        Queen = new MaterialSettingsPhase.PieceSettings() { BaseValue = 1255, PairBonus = 0 }
                    },
                    Endgame = new MaterialSettingsPhase()
                    {
                        Pawn = new MaterialSettingsPhase.PawnSettings() { First = 130, Eighth = 130, Curve = new Helpers.Point() { X = .5, Y = .5 } },
                        Knight = new MaterialSettingsPhase.PieceSettings() { BaseValue = 380, PairBonus = 0 },
                        Bishop = new MaterialSettingsPhase.PieceSettings() { BaseValue = 415, PairBonus = 75 },
                        Rook = new MaterialSettingsPhase.PieceSettings() { BaseValue = 745, PairBonus = 0 },
                        Queen = new MaterialSettingsPhase.PieceSettings() { BaseValue = 1325, PairBonus = 0 }
                    }
                },

                PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<PcSqDictionary>>()
                {
                    Pawn = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = 0, Rank2 = -6, Rank3 = 0, Rank4 = 6, Rank5 = 15, Rank6 = 45, Rank7 = 15, Rank8 = 0,
                            FileAH = -7, FileBG = 7, FileCF = 3, FileDE = 5,
                            Center4 = 10, CenterBorder = 2, OutsideEdge = -6
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = 0, Rank2 = 2, Rank3 = 3, Rank4 = 3, Rank5 = 6, Rank6 = 17, Rank7 = 40, Rank8 = 0,
                            FileAH = 0, FileBG = 9, FileCF = 16, FileDE = 12,
                            Center4 = -21, CenterBorder = -9, OutsideEdge = 18
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,  0,  0,  0,  0,  0,  0,  0,	
                        //    -10, -5, 0, 5,  5,  0, -5, -10,
                        //    -10, -5, 0, 5,  5,  0, -5, -10,	
                        //    -10, -5, 0, 10, 10, 0, -5, -10,	
                        //    -10, -5, 5, 15, 15, 0, -5, -10,	
                        //    -10, -5, 5, 10, 10, 0, -5, -10,	
                        //    -10, -5, 0, 5,  5,  5, -5, -10,	
                        //     0,  0,  0,  0,  0,  0,  0,  0,	
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,  0,  0,  0,  0,  0,  0,  0,	
                        //    20, 25, 34, 38, 38, 34, 25, 20,
                        //    12, 20, 24, 32, 32, 24, 20, 12,	
                        //     6, 11, 18,	27, 27, 16,	11,  6,	
                        //     4,  7, 10, 12, 12, 10,  7,  4,	
                        //    -3, -3, -3, -3, -3, -3, -3,  -3,	
                        //    -10,-10,-10,-10,-10,-10,-10,-10,	
                        //     0,  0,  0,  0,  0,  0,  0,  0,	
                        //})
                    },
                    Knight = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = -9, Rank2 = 0, Rank3 = 3, Rank4 = 15, Rank5 = 15, Rank6 = 27, Rank7 = 21, Rank8 = -57,
                            FileAH = -6, FileBG = -3, FileCF = 6, FileDE = 18,
                            Center4 = -3, CenterBorder = 6, OutsideEdge = -6
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = 0, Rank2 = 0, Rank3 = 0, Rank4 = 13, Rank5 = 22, Rank6 = 14, Rank7 = 9, Rank8 = 7,
                            FileAH = 1, FileBG = 6, FileCF = 2, FileDE = 7,
                            Center4 = 3, CenterBorder = -3, OutsideEdge = 3
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        //     2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
                        //     3,	 7,	14,	16,	16,	14,	 7,	 3,	
                        //     4,	 9,	15,	16,	16,	15,	 9,	 4,	
                        //     4,	 9,	13,	15,	15,	13,	 9,	 4,	
                        //     3,	 7,	11,	13,	13,	11,	 7,	 3,	
                        //     2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
                        //    -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        //     2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
                        //     3,	 7,	14,	16,	16,	14,	 7,	 3,	
                        //     4,	 9,	15,	16,	16,	15,	 9,	 4,	
                        //     4,	 9,	13,	15,	15,	13,	 9,	 4,	
                        //     3,	 7,	11,	13,	13,	11,	 7,	 3,	
                        //     2,	 3,	 7,	10,	10,	 7,	 3,	 2,	
                        //    -7,	-3,	 1,	 3,	 3,	 1,	-3,	-7,	
                        //})
                    },
                    Bishop = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = 0, Rank2 = 12, Rank3 = 12, Rank4 = -3, Rank5 = -12, Rank6 = 21, Rank7 = -21, Rank8 = -24,
                            FileAH = 0, FileBG = 9, FileCF = -3, FileDE = -9,
                            Center4 = 30, CenterBorder = 4, OutsideEdge = -6
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = -3, Rank2 = 3, Rank3 = 3, Rank4 = 0, Rank5 = 3, Rank6 = 6, Rank7 = 6, Rank8 = 12,
                            FileAH = 0, FileBG = 0, FileCF = -3, FileDE = 0,
                            Center4 = 2, CenterBorder = 7, OutsideEdge = 1
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
                        //    -3,	 9,	11,	11,	11,	11,	 9,	-3,	
                        //    -3,	 8,	12,	12,	12,	12,	 8,	-3,	
                        //    -3,	 0,	12,	12,	12,	12,	 6,	-3,	
                        //    -3,	 0,	12,	12,	12,	12,	 6,	-3,	
                        //     6,	 8,	12,	12,	12,	12,	 8,	 6,	
                        //     6,	 9,	11,	11,	11,	11,	 9,	 6,	
                        //    -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,	
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,
                        //    -3,	 9,	11,	11,	11,	11,	 9,	-3,	
                        //    -3,	 8,	12,	12,	12,	12,	 8,	-3,	
                        //    -3,	 0,	12,	12,	12,	12,	 6,	-3,	
                        //    -3,	 0,	12,	12,	12,	12,	 6,	-3,	
                        //     6,	 8,	12,	12,	12,	12,	 8,	 6,	
                        //     6,	 9,	11,	11,	11,	11,	 9,	 6,	
                        //    -3,	-3,	-3,	-3,	-3,	-3,	-3,	-3,		
                        //})
                    },
                    Rook = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = 6, Rank2 = -12, Rank3 = -9, Rank4 = -3, Rank5 = 21, Rank6 = 48, Rank7 = 37, Rank8 = 36,
                            FileAH = -6, FileBG = 6, FileCF = 22, FileDE = 25,
                            Center4 = -6, CenterBorder = -6, OutsideEdge = 3
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = -9, Rank2 = -12, Rank3 = -6, Rank4 = 0, Rank5 = 6, Rank6 = 15, Rank7 = 21, Rank8 = 15,
                            FileAH = 0, FileBG = 0, FileCF = -3, FileDE = -6,
                            Center4 = 3, CenterBorder = 0, OutsideEdge = 0
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,	0,	4,	6,	6,	4,	0,	0,	
                        //    10,	10,	14,	16,	16,	14,	10,	10,
                        //    0,	0,	4,	6,	6,	4,	0,	0,
                        //    0,	0,	4,	6,	6,	4,	0,	0,
                        //    0,	0,	4,	6,	6,	4,	0,	0,	
                        //    0,	0,	4,	6,	6,	4,	0,	0,	
                        //    0,	0,	4,	6,	6,	4,	0,	0,	
                        //    0,	0,	4,	6,	6,	4,	0,	0	
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0	
                        //})
                    },
                    Queen = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = 9, Rank2 = 15, Rank3 = 6, Rank4 = 3, Rank5 = -18, Rank6 = 12, Rank7 = -24, Rank8 = -33,
                            FileAH = -6, FileBG = 0, FileCF = 3, FileDE = 6,
                            Center4 = -7, CenterBorder = 1, OutsideEdge = 5
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = -39, Rank2 = -15, Rank3 = 12, Rank4 = 18, Rank5 = 42, Rank6 = 27, Rank7 = 57, Rank8 = 57,
                            FileAH = 0, FileBG = 0, FileCF = 0, FileDE = 0,
                            Center4 = -1, CenterBorder = 7, OutsideEdge = -13
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,  0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	2,	2,	0,	0,	0,	
                        //    0,	0,	0,	2,	2,	0,	0,	0,	
                        //    0,	1,	1,	1,	1,	0,	0,	0,	
                        //    0,	0,	1,	1,	1,	0,	0,	0,	
                        //    -2,-2, -2,  0,	0, -2, -2, -2
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,  0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	0,	0,	0,	0,	0,	
                        //    0,	0,	0,	2,	2,	0,	0,	0,	
                        //    0,	0,	0,	2,	2,	0,	0,	0,	
                        //    0,	1,	1,	1,	1,	0,	0,	0,	
                        //    0,	0,	1,	1,	1,	0,	0,	0,	
                        //    -2,-2, -2,  0,	0, -2, -2, -2	
                        //})
                    },
                    King = new ChessGameStageDictionary<PcSqDictionary>()
                    {
                        Opening = new PcSqDictionary()
                        {
                            Rank1 = 0, Rank2 = -5, Rank3 = -23, Rank4 = -64, Rank5 = -80, Rank6 = -80, Rank7 = -80, Rank8 = -80,
                            FileAH = 9, FileBG = 26, FileCF = -1, FileDE = -3,
                            Center4 = 0, CenterBorder = 0, OutsideEdge = 0
                        },
                        Endgame = new PcSqDictionary()
                        {
                            Rank1 = -27, Rank2 = -9, Rank3 = 0, Rank4 = 15, Rank5 = 33, Rank6 = 69, Rank7 = 60, Rank8 = 6,
                            FileAH = -9, FileBG = 0, FileCF = 6, FileDE = 6,
                            Center4 = 7, CenterBorder = 7, OutsideEdge = -5
                        }
                        //Opening = new ChessPositionDictionary<int>(new int[]
                        //{
                        //    -80,-80,-80,-80,-80,-80,-80,-80,
                        //    -80,-80,-80,-80,-80,-80,-80,-80,
                        //    -80,-80,-80,-80,-80,-80,-80,-80,
                        //    -60,-60,-60,-60,-60,-60,-60,-60,
                        //    -40,-40,-40,-40,-40,-40,-40,-40,
                        //    -7,	-15,-15,-15,-15,-15,-15,-7,	
                        //    -5,	-5,	-12,-12,-12,-12,-5,	-5,	
                        //     3,	 3,	 8,	-5,  -8,-5, 10,	 5		
                        //}),
                        //Endgame = new ChessPositionDictionary<int>(new int[]
                        //{
                        //     -8, -8, -8, -8, -8, -8, -8, -8,
                        //     -8, -0, -0, -0, -0, -0, -0, -8,
                        //     -8, -0, 05, 05, 05, 05, -0, -8,
                        //     -8, -0, 05, 30, 30, 05, -0, -8,
                        //     -8, -0, 05, 30, 30, 05, -0, -8,
                        //     -8, -0, 05, 30, 30, 05, -0, -8,
                        //     -8, -0, -0, -0, -0, -0, -0, -8,
                        //     -8, -8, -8, -8, -8, -8, -8, -8,
                        //})
                    },
                },




                PawnDoubled = new ChessGameStageDictionary<int>() { Opening = 8, Endgame = 2 },
                PawnIsolated = new ChessGameStageDictionary<int>() { Opening = 15, Endgame = 15 },
                PawnUnconnected = new ChessGameStageDictionary<int>(){ Opening = 5, Endgame = 0},
                
                PawnPassed8thRankScore = 417,
                PawnPassedRankReduction = .57f,
                PawnPassedDangerPct = .20f,
                
                PawnPassedOpeningPct = .31f,
                PawnPassedMinScore = 0,
                PawnCandidatePct = .30f,
                PawnPassedClosePct = .99f,
                PawnPassedFarPct = 1.1f,
                PawnShelterFactor = 7,

                RookFileOpen = 20,

                KingAttackCountValue = 6,
                KingAttackWeightValue = 8,
                KingAttackWeightCutoff = 6,
                KingRingAttack = 5,
                KingRingAttackControlBonus = 35,
                KingAttackFactor = .68f,
                KingAttackFactorQueenTropismBonus = 2.15f,

                Mobility = new ChessPieceTypeDictionary<ChessGameStageDictionary<Helpers.Mobility>>()
                {
                    Knight = new ChessGameStageDictionary<Helpers.Mobility>()
                    {
                        Opening = new Helpers.Mobility() { ExpectedAttacksAvailable = 3, Amplitude = 54, BezControlPct = new Helpers.Point(.3, .6)},
                        Endgame = new Helpers.Mobility() { ExpectedAttacksAvailable = 4, Amplitude = 120, BezControlPct = new Helpers.Point(.4, 1.3) },
                    },
                    Bishop = new ChessGameStageDictionary<Helpers.Mobility>()
                    {
                        Opening = new Helpers.Mobility() { ExpectedAttacksAvailable = 4, Amplitude = 54, BezControlPct = new Helpers.Point(-0.2, .4) },
                        Endgame = new Helpers.Mobility() { ExpectedAttacksAvailable = 7, Amplitude = 82, BezControlPct = new Helpers.Point(.3, 1.1) },
                    },
                    Rook = new ChessGameStageDictionary<Helpers.Mobility>()
                    {
                        Opening = new Helpers.Mobility() { ExpectedAttacksAvailable = 5, Amplitude = 50, BezControlPct = new Helpers.Point(.7, 1.3) },
                        Endgame = new Helpers.Mobility() { ExpectedAttacksAvailable = 8, Amplitude = 98, BezControlPct = new Helpers.Point(.1, .9) },
                    },
                    Queen = new ChessGameStageDictionary<Helpers.Mobility>()
                    {
                        Opening = new Helpers.Mobility() { ExpectedAttacksAvailable = 8, Amplitude = 28, BezControlPct = new Helpers.Point(.7, .9) },
                        Endgame = new Helpers.Mobility() { ExpectedAttacksAvailable = 12, Amplitude = 120, BezControlPct = new Helpers.Point(.35, 1.4) },
                    },
                }
            };

            
            return retval;
        }

        #endregion


        public ChessPieceTypeDictionary<ChessGameStageDictionary<PcSqDictionary>> PcSqTables = new ChessPieceTypeDictionary<ChessGameStageDictionary<PcSqDictionary>>();
        public ChessGameStageDictionary<MaterialSettingsPhase> MaterialValues = new ChessGameStageDictionary<MaterialSettingsPhase>();
        public ChessGameStageDictionary<int> MaterialBishopPair = new ChessGameStageDictionary<int>();

        public ChessGameStageDictionary<int> PawnDoubled = new ChessGameStageDictionary<int>();
        public ChessGameStageDictionary<int> PawnIsolated = new ChessGameStageDictionary<int>();
        public ChessGameStageDictionary<int> PawnUnconnected = new ChessGameStageDictionary<int>();
        public ChessPieceTypeDictionary<ChessGameStageDictionary<Helpers.Mobility>> Mobility = new ChessPieceTypeDictionary<ChessGameStageDictionary<Helpers.Mobility>>();
        
        public int PawnPassed8thRankScore = 0;
        public double PawnPassedRankReduction = 0;
        public int PawnPassedMinScore = 0;
        public double PawnPassedDangerPct = 0;
        public double PawnPassedOpeningPct = 0;
        public double PawnCandidatePct = 0;
        public double PawnPassedClosePct = 0;
        public double PawnPassedFarPct = 0;
        public int PawnShelterFactor = 0;

        public int RookFileOpen = 0;

        public int KingAttackCountValue = 0;
        public int KingAttackWeightValue = 0;
        public int KingAttackWeightCutoff = 0;
        public int KingRingAttack = 0;
        public int KingRingAttackControlBonus = 0;

        public double KingAttackFactor = 0;
        public double KingAttackFactorQueenTropismBonus = 0;


        public Settings CloneDeep()
        {
            return DeserializeObject<Settings>(SerializeObject<Settings>(this));
        }

        public override bool Equals(object obj)
        {
            Settings other = obj as Settings;
            if (other == null) { return false; }

            var v1 = SerializeObject<Settings>(this);
            var v2 = SerializeObject<Settings>(other);
            return v1 == v2;
        }

        public override int GetHashCode()
        {
            return 1;// SerializeObject<ChessEvalSettings>(this).GetHashCode();
        }


        public static Settings Load(System.IO.Stream stream, bool applyDefaultSettings = true)
        {
            XmlDocument loadDoc = new XmlDocument();
            loadDoc.Load(stream);

            if (applyDefaultSettings)
            {
                ApplyDefaultValuesToXml(loadDoc.SelectSingleNode("ChessEvalSettings") as XmlElement, getDefaultDoc().SelectSingleNode("ChessEvalSettings") as XmlElement);
            }

            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(Settings));

            using (var memory = new System.IO.MemoryStream())
            {
                loadDoc.Save(memory);
                memory.Position = 0;
                return (Settings)xs.Deserialize(memory);
            }
        }

        public static Settings Load(System.IO.FileInfo file, bool applyDefaultSettings = true)
        {
            using (var stream = file.OpenRead())
            {
                return Load(stream);
            }
        }

        public void Save(System.IO.Stream stream)
        {
            using (var writer = new System.IO.StreamWriter(stream))
            {
                writer.Write(SerializeObject<Settings>(this));
            }
        }
        public void Save(System.IO.FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
            using (var stream = file.OpenWrite())
            {
                Save(stream);
            }
        }
        public void Save(string fileName)
        {
            Save(new System.IO.FileInfo(fileName));
        }


        private static void ApplyDefaultValuesToXml(XmlElement ele, XmlElement defEle)
        {
            foreach (XmlNode nodeChildDefault in defEle.ChildNodes)
            {
                if (nodeChildDefault is XmlElement)
                {
                    XmlElement eleChildDefault = nodeChildDefault as XmlElement;
                    XmlElement match = ele.SelectSingleNode(eleChildDefault.Name) as XmlElement;
                    if (match == null)
                    {
                        match = ele.OwnerDocument.CreateElement(eleChildDefault.Name);
                        ele.AppendChild(match);
                    }
                    ApplyDefaultValuesToXml(match, eleChildDefault);
                }
                else if(nodeChildDefault is XmlText)
                {
                    if (ele.InnerXml == "")
                    {
                        ele.InnerXml = defEle.InnerXml;
                    }                    
                }
                
            }
            if (defEle.ChildNodes.Count == 0)
            {
                
            }
        }

        private static XmlDocument _defaultDoc;
        private static XmlDocument getDefaultDoc()
        {
            if (_defaultDoc == null)
            {
                _defaultDoc = new XmlDocument();
                _defaultDoc.LoadXml(SerializeObject<Settings>(Settings.Default()));
            }
            return _defaultDoc;
        }

        /// <summary>
        /// Serialize an object into an XML string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

                System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, Encoding.UTF8);

                xs.Serialize(writer, obj);
                memoryStream.Position = 0;
                System.IO.StreamReader reader = new System.IO.StreamReader(memoryStream, Encoding.UTF8);
                string retval = reader.ReadToEnd();
                return retval;
                //return reader.ReadToEnd();
                //xmlString = UTF8ByteArrayToString(memoryStream.ToArray()); 
                //return xmlString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reconstruct an object from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, Encoding.UTF8);
            writer.Write(xml);
            writer.Flush();




            memoryStream.Position = 0;


            System.IO.StreamReader reader = new System.IO.StreamReader(memoryStream, Encoding.UTF8);
            return (T)xs.Deserialize(reader);
        }


        

    }

}
