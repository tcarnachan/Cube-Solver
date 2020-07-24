using Cube_Solver.Cubes;
using System.Collections.Generic;

namespace Cube_Solver.Solver
{
    class IdCalculator
    {
        private int[] onesLookup = new int[Search.NUM_CPS];
        private Dictionary<int, int> eslice2ix = new Dictionary<int, int>();
        private int[] factorials;

        public IdCalculator(CubieCube solved)
        {
            for (int i = 0; i < Search.NUM_CPS; i++)
                onesLookup[i] = BitCount(i);
            for (int i = 0, j = 0; i < Search.NUM_ESLICE; i++, j++)
            {
                int bits = onesLookup[j];
                while (bits != 3 && bits != 4)
                    bits = onesLookup[++j];
                eslice2ix[j] = i;
            }
            factorials = new int[solved.ep.Length];
            factorials[0] = 1;
            for (int fact = 1, i = 1; i < solved.ep.Length; ++i)
            {
                fact *= i;
                factorials[i] = fact;
            }
        }

        private static int BitCount(int n)
        {
            int count = 0;
            while (n > 0)
            {
                n &= (n - 1);
                count++;
            }
            return count;
        }

        public int GetCO(CubieCube cc)
        {
            int id = 0;
            for (int i = 0; i < cc.co.Length - 1; i++)
            {
                id *= 3;
                id += cc.co[i];
            }
            return id;
        }

        public int GetEO(CubieCube cc)
        {
            int id = 0;
            for (int i = 0; i < cc.eo.Length - 1; i++)
            {
                id <<= 1;
                id += cc.eo[i];
            }
            return id;
        }

        public int GetEslice(CubieCube cc)
        {
            int esp = 0;
            for (int i = 0; i < cc.ep.Length - 1; ++i)
            {
                if ((int)Cube.Edge.BL <= cc.ep[i] && cc.ep[i] <= (int)Cube.Edge.FL)
                    esp |= (1 << i);
            }
            return eslice2ix[esp];
        }

        public int GetCP(CubieCube cc)
        {
            int id = 0, seen = 0;
            for (int i = 0; i < cc.cp.Length - 1; ++i)
            {
                seen |= 1 << (cc.cp.Length - 1 - cc.cp[i]);
                int less = onesLookup[seen >> (cc.cp.Length - cc.cp[i])];
                id += (cc.cp[i] - less) * factorials[cc.cp.Length - 1 - i];
            }
            return id;
        }

        public int GetEP(CubieCube cc)
        {
            int id = 0, seen = 0;
            for (int i = 0; i < cc.cp.Length - 1; ++i)
            {
                seen |= 1 << (cc.cp.Length - 1 - cc.ep[i]);
                int less = onesLookup[seen >> (cc.cp.Length - cc.ep[i])];
                id += (cc.ep[i] - less) * factorials[cc.cp.Length - 1 - i];
            }
            id *= 24;
            seen = 0;
            int BL = (int)Cube.Edge.BL;
            for (int i = 0; i < 3; ++i)
            {
                seen |= 1 << (3 - cc.ep[i + BL] + BL);
                int less = onesLookup[seen >> (4 - cc.ep[i + BL] + BL)];
                id += (cc.ep[i + BL] - BL - less) * factorials[3 - i];
            }
            return id;
        }
    }
}
