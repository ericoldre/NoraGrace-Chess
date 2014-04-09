using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinobyl.Engine
{
    public static class MagicBitboards
    {

        private static readonly ulong[] _bishopMagics = new ulong[]{
            27307558868091298UL, //A8
            4901051092795687938UL, //B8
            2260597525758084UL, //C8
            298367944260354048UL, //D8
            1130436483953712UL, //E8
            9572691933659168UL, //F8
            720647443532750976UL, //G8
            1152957789598924930UL, //H8
            4686070196322176032UL, //A7
            9009604507762820UL, //B7
            36381817636454944UL, //C7
            9369741500809743425UL, //D7
            142945374175748UL, //E7
            309203629307592721UL, //F7
            2326706325327880UL, //G7
            2305887540528287808UL, //H7
            18227875601978435UL, //A6
            1858016459914018880UL, //B6
            1125908565070336UL, //C6
            147813962438705152UL, //D6
            1298162601190117634UL, //E6
            140823656171521UL, //F6
            1143638290662432UL, //G6
            562985403729920UL, //H6
            9227893228773314561UL, //A5
            14989145478423117968UL, //B5
            2260630283354144UL, //C5
            11817586710135570562UL, //D5
            144961812096386048UL, //E5
            112732928337379844UL, //F5
            1127023587629312UL, //G5
            283678568218788UL, //H5
            146683793269917696UL, //A4
            4902177005638255106UL, //B4
            1156308309660205569UL, //C4
            18726057672768UL, //D4
            2451088512426971152UL, //E4
            4503883095281936UL, //F4
            9225632637074817344UL, //G4
            33783870562271744UL, //H4
            290491144001299457UL, //A3
            81628912685416960UL, //B3
            281578123067905UL, //C3
            9007611890386944UL, //D3
            5773615891065340928UL, //E3
            9224511886832173584UL, //F3
            5190399704451551368UL, //G3
            292479320458312UL, //H3
            9242516956723316224UL, //A2
            144186708963885056UL, //B2
            288230586741949442UL, //C2
            4611791717740445728UL, //D2
            9516115515538669696UL, //E2
            9223424951145531396UL, //F2
            577595457031766144UL, //G2
            2261285257740304UL, //H2
            284232614249472UL, //A1
            7061927898328089088UL, //B1
            52785156723004UL, //C1
            4621185809901815824UL, //D1
            1297036695234348032UL, //E1
            2307109664068731456UL, //F1
            1130334597481474UL, //G1
            9368059254512287776UL, //H1
        };

        public static void Test()
        {
            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {
                var mask = BishopMask(position);
                var magic = _bishopMagics[(int)position];
                var shift = 64 - mask.BitCount();

                foreach (var bitset in Combinations(mask))
                {
                    var genAttacks = BishopAttacks(position, bitset);

                    var index = ((ulong)bitset * magic) >> shift;

                    var attackLookup = _bishopLookup[(int)position][index];

                    if (attackLookup != genAttacks)
                    {
                        throw new Exception("bad magic");
                    }
                }
            }

        }

        private static ChessBitboard[][] _bishopLookup = new ChessBitboard[64][];
        static MagicBitboards()
        {
            foreach (ChessPosition position in ChessPositionInfo.AllPositions)
            {

                var mask = BishopMask(position);
                var possibleCombinations = 1 << mask.BitCount();
                var shift = 64 - mask.BitCount();
                var magic = _bishopMagics[(int)position];

                _bishopLookup[(int)position] = new ChessBitboard[possibleCombinations];

                foreach (var bitset in Combinations(mask))
                {
                    var attacks = BishopAttacks(position, bitset);
                    var index = (magic * (ulong)bitset) >> shift;
                    _bishopLookup[(int)position][index] = attacks;
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

        private const ChessBitboard EDGES = ChessBitboard.Rank1 | ChessBitboard.Rank8 | ChessBitboard.FileA | ChessBitboard.FileH;



        static ChessBitboard RookMask(ChessPosition position)
        {
            ChessBitboard attacks = RookAttacks(position, ChessBitboard.Empty);
            ChessBitboard retval = attacks & ~EDGES;
            return retval;
        }

        static ChessBitboard BishopMask(ChessPosition position)
        {
            ChessBitboard attacks = BishopAttacks(position, ChessBitboard.Empty);
            ChessBitboard retval = attacks & ~EDGES;
            return retval;
        }

        static ChessBitboard RookAttacks(ChessPosition position, ChessBitboard blockers)
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

        static ChessBitboard BishopAttacks(ChessPosition position, ChessBitboard blockers)
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
            private static readonly Random _rand = new Random(23456);
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

            static ulong Rand64Few()
            {
                return Rand64() & Rand64() & Rand64();
            }

            static ChessBitboard index_to_uulong(int index, int bits, ChessBitboard m)
            {
                int i, k;
                ulong result = 0UL;
                for (i = 0; i < bits; i++)
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


            private static int Transform(ulong b, ulong magic, int bits)
            {
                return (int)((b * magic) >> (64 - bits));
            }

            private static ulong find_magic(ChessPosition sq, int m, bool bishop)
            {
                ulong magic;
                ChessBitboard[] b = new ChessBitboard[4096];
                ChessBitboard[] a = new ChessBitboard[4096];
                ChessBitboard[] used = new ChessBitboard[4096];
                ChessBitboard mask;
                int i, j, k, n;
                mask = bishop ? BishopMask(sq) : RookMask(sq);
                n = mask.BitCount();

                for (i = 0; i < (1 << n); i++)
                {
                    b[i] = index_to_uulong(i, n, mask);
                    a[i] = bishop ? BishopAttacks(sq, b[i]) : RookAttacks(sq, b[i]);
                }
                for (k = 0; k < int.MaxValue; k++)
                {
                    magic = Rand64Few();

                    var mul = (ulong)mask * magic;
                    var mulmask = mul & 0xFF00000000000000UL;
                    var mulmaskBitCount = ((ChessBitboard)mulmask).BitCount();
                    if (mulmaskBitCount < 6) { continue; }

                    for (i = 0; i < 4096; i++) { used[i] = 0UL; }

                    bool fail = false;
                    for (i = 0; i < (1 << n); i++)
                    {
                        j = Transform((ulong)b[i], magic, m);
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
                    if (!fail) return magic;
                }

                return 0UL;
            }

            public static void FindMagics()
            {
                try
                {
                    //find bishops
                    Console.WriteLine("private static readonly ulong[] _bishopMagics = new ulong[]{");
                    foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                    {
                        var mask = BishopMask(p);
                        var maskCount = mask.BitCount();
                        var magic = find_magic(p, maskCount, true);
                        Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                    }
                    Console.WriteLine("};");


                    //find rooks
                    Console.WriteLine("\n\nprivate static readonly ulong[] _rookMagics = new ulong[]{");
                    foreach (ChessPosition p in ChessPositionInfo.AllPositions)
                    {
                        var mask = RookMask(p);
                        var maskCount = mask.BitCount();
                        var magic = find_magic(p, maskCount, false);
                        Console.WriteLine(string.Format("\t{0}UL, //{1}", magic, p));
                    }
                    Console.WriteLine("};");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


        }
    }
}
