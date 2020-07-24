using System;
using System.Collections.Generic;
using System.IO;
using Cube_Solver.Cubes;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    class Search
    {
        private const int NUM_COS = 2187;   // 3^7 cases
        public const int NUM_ESLICE = 495; // 12C4 cases
        private const int NUM_EOS = 1013760; // 2^11 * 12C4 cases
        public const int NUM_CPS = 40320;  // 8! cases
        private const int NUM_EPS = 967680; // 8! * 4! cases

        private const string coPath = "res/corner_ori";
        private const string eoPath = "res/edge_ori";
        private const string cpPath = "res/corner_perm";
        private const string epPath = "res/edge_perm";

        private PruningTable coTable, eoTable, cpTable, epTable;

        private List<(Face, Dir)> phase1moves = new List<(Face, Dir)>();
        private List<(Face, Dir)> phase2moves = new List<(Face, Dir)>();

        private IdCalculator idCalc;

        #region Generating pruning tables
        public Search(CubieCube solved)
        {
            idCalc = new IdCalculator(solved);

            // All moves are valid for phase 1
            foreach(Face f in Enum.GetValues(typeof(Face)))
            {
                foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                    phase1moves.Add((f, dir));
            }

            // Initialise phase 1 pruning tables
            InitTable(out coTable, solved, NUM_COS, idCalc.GetCO, phase1moves, coPath, "corner orientation");
            InitTable(out eoTable, solved, NUM_EOS, cc => idCalc.GetEO(cc) * NUM_ESLICE + idCalc.GetEslice(cc), phase1moves, eoPath, "edge orientation");

            // Phase 2 <U, D, F2, B2, R2, L2>
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                if (f == Face.U || f == Face.D)
                {
                    foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                        phase2moves.Add((f, dir));
                }
                else
                    phase2moves.Add((f, Dir.HALF));
            }

            // Initialise phase 2 pruning tables
            InitTable(out cpTable, solved, NUM_CPS, idCalc.GetCP, phase2moves, cpPath, "corner permutation");
            InitTable(out epTable, solved, NUM_EPS, idCalc.GetEP, phase2moves, epPath, "edge permutation");
        }


        private void InitTable(out PruningTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Cube.Face, Cube.Dir)> applicableMoves, string filepath, string description)
        {
            if (File.Exists(filepath))
                table = new PruningTable(filepath);
            else
            {
                Console.WriteLine($"Generating {description} tables...");
                table = new PruningTable(solved, size, GetID, applicableMoves, filepath);
                Console.WriteLine("done\n");
            }
        }
        #endregion

        #region Kociemba
        private int maxDepth;
        private Stack<(CubieCube, int)> path;

        public void Solve(CubieCube cube)
        {
            path = new Stack<(CubieCube, int)>();
            path.Push((cube, -1));
            maxDepth = int.MaxValue;

            for (int i = 0; i <= maxDepth; i++)
                IDAStar(i, cc => Math.Max(coTable[idCalc.GetCO(cc)], eoTable[idCalc.GetEO(cc) * NUM_ESLICE + idCalc.GetEslice(cc)]), BeginPhase2, phase1moves);
        }

        private void IDAStar(int depth, Func<CubieCube, int> Heur, Action EndFunc, List<(Cube.Face, Cube.Dir)> applicableMoves)
        {
            if (depth < 0)
                return;

            CubieCube curr = path.Peek().Item1;
            if (Heur(curr) == 0)
                EndFunc();
            else if (Heur(curr) <= depth)
            {
                int prev = (path.Peek().Item2 / 3);
                foreach (var move in applicableMoves)
                {
                    if (prev == -1 || ValidMove((Face)prev, move.Item1))
                    {
                        path.Push(((CubieCube)curr.ApplyMove(move.Item1, move.Item2), (int)move.Item1 * 3 + (int)move.Item2));
                        IDAStar(depth - 1, Heur, EndFunc, applicableMoves);
                        path.Pop();
                    }
                }
            }
        }

        private void BeginPhase2()
        {
            int m = path.Peek().Item2;
            int len = path.Count;
            if (!phase2moves.Contains(((Face)(m / 3), (Dir)(m % 3))))
            {
                for (int i = 0; i <= maxDepth - len; i++)
                    IDAStar(i, cc => Math.Max(cpTable[idCalc.GetCP(cc)], epTable[idCalc.GetEP(cc)]), EndPhase2, phase2moves);
            }
        }

        private void EndPhase2()
        {
            // Print solution
            var temp = new Stack<(CubieCube, int)>(path);
            temp.Pop();
            int len = temp.Count;
            while (temp.Count > 0)
            {
                int m = temp.Pop().Item2;
                Console.Write(Cube.FACE_CHARS[m / 3] + new string[] { " ", "2 ", "' " }[m % 3]);
            }
            Console.WriteLine($"({len})");
            // Update maxDepth
            maxDepth = len;
        }

        private Dictionary<Face, Face> movePairs = new Dictionary<Face, Face>
        {
            { Face.B, Face.F },
            { Face.R, Face.L },
            { Face.U, Face.D }
        };
        private bool ValidMove(Face prev, Face curr)
        {
            if (prev == curr)
                return false;

            return (!movePairs.ContainsKey(prev) || movePairs[prev] != curr);
        }
        #endregion
    }
}