using Cube_Solver.Cubes;
using System.Collections.Generic;

namespace Cube_Solver.Solver
{
    /// <summary>
    /// A class used for calculating ids for CubieCubes which are used for fast indexing into pruning/move tables
    /// </summary>
    public class IdCalculator
    {
        /// <summary>
        /// Array lookup for the number of 1s in the binary representation of the index
        /// </summary>
        private int[] onesLookup = new int[SearchTables.NUM_CPS];
        /// <summary>
        /// Dictionary converting a middle slice state into an id
        /// </summary>
        private Dictionary<int, int> eslice2ix = new Dictionary<int, int>();
        /// <summary>
        /// Array storing the factorials for fast lookup since calculating it repeatedly is O(n)
        /// </summary>
        private int[] factorials;
        /// <summary>
        /// The index of the first middle layer edge
        /// </summary>
        private const int BL = 8;

        public IdCalculator(CubieCube solved)
        {
            // Populate the 1s lookup table
            for (int i = 0; i < SearchTables.NUM_CPS; i++)
                onesLookup[i] = BitCount(i);
            // Populate eslice dictionary
            for (int i = 0, j = 0; i < SearchTables.NUM_ESLICE; i++, j++)
            {
                int bits = onesLookup[j];
                while (bits != 3 && bits != 4)
                    bits = onesLookup[++j];
                eslice2ix[j] = i;
            }
            // Populate the factorials array
            factorials = new int[solved.ep.Length];
            factorials[0] = 1;
            for (int fact = 1, i = 1; i < solved.ep.Length; ++i)
            {
                fact *= i;
                factorials[i] = fact;
            }
        }

        /// <summary>
        /// Returns the number of 1s in the binary representation of n
        /// </summary>
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

        /// <summary>
        /// Returns an id representing the corner orientations of the cube
        /// </summary>
        public int GetCO(CubieCube cc)
        {
            int id = 0;
            for (int i = 0; i < cc.co.Length - 1; i++)
            {
                // cc.co is treated as a ternary number and converted into a single integer
                id *= 3;
                id += cc.co[i];
            }
            return id;
        }

        /// <summary>
        /// Returns an id representing the edge orientations of the cube
        /// </summary>
        public int GetEO(CubieCube cc)
        {
            int id = 0;
            for (int i = 0; i < cc.eo.Length - 1; i++)
            {
                // cc.eo is treated as a binary number and converted into a single integer
                id <<= 1;
                id += cc.eo[i];
            }
            return id;
        }

        /// <summary>
        /// Returns an id representing the permutation of the middle (e) slice layer of the cube
        /// </summary>
        public int GetEslice(CubieCube cc)
        {
            int esp = 0;
            for (int i = 0; i < cc.ep.Length - 1; ++i)
            {
                if (BL <= cc.ep[i])
                    esp |= 1 << i;
            }
            return eslice2ix[esp];
        }

        /// <summary>
        /// Returns an id representing the corner permutations of the cube
        /// </summary>
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

        /// <summary>
        /// Returns an id representing the edge permutations of the cube
        /// </summary>
        public int GetEP(CubieCube cc)
        {
            // First calculate the id for the up and down (U/D) layers of the cube
            int id = 0, seen = 0;
            for (int i = 0; i < cc.cp.Length - 1; ++i)
            {
                seen |= 1 << (cc.cp.Length - 1 - cc.ep[i]);
                int less = onesLookup[seen >> (cc.cp.Length - cc.ep[i])];
                id += (cc.ep[i] - less) * factorials[cc.cp.Length - 1 - i];
            }
            id *= 24;
            seen = 0;
            // Then add the id for the middle layer of the cube
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
