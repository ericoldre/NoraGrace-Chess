using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static class MagicBitboards
    {

        #region magicarrays
        private static readonly ulong[] _bishopMagics = new ulong[]
        {
            11533720853379293696UL, //A8
            4634221887939944448UL, //B8
            2469107311143059968UL, //C8
            4631963212403773698UL, //D8
            4612816453819826288UL, //E8
            286010531514368UL, //F8
            9522318258233933892UL, //G8
            44564589584384UL, //H8
            4611844485559689760UL, //A7
            9234639900802844704UL, //B7
            1152939099510816896UL, //C7
            4612253436254175747UL, //D7
            163611745976713216UL, //E7
            9079773542227200UL, //F7
            579279902869555712UL, //G7
            276019808288UL, //H7
            2533292243167252UL, //A6
            578818242689180172UL, //B6
            4521196125167876UL, //C6
            4613940019445383360UL, //D6
            9288709128585248UL, //E6
            9223408325043949572UL, //F6
            288511859852706384UL, //G6
            2306406097981834240UL, //H6
            9242019754598990849UL, //A5
            2319397788830531718UL, //B5
            2306970008900665377UL, //C5
            6917599675625308672UL, //D5
            281543712981000UL, //E5
            9799974643371622914UL, //F5
            864977001780478404UL, //G5
            576779610692288648UL, //H5
            5766863789614178434UL, //A4
            2342507332688152577UL, //B4
            4629772993306838017UL, //C4
            11261200239689856UL, //D4
            2341946590220009728UL, //E4
            2378041967813656835UL, //F4
            9259966266318524544UL, //G4
            2315133883006616576UL, //H4
            11260116309381120UL, //A3
            81137431994175636UL, //B3
            9223935056737747972UL, //C3
            36169809657856512UL, //D3
            36037597411148800UL, //E3
            9223671246053441600UL, //F3
            4556930643132672UL, //G3
            288516266363846720UL, //H3
            36877689021792364UL, //A2
            1848165255562330128UL, //B2
            75333735286019UL, //C2
            182431004186124418UL, //D2
            324681420266274946UL, //E2
            583849532784640UL, //F2
            4538788311334912UL, //G2
            2260630406449152UL, //H2
            18085883964031488UL, //A1
            709176350153515040UL, //B1
            72061992638613568UL, //C1
            1152921513742304320UL, //D1
            4611686091979247680UL, //E1
            9259409771768840320UL, //F1
            3396400276832386UL, //G1
            866978129825105952UL, //H1
        };


        private static readonly ulong[] _rookMagics = new ulong[]
        {
            5800636870882762768UL, //A8
            90072129990692872UL, //B8
            9871903714778873872UL, //C8
            9259409630235230214UL, //D8
            36037593179127810UL, //E8
            72066394426179586UL, //F8
            1189091588903534848UL, //G8
            1765411603689251072UL, // rook H8
            4644337656791176UL, //A7
            8070591407179432064UL, //B7
            612630355532316800UL, //C7
            140806208356480UL, //D7
            2306124518566924292UL, //E7
            2328923995992228880UL, //F7
            868209638683836544UL, //G7
            4616893306587056256UL, //H7
            36033469945481285UL, //A6
            1896015718537773056UL, //B6
            282575564177424UL, //C6
            1441434455784816648UL, //D6
            9148486633128960UL, //E6
            9261935208310243330UL, //F6
            3518986981474816UL, //G6
            9223444604689334401UL, //H6
            10682538455709270144UL, //A5
            5084599199107907649UL, //B5
            180179171615441024UL, //C5
            2305851947041685764UL, //D5
            8798240768128UL, //E5
            19141399001825344UL, //F5
            54047597886702080UL, //G5
            2450310324478312708UL, //H5
            36169809393614880UL, //A4
            1170971156212105216UL, //B4
            17594341924864UL, //C4
            9655735195424262144UL, //D4
            36310306372194308UL, //E4
            2201179128832UL, //F4
            140746086678784UL, //G4
            576812888115642497UL, //H4
            900790296365793316UL, //A3
            1026891221492056064UL, //B3
            1179951898732544064UL, //C3
            324276765490970752UL, //D3
            11259016256716809UL, //E3
            36591764160675842UL, //F3
            288793343419351041UL, //G3
            2291384920965121UL, //H3
            324259310613823552UL, //A2
            35253095759936UL, //B2
            4665870020165763200UL, //C2
            8796361490560UL, //D2
            2323861805903937664UL, //E2
            578714753288110208UL, //F2
            4756082698693378304UL, //G2
            10376329827499377152UL, //H2
            1193489635382149377UL, //A1
            9223442409902317585UL, //B1
            4649984246102884417UL, //C1
            2814888280327186UL, //D1
            1688918716056578UL, //E1
            1155455063334848553UL, //F1
            9331608030229332484UL, //G1
            45071337550774534UL, //H1
        };

        #endregion

        public static ChessBitboard BishopAttacks(ChessPosition pos, ChessBitboard allPieces)
        {
            //mask all pieces to relevant squares
            //multiply by magic number (index now stored in high order bits)
            //shift result to lower bits
            //lookup attacks in table
            return _bishopLookup[(int)pos][((ulong)(allPieces & _bishopMask[(int)pos]) * _bishopMagics[(int)pos]) >> _bishopShift[(int)pos]];
        }

        public static ChessBitboard RookAttacks(ChessPosition pos, ChessBitboard allPieces)
        {
            //mask all pieces to relevant squares
            //multiply by magic number (index now stored in high order bits)
            //shift result to lower bits
            //lookup attacks in table
            return _rookLookup[(int)pos][((ulong)(allPieces & _rookMask[(int)pos]) * _rookMagics[(int)pos]) >> _rookShift[(int)pos]];
        }

        public static ChessBitboard QueenAttacks(ChessPosition pos, ChessBitboard allPieces)
        {
            return RookAttacks(pos, allPieces) | BishopAttacks(pos, allPieces);
        }

        public static void Test()
        {
            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = BishopMask(position);

                var distinctCount = Combinations(mask).Distinct().Count();
                var possibleCombinations = 1 << mask.BitCount();
                if (distinctCount != possibleCombinations)
                {
                    throw new Exception("bad");
                }

                foreach (var bitset in Combinations(mask))
                {
                    var calcAttacks = BishopAttacksCalc(position, bitset);
                    var lookupAttacks = BishopAttacks(position, bitset);

                    if (calcAttacks != lookupAttacks)
                    {
                        throw new Exception("bad magic");
                    }
                }
            }

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = RookMask(position);

                var distinctCount = Combinations(mask).Distinct().Count();
                var possibleCombinations = 1 << mask.BitCount();
                if (distinctCount != possibleCombinations)
                {
                    throw new Exception("bad");
                }

                foreach (var bitset in Combinations(mask))
                {
                    var calcAttacks = RookAttacksCalc(position, bitset);
                    var lookupAttacks = RookAttacks(position, bitset);

                    if (calcAttacks != lookupAttacks)
                    {
                        throw new Exception("bad magic");
                    }
                }
            }

            Console.WriteLine("all good");

        }

        private static ChessBitboard[][] _bishopLookup = new ChessBitboard[64][];
        private static ChessBitboard[][] _rookLookup = new ChessBitboard[64][];
        private static int[] _bishopShift = new int[64];
        private static int[] _rookShift = new int[64];
        private static ChessBitboard[] _bishopMask = new ChessBitboard[64];
        private static ChessBitboard[] _rookMask = new ChessBitboard[64];

        static MagicBitboards()
        {

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = BishopMask(position);
                var possibleCombinations = 1 << mask.BitCount();
                var shift = 64 - mask.BitCount();

                _bishopShift[(int)position] = shift;
                _bishopMask[(int)position] = mask;

                var magic = _bishopMagics[(int)position];

                _bishopLookup[(int)position] = new ChessBitboard[possibleCombinations];

                foreach (var bitset in Combinations(mask))
                {
                    var attacks = BishopAttacksCalc(position, bitset);
                    var index = (magic * (ulong)bitset) >> shift;
                    _bishopLookup[(int)position][index] = attacks;
                }
            }

            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = RookMask(position);
                var possibleCombinations = 1 << mask.BitCount();
                var shift = 64 - mask.BitCount();

                _rookShift[(int)position] = shift;
                _rookMask[(int)position] = mask;

                var magic = _rookMagics[(int)position];

                _rookLookup[(int)position] = new ChessBitboard[possibleCombinations];

                foreach (var bitset in Combinations(mask))
                {
                    var attacks = RookAttacksCalc(position, bitset);
                    var index = (magic * (ulong)bitset) >> shift;
                    _rookLookup[(int)position][index] = attacks;
                }
            }
        }

        private static IEnumerable<ChessBitboard> Combinations(ChessBitboard mask)
        {
            var positions = mask.ToPositions().ToArray();
            var bitCount = mask.BitCount();
            var possibleCombinations = 1 << mask.BitCount();
            for (int i = 0; i < possibleCombinations; i++)
            {
                ChessBitboard combo = ChessBitboard.Empty;
                for (int b = 0; b < bitCount; b++)
                {
                    int r = i & (1 << b);
                    if (r != 0)
                    {
                        combo |= positions[b].Bitboard();
                    }
                }
                yield return combo;
            }

        }


        public static ChessBitboard RookMask(ChessPosition position)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsRook)
            {
                ChessPosition p = position.PositionInDirection(dir);
                ChessPosition next = p.PositionInDirection(dir);
                while (p.PositionInDirection(dir) != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    p = p.PositionInDirection(dir);
                    next = p.PositionInDirection(dir);
                }
            }
            return retval;
        }

        public static ChessBitboard BishopMask(ChessPosition position)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsBishop)
            {
                ChessPosition p = position.PositionInDirection(dir);
                while (p.PositionInDirection(dir) != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    p = p.PositionInDirection(dir);
                }
            }
            return retval;
        }

        public static ChessBitboard RookAttacksCalc(ChessPosition position, ChessBitboard blockers)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsRook)
            {
                ChessPosition p = position.PositionInDirection(dir);
                while (p != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    if (blockers.Contains(p)) { break; }
                    p = p.PositionInDirection(dir);
                }
            }
            return retval;
        }

        public static ChessBitboard BishopAttacksCalc(ChessPosition position, ChessBitboard blockers)
        {
            ChessBitboard retval = ChessBitboard.Empty;
            foreach (ChessDirection dir in ChessDirectionInfo.AllDirectionsBishop)
            {
                ChessPosition p = position.PositionInDirection(dir);
                while (p != ChessPosition.OUTOFBOUNDS)
                {
                    retval |= p.Bitboard();
                    if (blockers.Contains(p)) { break; }
                    p = p.PositionInDirection(dir);
                }
            }
            return retval;
        }



        public static class Generation
        {
            private static readonly Random _rand = new Random(231456);
            private static ulong Rand64()
            {
                byte[] bytes = new byte[8];
                _rand.NextBytes(bytes);
                ulong retval = 0;
                for (int i = 0; i <= 7; i++)
                {
                    //ulong ibyte = (ulong)bytes[i]&256;
                    ulong ibyte = (ulong)bytes[i];
                    retval |= ibyte << (i * 8);
                }
                return retval;
            }

            static int[] boosters = new int[] { 3191, 2184, 1310, 1308, 2452, 3996, 
                1059, 3608, 2029, 3043 };
            static ulong Rand64Few()
            {
                //return Rand64() & Rand64() & Rand64();

                int booster = boosters[_rand.Next(boosters.Length)];

                int s1 = booster & 63;
                int s2 = (booster >> 6) & 63;

                ulong magic = Rand64();
                magic = (magic >> s1) | (magic << (64 - s1));
                magic &= Rand64();
                magic = (magic >> s2) | (magic << (64 - s2));
                magic &= Rand64();
                return magic;

            }

            static ChessBitboard index_to_mask(int index, int bitCount, ChessBitboard m)
            {
                int i;
                ulong result = 0UL;
                for (i = 0; i < bitCount; i++)
                {

                    var j = m.NorthMostPosition();
                    m = m & ~j.Bitboard();

                    if ((index & (1 << i)) != 0)
                    {
                        result |= (1UL << (int)j);
                    }
                }
                return (ChessBitboard)result;
            }


            private static int Transform(ulong b, ulong magic, int bitCount)
            {
                return (int)((b * magic) >> (64 - bitCount));
            }
            public static int totalCalcs = 0;

            private static ulong find_magic(ChessPosition sq, bool bishop)
            {
                ChessBitboard[] b = new ChessBitboard[4096];
                ChessBitboard[] a = new ChessBitboard[4096];
                ChessBitboard[] used = new ChessBitboard[4096];


                ChessBitboard allAttacks = bishop ? BishopAttacksCalc(sq, ChessBitboard.Empty) : RookAttacksCalc(sq, ChessBitboard.Empty);
                ChessBitboard mask = bishop ? BishopMask(sq) : RookMask(sq);

                int j, k;

                int bitCount = mask.BitCount();
                int possibleCombinations = 1 << bitCount;
                ulong bitsLow = (1UL << bitCount) - 1UL;
                ulong bitsHigh = bitsLow << (64 - bitCount);

                if (possibleCombinations > 4096)
                {
                    var tmp = RookMask(ChessPosition.A8);
                }
                for (int i = 0; i < possibleCombinations; i++)
                {
                    b[i] = index_to_mask(i, bitCount, mask);
                    a[i] = bishop ? BishopAttacksCalc(sq, b[i]) : RookAttacksCalc(sq, b[i]);
                }

                for (k = 0; k < 100000000; k++)
                {
                    totalCalcs++;

                    ulong magic = Rand64Few();

                    var mul = (ulong)mask * magic;

                    var mulmask = mul & bitsHigh;
                    if (mulmask != bitsHigh) { continue; }


                    for (int i = 0; i < 4096; i++) { used[i] = 0UL; }

                    bool fail = false;
                    for (int i = 0; i < possibleCombinations; i++)
                    {
                        j = Transform((ulong)b[i], magic, bitCount);
                        if (used[j] == 0UL)
                        {
                            used[j] = a[i];
                        }
                        else if (used[j] != a[i])
                        {
                            fail = true;
                            break;
                        }
                    }
                    if (!fail)
                    {
                        return magic;
                    }
                }

                return 0UL;
            }

            public static void FindMagics()
            {

                var magicH8 = find_magic(ChessPosition.H8, false);
                Console.WriteLine(string.Format("\t{0}UL, // rook {1}", magicH8, ChessPosition.H8));

                //find bishops
                Console.WriteLine("private static readonly ulong[] _bishopMagics = new ulong[]{");
                foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                {
                    //var mask = BishopMask(p);
                    //var maskCount = mask.BitCount();
                    var magic = find_magic(p, true);
                    Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                }
                Console.WriteLine("};");

                    
                //find rooks
                Console.WriteLine("\n\nprivate static readonly ulong[] _rookMagics = new ulong[]{");
                foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                {
                    //var mask = RookMask(p);
                    //var maskCount = mask.BitCount();
                    var magic = find_magic(p, false);
                    Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                }
                Console.WriteLine("};");
                


            }


        }
    }
}
