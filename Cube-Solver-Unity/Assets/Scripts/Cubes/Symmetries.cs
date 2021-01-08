using System;
using System.Linq;
using System.Collections.Generic;

namespace Cube_Solver.Cubes
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;
    using Edge = Cube.Edge;
    using Corner = Cube.Corner;

    /// <summary>
    /// A class used to generate the symmetries of a cube
    /// </summary>
    public class Symmetries
    {
        /*
         * There are 48 possible symmetries of a cube, although not all of them are used in this program
         * All 48 symmetries can be generated from combinations of the 4 'basic symmetries' which are defined below
         */

        private static CubieCube[] symmetries;
        public const int N_SYMS = 48;

        public Symmetries()
        {
            // Initialise basic symmetries
            InitURFCube();
            InitReflectRL();
            InitFBCube();
            InitUDCube();

            // Generate all possible symmetries from the basic symmetries
            symmetries = new CubieCube[N_SYMS];
            CubieCube cc = CubieCube.SolvedCube();
            int ix = 0;
            for (int urf = 0; urf < 3; urf++)
            {
                for (int fb = 0; fb < 2; fb++)
                {
                    for (int ud = 0; ud < 4; ud++)
                    {
                        for (int lr = 0; lr < 2; lr++)
                        {
                            symmetries[ix] = new CubieCube(cc);
                            ix++;
                            cc = cc.Multiply(reflectRL);
                        }
                        cc = cc.Multiply(udSliceCube);
                    }
                    cc = cc.Multiply(fbSliceCube);
                }
                cc = cc.Multiply(urfCube);
            }
        }

        // Get the symmetries of the cube
        public HashSet<int> GetSymmetries(CubieCube cube)
        {
            // Get a list of symmetries and anti-symmetries for the cube
            HashSet<int> syms = new HashSet<int>();

            for (int i = 0; i < N_SYMS; i++)
            {
                CubieCube symCube = symmetries[i];
                CubieCube temp = symCube.InverseCube().Multiply(cube).Multiply(symCube);
                if (Equals(temp)) // Symmetry
                    syms.Add(i);
                if (cube.InverseCube().Equals(temp)) // Antisymmetry
                    syms.Add(i + N_SYMS);
            }

            return syms;
        }

        #region Basic Symmetries
        /*
         * The list of 'basic symmetries' which are used here:
             * URF cube - 120 degree rotation about the axis through the URF and DBL corners (b and g corners in the diagram below)
             * ReflectRL - Reflection about the RL-slice plane (parallel to the bdfh and aceg faces, perpendicular to the abef-face)
             * UD cube - 90 degree rotation about the axis going through the U and D centres (U-face is abef-face, D-face is cdgh-face)
             * FB cube - 180 degree rotation about the axis going through the F and B centres (F-face is abcd-face, B-face is efgh-face)
         *    e-------f
         *   /|      /|
         *  / |     / |
         * a--+----b  |
         * |  g----|--h
         * | /     | /
         * c-------d
         */

        // 120 degree rotation about the axis through the URF and DBL corners
        private Edge[][] urfEP = new Edge[][]
        {
            new Edge[] { Edge.UB, Edge.DR, Edge.FL },
            new Edge[] { Edge.UR, Edge.FR, Edge.UF },
            new Edge[] { Edge.UL, Edge.BR, Edge.DF },
            new Edge[] { Edge.DL, Edge.BL, Edge.DB }
        };

        private Corner[][] urfCP = new Corner[][]
        {
            new Corner[] { Corner.ULB, Corner.DRB, Corner.DLF },
            new Corner[] { Corner.UBR, Corner.DFR, Corner.UFL }
        };

        private Corner[] urfCO = new Corner[] { Corner.UBR, Corner.UFL, Corner.DFR, Corner.DBL };

        private static CubieCube urfCube;

        private void InitURFCube()
        {
            int[] ep = new int[Cube.NUM_EDGES], eo = new int[Cube.NUM_EDGES];
            int[] cp = Enumerable.Range(0, Cube.NUM_CORNERS).ToArray();
            int[] co = Enumerable.Repeat(1, Cube.NUM_CORNERS).ToArray();

            foreach (Edge[] cycle in urfEP)
            {
                for (int i = 0; i < cycle.Length; i++)
                    ep[(int)cycle[(i + 1) % cycle.Length]] = (int)cycle[i];
            }

            foreach (Corner[] cycle in urfCP)
            {
                for (int i = 0; i < cycle.Length; i++)
                    cp[(int)cycle[(i + 1) % cycle.Length]] = (int)cycle[i];
            }

            foreach (Edge e in Enum.GetValues(typeof(Edge)))
                eo[(int)e] = "RL".Contains(e.ToString()[1]) ? 1 : 0;

            foreach (Corner c in urfCO)
                co[(int)c]++;

            urfCube = new CubieCube(cp, co, ep, eo);
        }

        // Used for searching in parallel
        public CubieCube RotURF(CubieCube cube)
        {
            return urfCube.InverseCube().Multiply(cube).Multiply(urfCube);
        }

        // Reflection about the RL-slice plane
        private static CubieCube reflectRL;

        private void InitReflectRL()
        {
            int[] ep = Enumerable.Range(0, 12).ToArray(), eo = new int[Cube.NUM_EDGES];
            int[] cp = new int[Cube.NUM_CORNERS], co = Enumerable.Repeat(3, Cube.NUM_CORNERS).ToArray();

            foreach (Corner c in Enum.GetValues(typeof(Corner)))
            {
                string cStr = c.ToString();
                if (cStr.Contains('R'))
                {
                    if (cStr[1] == 'R') cStr = $"{cStr[0]}{cStr[2]}L";
                    else cStr = $"{cStr[0]}L{cStr[1]}";
                }
                else
                {
                    if (cStr[1] == 'L') cStr = $"{cStr[0]}{cStr[2]}R";
                    else cStr = $"{cStr[0]}R{cStr[1]}";
                }
                cp[(int)c] = (int)Enum.Parse(typeof(Corner), cStr);
            }

            foreach (Edge e in Enum.GetValues(typeof(Edge)))
            {
                string eStr = e.ToString();
                if ("RL".Contains(eStr[1]))
                {
                    if (eStr[1] == 'R') eStr = $"{eStr[0]}L";
                    else eStr = $"{eStr[0]}R";
                    ep[(int)e] = (int)Enum.Parse(typeof(Edge), eStr);
                }
            }

            reflectRL = new CubieCube(cp, co, ep, eo);
        }

        // 90 degree rotation about the axis going through the U and D centres
        private Edge[] udSliceRot = new Edge[] { Edge.FR, Edge.FL, Edge.BL, Edge.BR };

        private static CubieCube udSliceCube;

        public void InitUDCube()
        {
            udSliceCube = CubieCube.SolvedCube();
            // The slice layer cubies are rotated here
            for (int i = 0; i < udSliceRot.Length; i++)
            {
                udSliceCube.ep[(int)udSliceRot[(i + 1) % udSliceRot.Length]] = (int)udSliceRot[i];
                udSliceCube.eo[(int)udSliceRot[i]] = 1;
            } // The rotation is completed by turning the U and D layers
            udSliceCube = (CubieCube)udSliceCube.ApplyMove(Face.U, Dir.CW).ApplyMove(Face.D, Dir.CCW);
        }

        // 180 degree rotation about the axis going through the F and B centres
        private (Edge, Edge)[] fbSliceRot = new (Edge, Edge)[] { (Edge.UR, Edge.DL), (Edge.UL, Edge.DR) };

        private static CubieCube fbSliceCube, fbSliceInv;

        public void InitFBCube()
        {
            fbSliceCube = CubieCube.SolvedCube();
            foreach ((Edge a, Edge b) pair in fbSliceRot)
            {
                fbSliceCube.ep[(int)pair.a] = (int)pair.b;
                fbSliceCube.ep[(int)pair.b] = (int)pair.a;
            }
            fbSliceCube = (CubieCube)fbSliceCube.ApplyMove(Face.F, Dir.HALF).ApplyMove(Face.B, Dir.HALF);
        }
        #endregion
    }
}
