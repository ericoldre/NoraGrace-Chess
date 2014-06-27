using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoraGrace.Engine
{
    public enum SearchDepth
    {
        PLY = 10
    }

    public static class SearchDepthInfo
    {
        
        public static SearchDepth SubstractPly(this SearchDepth depth, int ply)
        {
            return depth - (ply * (int)SearchDepth.PLY);
        }

        public static SearchDepth AddPly(this SearchDepth depth, int ply)
        {
            return depth + (ply * (int)SearchDepth.PLY);
        }

        public static int Value(this SearchDepth depth)
        {
            return (int)depth;
        }
        public static int ToPly(this SearchDepth depth)
        {
            return depth.Value() / SearchDepth.PLY.Value();
        }

        public static SearchDepth FromPly(int ply)
        {
            return (SearchDepth)(ply * (int)SearchDepth.PLY);
        }
    }
}
