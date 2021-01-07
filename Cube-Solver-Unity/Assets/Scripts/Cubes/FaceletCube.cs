using System;
using System.Linq;
using System.Collections.Generic;

namespace Cube_Solver.Cubes
{
    /// <summary>
    /// Represents a cube on the facelet level.
    /// Each facelet on the cube is stored in a char[][][] which is accessed by [FACE][Row][Col].
    /// </summary>
    public class FaceletCube : Cube
    {
        public Face[][,] faces { get; private set; }

        /// <summary>
        /// Constructs a facelet cube from a string representation.
        /// </summary>
        /// <param name="state">The state of the facelet cube.</param>
        public FaceletCube(string state)
        {
            // Validate the state size
            if(state.Length < DIM_SQR)
                throw new Exception("Incorrect number of total facelets");
            
            // Map chars in the state to Faces
            Dictionary<char, Face> replacements = new Dictionary<char, Face>();
            for (int i = 0; i < NUM_FACES; i++)
                // Use the centre facelet of each face
                replacements[state[(i * DIM_SQR) + (DIM_SQR / 2)]] = (Face)i;

            // Initialise faces
            faces = new Face[NUM_FACES][,];
            for (int i = 0; i < NUM_FACES; i++)
            {
                // Get the substring for this specific face
                string face = state.Substring(i * DIM_SQR, DIM_SQR);
                // Use the replacements dictionary to convert to a Face[,]
                Face[,] f = new Face[DIM, DIM];
                for (int ix = 0; ix < DIM_SQR; ix++)
                    f[ix / DIM, ix % DIM] = replacements[face[ix]];
                faces[i] = f;
            }

            // Check that this is a valid state
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
            // Set centres
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
                    var pos = GetCorner(i)[j];
                    faces[pos.f][pos.r, pos.c] = (Face)GetCornerFaces(cc.cp[i])[(j + 3 - ori) % 3];
                }
            }
            // Set edges
            for (int i = 0; i < NUM_EDGES; i++)
            {
                int ori = cc.eo[i];
                for (int j = 0; j < 2; j++)
                {
                    var pos = GetEdge(i)[j];
                    faces[(int)pos.f][pos.r, pos.c] = (Face)GetEdgeFaces(cc.ep[i])[(j + ori) % 2];
                }
            }
        }

        /// <summary>
        /// Checks the number of occurances of each facelet to ensure it is a possible state.
        /// </summary>
        /// <returns>Whether or not this is a valid state.</returns>
        protected override string Verify()
        {
            // For every face colour
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                int count = 0;
                // Check how many times that colour occurs in each face
                foreach (Face[,] face in faces)
                    count += Enumerable.Range(0, DIM_SQR).Count(i => face[i / DIM, i % DIM] == f);
                if (count != DIM_SQR) return "Incorrect number of individual facelets";
            }
            return VALID_STATE;
        }

        // Check if the cube is solved
        public override bool IsSolved()
        {
            // For each face of the cube
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                Face[,] face = faces[(int)f];
                // If any of the facelets shouldn't be on this face
                if (Enumerable.Range(0, DIM_SQR).Any(i => face[i / DIM, i % DIM] != f))
                    return false;
            }
            return true;
        }

        // Pretty-print this cube
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

        /// <summary>
        /// Flattens the 3d array and converts it to a string
        /// </summary>
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

        #region Turning the cube
        /*
         * A move is rotating one face of the cube in a specific direction (90 degrees clockwise, 180 degrees, 90 degrees counterclockwise)
         * All these moves can be generated from three moves (x rotation, y rotation, U turn) defined below
         * 180 degree and 90 degree counterclockwise moves are done by applying the clockwise move 2 or 3 times respectively
         */
        public override Cube ApplyMove(Face f, Dir dir)
        {
            FaceletCube res = new FaceletCube(this);
            for (int i = 0; i <= (int)dir; i++)
                res = res.ApplyMove(f);
            return res;
        }

        // Cycles for the faces which are moved by these rotations
        Face[] yCycle = new Face[] { Face.F, Face.L, Face.B, Face.R }; // The F face goes to the L face which -> B -> R -> F
        Face[] xCycle = new Face[] { Face.F, Face.U, Face.B, Face.D }; // F -> U -> B -> D -> F

        /*
         * Performs a y rotation (the effect here is shown by labelling the corners)
         *    e-------f                 a-------e
         *   /|      /|                /|      /|
         *  / |     / |               / |     / |
         * a--+----b  |   goes to    b--+----f  |
         * |  g----|--h              |  c----|--g
         * | /     | /               | /     | /
         * c-------d                 d-------h
         *
         *      |
         *    e-|-----f
         *   /| |    /|
         *  / | |   / |
         * a--+-|--b  |  Rotating about this axis going through the U and D centres
         * |  g-|--|--h  (U is the abef-face, and D is the cdgh-face)
         * | /  |  | / 
         * c-------d   
         *      |
         */
        private FaceletCube RotY()
        {
            FaceletCube res = new FaceletCube(this);

            for (int i = 0; i < yCycle.Length; i++)
                res.faces[(int)yCycle[(i + 1) % yCycle.Length]] = faces[(int)yCycle[i]];
            res = res.RotFace((int)Face.U); // After moving the adjacent faces, rotate the U and D faces
            for (int i = 0; i < 3; i++) res = res.RotFace((int)Face.D); // D face is rotated counterclockwise

            return res;
        }

        /*
         * Performs an x rotation (the effect here is shown by labelling the corners)
         *    e-------f                 g-------h
         *   /|      /|                /|      /|
         *  / |     / |               / |     / |   Rotating about the axis going through the R and L centres
         * a--+----b  |   goes to    e--+----f  |   (L is the aceg-face and R is the bdfh-face in the diagram)
         * |  g----|--h              |  c----|--d
         * | /     | /               | /     | /
         * c-------d                 a-------b
         */
        private FaceletCube RotX()
        {
            FaceletCube res = new FaceletCube(this);

            for (int i = 0; i < xCycle.Length; i++)
                res.faces[(int)xCycle[(i + 1) % xCycle.Length]] = faces[(int)xCycle[i]];
            res = res.RotFace((int)Face.R); // After moving the adjacent faces, rotate the R and L faces
            for (int i = 0; i < 3; i++) res = res.RotFace((int)Face.L); // L face is rotated counterclockwise
            for (int i = 0; i < 2; i++) res = res.RotFace((int)Face.B).RotFace((int)Face.D); // B and D faces are flipped upside down

            return res;
        }

        /// <summary>
        /// Rotates the upper layer of a cube
        /// </summary>
        /// <returns>A copy of the cube with a U move applied to it</returns>
        private FaceletCube ApplyUMove()
        {
            FaceletCube res = new FaceletCube(this);
            // Update U face
            RotFace((int)Face.U);
            // Update adjacent faces
            for (int i = 0; i < DIM; i++)
            {
                for (int f = 1; f < 5; f++)
                    res.faces[f][0, i] = faces[f % 4 + 1][0, i];
            }
            return res;
        }

        /// <summary>
        /// Rotates one face of the cube 90 degrees
        /// </summary>
        /// <param name="f">The face to rotate</param>
        private FaceletCube RotFace(int f)
        {
            FaceletCube res = new FaceletCube(this);
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                    res.faces[f][i, j] = faces[f][DIM - j - 1, i];
            }
            return res;
        }

        // Applies a clockwise move to one face of the cube by rotating the cube then applying a move
        // which is already defined. Counterclockwise rotations are done as 3 clockwise rotations.
        private FaceletCube ApplyMove(Face face)
        {
            switch (face)
            {
                case Face.U: return ApplyUMove();
                // x2 U x2
                case Face.D: return RotX().RotX().ApplyUMove().RotX().RotX();
                // x U x'
                case Face.F: return RotX().ApplyUMove().RotX().RotX().RotX();
                // x' U x
                case Face.B: return RotX().RotX().RotX().ApplyUMove().RotX();
                // y F y'
                case Face.R: return RotY().ApplyMove(Face.F).RotY().RotY().RotY();
                // y B y'
                case Face.L: return RotY().ApplyMove(Face.B).RotY().RotY().RotY();
            }
            return null;
        }
        #endregion
    }
}

