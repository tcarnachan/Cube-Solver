﻿using System;
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
        private int[] onesLookup = new int[NUM_CPS];
        private Dictionary<int, int> eslice2ix = new Dictionary<int, int>();
        private int[] factorials;

        public Solver(CubieCube solved)
        {
            // Lookup tables for calculating ids
            for (int i = 0; i < NUM_CPS; i++)
                onesLookup[i] = BitCount(i);
            for (int i = 0, j = 0; i < NUM_ESLICE; i++, j++)
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

            // All moves are valid for phase 1
            List<(Cube.Face, Cube.Dir)> phase1moves = new List<(Cube.Face, Cube.Dir)>();
            foreach(Cube.Face f in Enum.GetValues(typeof(Cube.Face)))
            {
                foreach (Cube.Dir dir in Enum.GetValues(typeof(Cube.Dir)))
                    phase1moves.Add((f, dir));
            }

            // Initialise phase 1 pruning tables
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

            // Phase 2 <U, D, F2, B2, R2, L2>
            List<(Cube.Face, Cube.Dir)> phase2moves = new List<(Cube.Face, Cube.Dir)>();
            foreach (Cube.Face f in Enum.GetValues(typeof(Cube.Face)))
            {
                if (f == Cube.Face.U || f == Cube.Face.D)
                {
                    foreach (Cube.Dir dir in Enum.GetValues(typeof(Cube.Dir)))
                        phase2moves.Add((f, dir));
                }
                else
                    phase2moves.Add((f, Cube.Dir.HALF));
            }

            // Initialise phase 2 pruning tables
            if (File.Exists(cpPath))
                cpTable = new PruningTable(cpPath);
            else
            {
                Console.WriteLine("Generating corner permutation tables...");
                cpTable = new PruningTable(solved, NUM_CPS, GetCP, phase2moves, cpPath);
                Console.WriteLine("done\n");
            }

            if (File.Exists(epPath))
                eoTable = new PruningTable(epPath);
            else
            {
                Console.WriteLine("Generating edge permutation tables...");
                epTable = new PruningTable(solved, NUM_EPS, GetEP, phase2moves, epPath);
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

        private int GetCP(CubieCube cc)
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

        private int GetEP(CubieCube cc)
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

        public int[] Solve() { throw new NotImplementedException(); }
    }
}