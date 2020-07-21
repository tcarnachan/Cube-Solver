using System;
using System.Linq;

namespace Cube_Solver.Cubes
{
    /// <summary>
    /// Represents a cube on the cubie level.
    /// Each cubie has an orientation and a permutation.
    /// Cubies are split into corners and edges as they cannot change places.
    /// </summary>
    public class CubieCube : Cube
    {
        /// <summary>
        /// Corner orientations, 0 = oriented, 1 = twisted clockwise, 2 = twisted counter-clockwise.
        /// </summary>
        public int[] co = new int[NUM_CORNERS];
        /// <summary>
        /// Corner permutations, stores numbers from 0-7 representing each corner.
        /// </summary>
        public int[] cp = new int[NUM_CORNERS];
        /// <summary>
        /// Edge orientations, 0 = oriented, 1 = mis-oriented.
        /// </summary>
        public int[] eo = new int[NUM_EDGES];
        /// <summary>
        /// Edge permutations, stores numbers from 0-11 representing each corner.
        /// </summary>
        public int[] ep = new int[NUM_EDGES];

        /// <summary>
        /// Constructs a CubieCube from a cube at the facelet level.
        /// </summary>
        /// <param name="fc">The cube at the facelet level.</param>
        public CubieCube(FaceletCube fc)
        {
            // For each corner position
            for (int i = 0; i < NUM_CORNERS; i++)
            {
                Face[] c = new Face[]
                {
                    GetFacelet(fc, cornerPieces[i, 0]),
                    GetFacelet(fc, cornerPieces[i, 1]),
                    GetFacelet(fc, cornerPieces[i, 2])
                };
                // For every corner cubie
                int ori = -1, j;
                for(j = 0; j < NUM_CORNERS && ori == -1; j++)
                {
                    Face[] f = new Face[]
                    {
                        cornerPieces[j, 0].f,
                        cornerPieces[j, 1].f,
                        cornerPieces[j, 2].f
                    };
                    // Check if it is oriented
                    if (c[0] == f[0] && c[1] == f[1] && c[2] == f[2])
                        ori = 0;
                    // Check if it is twisted clockwise
                    else if (c[0] == f[2] && c[1] == f[0] && c[2] == f[1])
                        ori = 1;
                    // Check if it is twisted counter-clockwise
                    else if (c[0] == f[1] && c[1] == f[2] && c[2] == f[0])
                        ori = 2;
                }
                if (ori == -1)
                    throw new Exception($"Corner {c[0]}{c[1]}{c[2]} not found");

                // At corner position i is corner cubie j
                cp[i] = (j - 1);
                co[i] = (ori);
            }

            // For each edge position
            for (int i = 0; i < NUM_EDGES; i++)
            {
                Face[] e = new Face[]
                {
                    GetFacelet(fc, edgePieces[i, 0]),
                    GetFacelet(fc, edgePieces[i, 1]),
                };
                // For every corner cubie
                int ori = -1, j;
                for (j = 0; j < NUM_EDGES && ori == -1; j++)
                {
                    Face[] f = new Face[]
                    {
                        edgePieces[j, 0].f,
                        edgePieces[j, 1].f,
                    };
                    // Check if it is oriented
                    if (e[0] == f[0] && e[1] == f[1])
                        ori = 0;
                    // Check if it is flipped
                    else if (e[0] == f[1] && e[1] == f[0])
                        ori = 1;
                }
                if (ori == -1)
                    throw new Exception($"Edge {e[0]}{e[1]} not found");

                // At corner position i is corner cubie j
                ep[i] = (j - 1);
                eo[i] = (ori);
            }

            string err = Verify();
            if (err != VALID_STATE)
                throw new Exception(err);
        }

        private static Face GetFacelet(FaceletCube fc, (Face f, int r, int c) index)
        {
            return fc.faces[(int)index.f][index.r, index.c];
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="cc">The CubieCube to copy.</param>
        public CubieCube(CubieCube cc)
        {
            Array.Copy(cc.cp, cp, NUM_CORNERS);
            Array.Copy(cc.co, co, NUM_CORNERS);
            Array.Copy(cc.ep, ep, NUM_EDGES);
            Array.Copy(cc.eo, eo, NUM_EDGES);
        }

        public override Cube ApplyMove(Face f, Dir dir)
        {
            Cube nc = new FaceletCube(this).ApplyMove(f, dir);
            return new CubieCube((FaceletCube)nc);
        }

        // Prints the permutations and orientations of each piece (corners and then edges)
        public override void Print()
        {
            Console.Write("----------\nCorners:\np:");
            for (int i = 0; i < NUM_CORNERS; ++i)
                Console.Write($" {cp[i]}");
            Console.Write("\no:");
            for (int i = 0; i < NUM_CORNERS; ++i)
                Console.Write($" {co[i]}");
            Console.Write("\nEdges:\np:");
            for (int i = 0; i < NUM_EDGES; ++i)
                Console.Write($" {ep[i]:00}");
            Console.Write("\no:");
            for (int i = 0; i < NUM_EDGES; ++i)
                Console.Write($" {eo[i]:00}");
            Console.WriteLine();
        }

        /// <summary>
        /// Checks number of each piece, orientations of the pieces and the parity of the cube to ensure it is a possible state.
        /// </summary>
        /// <returns>Whether or not this is in a valid state.</returns>
        protected override string Verify()
        {
            // Check that each piece exists
            for(int i = 0; i < NUM_CORNERS; i++)
            {
                if(!cp.Contains(i))
                    return "Missing corner piece";
            }
            for (int i = 0; i < NUM_EDGES; i++)
            {
                if (!ep.Contains(i))
                    return "Missing edge piece";
            }
            // Check orientations
            if (co.Sum() % 3 != 0)
                return "Invalid corner orientation";
            if (eo.Sum() % 2 != 0)
                return "Invalid edge orientation";
            if ((GetParity(cp) ^ GetParity(ep)) != 0)
                return "Invalid parity";
            return VALID_STATE;
        }

        private int GetParity(int[] data)
        {
            int parity = 0;
            for(int i = 0; i < data.Length; i++)
            {
                for(int j = 0; j < i; j++)
                {
                    if (data[j] > data[i]) parity++;
                }
            }
            return parity % 2;
        }

        public override bool IsSolved()
        {
            // Check orientations
            if (co.Any(i => i != 0) || eo.Any(i => i != 0))
                return false;
            // Check permutations
            if (!cp.SequenceEqual(Enumerable.Range(0, NUM_CORNERS)) || !ep.SequenceEqual(Enumerable.Range(0, NUM_EDGES)))
                return false;
            return true;
        }
    }
}

