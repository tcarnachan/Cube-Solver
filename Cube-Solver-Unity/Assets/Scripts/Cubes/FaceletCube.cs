using System;
using System.Linq;
using System.Collections.Generic;

namespace Cube_Solver.Cubes
{
    /// <summary>
    /// Represents a cube on the facelet level.
    /// Each facelet on the cube is stored in a char[][][] which is accessed by [FACE][Row][Col].
    /// </summary>
    class FaceletCube : Cube
    {
        public Face[][,] faces { get; private set; }

        /// <summary>
        /// Constructs a facelet cube from a string representation.
        /// </summary>
        /// <param name="state">The state of the facelet cube.</param>
        public FaceletCube(string state)
        {
            if(state.Length < DIM_SQR)
                throw new Exception("Incorrect number of total facelets");
            
            Dictionary<char, Face> replacements = new Dictionary<char, Face>();
            for (int i = 0; i < NUM_FACES; i++)
                replacements[state[(i * DIM_SQR) + (DIM_SQR / 2)]] = (Face)i;

            faces = new Face[NUM_FACES][,];
            for (int i = 0; i < NUM_FACES; i++)
            {
                string face = state.Substring(i * DIM_SQR, DIM_SQR);
                Face[,] f = new Face[DIM, DIM];
                for (int ix = 0; ix < DIM_SQR; ix++)
                    f[ix / DIM, ix % DIM] = replacements[face[ix]];
                faces[i] = f;
            }

            string err = Verify();
            if (err != VALID_STATE)
                throw new Exception(err);
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="fc">The FaceletCube to copy.</param>
        public FaceletCube(FaceletCube fc)
        {
            faces = new Face[NUM_FACES][,];
            for (int i = 0; i < NUM_FACES; i++)
            {
                faces[i] = new Face[DIM, DIM];
                Array.Copy(fc.faces[i], faces[i], DIM_SQR);
            }
        }

        /// <summary>
        /// Constructs a facelet cube from a cube at the cubie level.
        /// </summary>
        /// <param name="cc">The cube at the cubie level.</param>
        public FaceletCube(CubieCube cc)
        {
            faces = new Face[NUM_FACES][,];
            for (int i = 0; i < NUM_FACES; i++)
            {
                faces[i] = new Face[DIM, DIM];
                faces[i][DIM / 2, DIM / 2] = (Face)i;
            }
            // Set corners
            for(int i = 0; i < NUM_CORNERS; i++)
            {
                int ori = cc.co[i];
                for (int j = 0; j < 3; j++)
                {
                    var pos = cornerPieces[i, j];
                    faces[(int)pos.f][pos.r, pos.c] = cornerPieces[cc.cp[i], (j + 3 - ori) % 3].f;
                }
            }
            // Set edges
            for (int i = 0; i < NUM_EDGES; i++)
            {
                int ori = cc.eo[i];
                for (int j = 0; j < 2; j++)
                {
                    var pos = edgePieces[i, j];
                    faces[(int)pos.f][pos.r, pos.c] = edgePieces[cc.ep[i], (j + ori) % 2].f;
                }
            }
        }

        /// <summary>
        /// Checks the number of each facelet to ensure it is a possible state.
        /// </summary>
        /// <returns>Whether or not this is a valid state.</returns>
        protected override string Verify()
        {
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                int count = 0;
                foreach (Face[,] face in faces)
                    count += Enumerable.Range(0, DIM_SQR).Count(i => face[i / 3, i % 3] == f);
                if (count != DIM_SQR) return "Incorrect number of individual facelets";
            }
            return VALID_STATE;
        }

        public override bool IsSolved()
        {
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                Face[,] face = faces[(int)f];
                if (Enumerable.Range(0, DIM_SQR).Any(i => face[i / 3, i % 3] != f))
                    return false;
            }
            return true;
        }

