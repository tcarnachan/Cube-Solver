using System;
using System.Collections.Generic;
using System.IO;
using Cube_Solver.Cubes;

namespace Cube_Solver.Solvers
{
    class Solver
    {
        private const int NUM_COS = 2187;   // 3^7 cases
        private const int NUM_ESLICE = 495; // 12C4 cases
        private const int NUM_EOS = 1013760; // 2^11 * 12C4 cases
        private const int NUM_CPS = 40320;  // 8! cases
        private const int NUM_EPS = 967680; // 8! * 4! cases

        private const string coPath = "res/corner_ori";
        private const string eoPath = "res/edge_ori";
        private const string cpPath = "res/corner_perm";
        private const string epPath = "res/edge_perm";

        private PruningTable coTable, eoTable, cpTable, epTable;
        private Dictionary<int, int> eslice2ix = new Dictionary<int, int>();

        public Solver(CubieCube solved)
        {
            // Lookup table for calculating ids
            for (int i = 0, j = 0; i < NUM_ESLICE; i++, j++)
            {
                int bits = BitCount(j);
                while (bits != 3 && bits != 4)
                    bits = BitCount(++j);
                eslice2ix[j] = i;
            }

            // All moves are valid for phase 1
            List<(Cube.Face, Cube.Dir)> phase1moves = new List<(Cube.Face, Cube.Dir)>();
            foreach(Cube.Face f in Enum.GetValues(typeof(Cube.Face)))
            {
                foreach (Cube.Dir dir in Enum.GetValues(typeof(Cube.Dir)))
                    phase1moves.Add((f, dir));
            }

            if (File.Exists(coPath))
                coTable = new PruningTable(coPath);
            else
            {
                Console.WriteLine("Generating corner orientation tables...");
                coTable = new PruningTable(solved, NUM_COS, GetCO, phase1moves, coPath);
                Console.WriteLine("done\n");
            }

            if (File.Exists(eoPath))
                eoTable = new PruningTable(eoPath);
            else
            {
                Console.WriteLine("Generating edge orientation tables...");
                eoTable = new PruningTable(solved, NUM_EOS, cc => GetEO(cc) * NUM_ESLICE + GetEslice(cc), phase1moves, eoPath);
                Console.WriteLine("done\n");
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

        private static int GetCO(CubieCube cc)
        {
            int id = 0;
            for(int i = 0; i < cc.co.Length - 1; i++)
            {
                id *= 3;
                id += cc.co[i];
            }
            return id;
        }

        private static int GetEO(CubieCube cc)
        {
            int id = 0;
            for (int i = 0; i < cc.eo.Length - 1; i++)
            {
                id <<= 1;
                id += cc.eo[i];
            }
            return id;
        }

        private int GetEslice(CubieCube cc)
        {
            int esp = 0;
            for (int i = 0; i < cc.ep.Length - 1; ++i)
            {
                if ((int)Cube.Edge.BL <= cc.ep[i] && cc.ep[i] <= (int)Cube.Edge.FL)
                    esp |= (1 << i);
            }
            return eslice2ix[esp];
        }

        private static int GetCP(CubieCube cc)
        {
            return -1;
        }

        private static int GetEP(CubieCube cc)
        {
            return -1;
        }

        public int[] Solve() { throw new NotImplementedException(); }
    }
}