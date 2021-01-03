using System;
using System.Collections.Generic;
using System.IO;
using Cube_Solver.Cubes;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    public class Search
    {
        private const int NUM_COS = 2187;   // 3^7 cases
        public const int NUM_ESLICE = 495; // 12C4 cases
        private const int NUM_EOS = 1013760; // 2^11 * 12C4 cases
        public const int NUM_CPS = 40320;  // 8! cases
        private const int NUM_EPS = 967680; // 8! * 4! cases

        public const string coPath = "corner_ori";
        public const string eoPath = "edge_ori";
        public const string cpPath = "corner_perm";
        public const string epPath = "edge_perm";
        public const string mtCPPath = "move_cp";
        public const string mtEPPath = "move_ep";

        private PruningTable coTable, eoTable, cpTable, epTable;
        private MoveTable mtCP, mtEP;

        private List<(Face, Dir)> phase1moves = new List<(Face, Dir)>();
        private List<(Face, Dir)> phase2moves = new List<(Face, Dir)>();

        private IdCalculator idCalc;
        private string resourcePath;
        private Action<string> Output;

        #region Generating pruning tables
        public Search(string solvedState, string resourcePath, Action<string> Output)
        {
            this.resourcePath = resourcePath;
            this.Output = Output;

            CubieCube solved = new CubieCube(new FaceletCube(solvedState));
            idCalc = new IdCalculator(solved);

            // All moves are valid for phase 1
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                    phase1moves.Add((f, dir));
            }

            // Initialise phase 1 pruning tables
            InitPruningTable(out coTable, solved, NUM_COS, idCalc.GetCO, phase1moves, coPath, "corner orientation");
            InitPruningTable(out eoTable, solved, NUM_EOS, cc => idCalc.GetEO(cc) * NUM_ESLICE + idCalc.GetEslice(cc), phase1moves, eoPath, "edge orientation");

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
            InitPruningTable(out cpTable, solved, NUM_CPS, idCalc.GetCP, phase2moves, cpPath, "corner permutation");
            InitPruningTable(out epTable, solved, NUM_EPS, idCalc.GetEP, phase2moves, epPath, "edge permutation");

            // Initialise phase 2 move tables
            //mtCP = new MoveTable(mtCPPath);
            //mtEP = new MoveTable(mtEPPath);
            InitMoveTable(out mtCP, solved, NUM_CPS, idCalc.GetCP, mtCPPath, "corner permutation");
            InitMoveTable(out mtEP, solved, NUM_EPS, idCalc.GetEP, mtEPPath, "edge permutation");
        }


        private void InitPruningTable(out PruningTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Cube.Face, Cube.Dir)> applicableMoves, string filename, string description)
        {
            string filepath = resourcePath + filename;
            if (File.Exists(filepath))
                table = new PruningTable(filepath);
            else
            {
                Output($"Generating {description} pruning tables...");
                table = new PruningTable(solved, size, GetID, applicableMoves, filepath);
                Output("done\n");
            }
        }

        private void InitMoveTable(out MoveTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, string filename, string description)
        {
            string filepath = resourcePath + filename;
            if (File.Exists(filepath))
                table = new MoveTable(filepath);
            else
            {
                Output($"Generating {description} move tables...");
                table = new MoveTable(solved, size, GetID, filepath);
                Output("done\n");
            }
        }
        #endregion

        #region Kociemba
        private int maxDepth;
        private Stack<(CubieCube, int)> path1;
        private Stack<(int cp, int ep, int)> path2;
        public bool exit;

        public void Solve(string state)
        {
            CubieCube cube = new CubieCube(new FaceletCube(state));
            exit = false;
            path1 = new Stack<(CubieCube, int)>();
            path1.Push((cube, -1));
            maxDepth = int.MaxValue;
            for (int i = 0; i < maxDepth; i++)
                Phase1(i);
        }

        private void Phase1(int depth)
        {
            if (exit) return;

            CubieCube curr = path1.Peek().Item1;
            int heur = Math.Max(coTable[idCalc.GetCO(curr)], eoTable[idCalc.GetEO(curr) * NUM_ESLICE + idCalc.GetEslice(curr)]);
            if (depth == 0)
            {
                int prevMove = path1.Peek().Item2;
                Face f = (Face)(prevMove / 3);
                Dir d = (Dir)(prevMove % 3);
                if (heur == 0 && (prevMove == -1 || !phase2moves.Contains((f, d))))
                    BeginPhase2();
            }
            else if (heur <= depth)
            {
                int prev = path1.Peek().Item2;
                foreach (var move in phase1moves)
                {
                    if (prev == -1 || ValidMove((Face)(prev / 3), move.Item1))
                    {
                        path1.Push(((CubieCube)curr.ApplyMove(move.Item1, move.Item2), (int)move.Item1 * 3 + (int)move.Item2));
                        Phase1(depth - 1);
                        path1.Pop();
                    }
                }
            }
        }

        private void BeginPhase2()
        {
            int len = path1.Count;
            var curr = path1.Peek();
            path2 = new Stack<(int cp, int ep, int)>();
            path2.Push((idCalc.GetCP(curr.Item1), idCalc.GetEP(curr.Item1), curr.Item2));
            for (int i = 0; i < maxDepth - len - 1; i++)
                Phase2(i);
        }

        private void Phase2(int depth)
        {
            if (exit) return;

            var curr = path2.Peek();
            int heur = Math.Max(cpTable[curr.cp], epTable[curr.ep]);
            if (depth == 0)
            {
                if (heur == 0)
                    EndPhase2();
            }
            else if (heur <= depth)
            {
                int prev = path2.Peek().Item3;
                foreach (var move in phase2moves)
                {
                    if (prev == -1 || ValidMove((Face)(prev / 3), move.Item1))
                    {
                        (int cp, int ep) next = (curr.cp, curr.ep);
                        if (move.Item1 == Face.D || move.Item1 == Face.U)
                        {
                            for (int i = 0; i <= (int)move.Item2; i++)
                                next = (mtCP[next.cp, move.Item1], mtEP[next.ep, move.Item1]);
                        }
                        else
                            next = (mtCP[next.cp, move.Item1], mtEP[next.ep, move.Item1]);
                        path2.Push((next.cp, next.ep, (int)move.Item1 * 3 + (int)move.Item2));
                        Phase2(depth - 1);
                        path2.Pop();
                    }
                }
            }
        }

        private void EndPhase2()
        {
            // Print phase 1
            var temp1 = new Stack<(CubieCube, int)>(path1);
            temp1.Pop();
            int len = temp1.Count;
            string s = "";
            while (temp1.Count > 0)
            {
                int m = temp1.Pop().Item2;
                s += Cube.FACE_CHARS[m / 3] + new string[] { " ", "2 ", "' " }[m % 3];
            }

            // Print phase 2
            var temp2 = new Stack<(int, int, int)>(path2);
            temp2.Pop();
            len += temp2.Count;
            while (temp2.Count > 0)
            {
                int m = temp2.Pop().Item3;
                s += Cube.FACE_CHARS[m / 3] + new string[] { " ", "2 ", "' " }[m % 3];
            }
            Output($"{s}({len})");
            // Update maxDepth
            maxDepth = len - 1;
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

            return !(movePairs.ContainsKey(prev) && movePairs[prev] == curr);
        }
        #endregion

        public string RandomCube()
        {
            CubieCube cc = CubieCube.RandomCube();
            FaceletCube fc = new FaceletCube(cc);
            return fc.ToString();
        }
    }
}
