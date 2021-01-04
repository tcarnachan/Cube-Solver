using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Cube_Solver.Cubes;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    public class Search
    {
        private SearchTables st;

        private CancellationToken ct;
        public static ConcurrentDictionary<string, byte> solutions;

        public Search(SearchTables st, CancellationToken ct)
        {
            this.st = st;
            this.ct = ct;
        }

        private int maxDepth;
        private Stack<(CubieCube, int)> path1;
        private Stack<(int cp, int ep, int)> path2;

        public void Solve(string state)
        {
            CubieCube cube = new CubieCube(new FaceletCube(state));
            path1 = new Stack<(CubieCube, int)>();
            path1.Push((cube, -1));
            maxDepth = int.MaxValue;
            for (int i = 0; i < maxDepth; i++)
                Phase1(i);
        }

        private void Phase1(int depth)
        {
            if (ct.IsCancellationRequested && solutions.Keys.Count > 0) return;

            CubieCube curr = path1.Peek().Item1;
            int heur = Math.Max(st.coTable[st.idCalc.GetCO(curr)], st.eoTable[st.idCalc.GetEO(curr) * SearchTables.NUM_ESLICE + st.idCalc.GetEslice(curr)]);
            if (depth == 0)
            {
                int prevMove = path1.Peek().Item2;
                Face f = (Face)(prevMove / 3);
                Dir d = (Dir)(prevMove % 3);
                if (heur == 0 && (prevMove == -1 || !st.phase2moves.Contains((f, d))))
                    BeginPhase2();
            }
            else if (heur <= depth)
            {
                int prev = path1.Peek().Item2;
                foreach (var move in st.phase1moves)
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
            path2.Push((st.idCalc.GetCP(curr.Item1), st.idCalc.GetEP(curr.Item1), curr.Item2));
            for (int i = 0; i < maxDepth - len - 1; i++)
                Phase2(i);
        }

        private void Phase2(int depth)
        {
            if (ct.IsCancellationRequested && solutions.Keys.Count > 0) return;

            var curr = path2.Peek();
            int heur = Math.Max(st.cpTable[curr.cp], st.epTable[curr.ep]);
            if (depth == 0)
            {
                if (heur == 0)
                    EndPhase2();
            }
            else if (heur <= depth)
            {
                int prev = path2.Peek().Item3;
                foreach (var move in st.phase2moves)
                {
                    if (prev == -1 || ValidMove((Face)(prev / 3), move.Item1))
                    {
                        (int cp, int ep) next = (curr.cp, curr.ep);
                        if (move.Item1 == Face.D || move.Item1 == Face.U)
                        {
                            for (int i = 0; i <= (int)move.Item2; i++)
                                next = (st.mtCP[next.cp, move.Item1], st.mtEP[next.ep, move.Item1]);
                        }
                        else
                            next = (st.mtCP[next.cp, move.Item1], st.mtEP[next.ep, move.Item1]);
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
            st.Output($"{s}({len})");
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
    }
}