        public override void Print()
        {
            // Print U-face
            Face[,] f = faces[0];
            for (int i = 0; i < DIM; i++)
            {
                Console.SetCursorPosition(2 * DIM + 2, i);
                for (int j = 0; j < DIM; j++)
                    Console.Write($"{f[i, j]} ");
            }
            // Print LFRB-faces
            for (int i = 1; i < NUM_FACES - 1; i++)
            {
                f = faces[i];
                for (int j = 0; j < DIM; j++)
                {
                    Console.SetCursorPosition(2 * (i - 1) * (DIM + 1), j + DIM + 1);
                    for (int k = 0; k < DIM; k++)
                        Console.Write($"{f[j, k]} ");
                }
            }
            // Print D-face
            f = faces[NUM_FACES - 1];
            for (int i = 0; i < DIM; i++)
            {
                Console.SetCursorPosition(2 * DIM + 2, i + DIM * 2 + 2);
                for (int j = 0; j < DIM; j++)
                    Console.Write($"{f[i, j]} ");
            }
            Console.WriteLine();
        }

        public override string ToString()
        {
            string s = "";
            for (int f = 0; f < NUM_FACES; f++)
            {
                for (int i = 0; i < DIM; i++)
                {
                    for (int j = 0; j < DIM; j++)
                        s += $"{faces[f][i, j]} ";
                }
            }
            return s;
        }

        public override Cube ApplyMove(Face f, Dir dir)
        {
            FaceletCube res = new FaceletCube(this);

            for (int _ = 0; _ <= (int)dir; _++)
                res = res.ApplyMove(f);

            return res;
        }

        /// <summary>
        /// Applies a clockwise rotation to one face of the cube.
        /// </summary>
        /// <param name="f">The face to rotate.</param>
        /// <returns>The result of rotating that face on this cube.</returns>
        private FaceletCube ApplyMove(Face face)
        {
            FaceletCube res = new FaceletCube(this);

            // Update this face
            int neg = DIM - 1;
            Face[,] oldFace = faces[(int)face];
            Face[,] newFace = res.faces[(int)face];
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                    newFace[i, j] = oldFace[neg - j, i];
            }

            // Update adjacent faces
            switch (face)
            {
                case Face.U:
                    for (int i = 0; i < DIM; i++)
                    {
                        for (int f = 1; f < 5; f++)
                            res.faces[f][0, i] = faces[f % 4 + 1][0, i];
                    }
                    break;
                case Face.D:
                    for (int i = 0; i < DIM; i++)
                    {
                        for (int f = 1; f < 5; f++)
                            res.faces[f % 4 + 1][neg, i] = faces[f][neg, i];
                    }
                    break;
                case Face.F:
                    for (int i = 0; i < DIM; i++)
                    {
                        res.faces[(int)Face.U][neg, i] = faces[(int)Face.L][neg - i, neg];
                        res.faces[(int)Face.L][i, neg] = faces[(int)Face.D][0, i];
                        res.faces[(int)Face.D][0, i] = faces[(int)Face.R][neg - i, 0];
                        res.faces[(int)Face.R][i, 0] = faces[(int)Face.U][neg, i];
                    }
                    break;
                case Face.B:
                    for (int i = 0; i < DIM; i++)
                    {
                        res.faces[(int)Face.U][0, i] = faces[(int)Face.R][i, neg];
                        res.faces[(int)Face.L][i, 0] = faces[(int)Face.U][0, neg - i];
                        res.faces[(int)Face.D][neg, i] = faces[(int)Face.L][i, 0];
                        res.faces[(int)Face.R][i, neg] = faces[(int)Face.D][neg, neg - i];
                    }
                    break;
                case Face.R:
                    for (int i = 0; i < DIM; i++)
                    {
                        res.faces[(int)Face.U][i, neg] = faces[(int)Face.F][i, neg];
                        res.faces[(int)Face.F][i, neg] = faces[(int)Face.D][i, neg];
                        res.faces[(int)Face.D][i, neg] = faces[(int)Face.B][neg - i, 0];
                        res.faces[(int)Face.B][i, 0] = faces[(int)Face.U][neg - i, neg];
                    }
                    break;
                case Face.L:
                    for (int i = 0; i < DIM; i++)
                    {
                        res.faces[(int)Face.U][i, 0] = faces[(int)Face.B][neg - i, neg];
                        res.faces[(int)Face.F][i, 0] = faces[(int)Face.U][i, 0];
                        res.faces[(int)Face.D][i, 0] = faces[(int)Face.F][i, 0];
                        res.faces[(int)Face.B][i, neg] = faces[(int)Face.D][neg - i, 0];
                    }
                    break;
            }

            return res;
        }
    }
}

