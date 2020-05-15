using System;
using System.Collections.Generic;
using System.Linq;

namespace Cube_Solver
{
    /// <summary>
    /// Abstract class storing useful values
    /// </summary>
    public abstract class Cube
    {
        protected const int NUM_FACES = 6;
        protected const int DIM = 3;
        protected const int DIM_SQR = DIM * DIM;
        protected const string FACE_CHARS = "ULFRBD";

        public enum Face { U, L, F, R, B, D }

        /// <summary>
        /// Direction for applying moves (clockwise, half-turn, counter-clockwise)
        /// </summary>
        public enum Dir {  CW, HALF, CCW }

        /// <summary>
        /// Returns this cube with the move applied to it without modifying this object.
        /// </summary>
        /// <param name="f">The face to apply the move to.</param>
        /// <param name="dir">The direction of the move.</param>
        public abstract Cube ApplyMove(Face f, Dir dir);

        protected abstract bool Verify();

        public abstract void Print();
    }

    /// <summary>
    /// Represents a cube on the facelet level.
    /// Each 'sticker' on the cube is stored in a char[][][] which is accessed by [FACE][Row][Col].
    /// </summary>
    public class FaceletCube : Cube
    {
        private char[][,] faces;

        /// <summary>
        /// Constructs a facelet cube from a string representation.
        /// </summary>
        /// <param name="state">The state of the facelet cube.</param>
        public FaceletCube(string state)
        {
            Dictionary<char, char> replacements = new Dictionary<char, char>();
            for(int i = 0; i < NUM_FACES; i++)
                replacements[FACE_CHARS[i]] = state[(i * DIM_SQR) + (DIM_SQR / 2)];

            faces = new char[NUM_FACES][,];
            for(int i = 0; i < NUM_FACES; i++)
            {
                string face = state.Substring(i * DIM_SQR, DIM_SQR);
                char[,] f = new char[DIM, DIM];
                for (int ix = 0; ix < DIM_SQR; ix++)
                    f[ix / DIM, ix % DIM] = replacements[face[ix]];
                faces[i] = f;
            }

            if (!Verify())
                throw new Exception("Incorrect number of individual facelets on cube");
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="fc">The FaceletCube to copy.</param>
        public FaceletCube(FaceletCube fc)
        {
            faces = new char[NUM_FACES][,];
            for (int i = 0; i < NUM_FACES; i++)
            {
                faces[i] = new char[DIM, DIM];
                Array.Copy(fc.faces[i], faces[i], DIM_SQR);
            }
        }

        /// <summary>
        /// Checks the number of each facelet to ensure it is a possible state
        /// </summary>
        /// <returns>Whether or not this is a valid state</returns>
        protected override bool Verify()
        {
            foreach(char c in FACE_CHARS)
            {
                int count = 0;
                foreach(char[,] face in faces)
                    count += Enumerable.Range(0, DIM_SQR).Count(i => face[i / 3, i % 3] == c);
                if (count != DIM_SQR) return false;
            }
            return true;
        }

        public override void Print()
        {
            // Print U-face
            char[,] f = faces[0];
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
            char[,] oldFace = faces[(int)face];
            char[,] newFace = res.faces[(int)face];
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

    /// <summary>
    /// Represents a cube on the cubie level.
    /// Each cubie has an orientation and a permutation.
    /// Cubies are split into corners and edges as they cannot change places.
    /// </summary>
    public class CubieCube : Cube
    {
        private const int NUM_EDGES = 12;
        private const int NUM_CORNERS = 8;

        /// <summary>
        /// Corner orientations, 0 = oriented, 1 = twisted clockwise, 2 = twisted counter-clockwise.
        /// </summary>
        private byte[] co = new byte[NUM_CORNERS];
        /// <summary>
        /// Corner permutations, stores numbers from 0-7 representing each corner.
        /// </summary>
        private byte[] cp = new byte[NUM_CORNERS];
        /// <summary>
        /// Edge orientations, 0 = oriented, 1 = mis-oriented.
        /// </summary>
        private byte[] eo = new byte[NUM_EDGES];
        /// <summary>
        /// Edge permutations, stores numbers from 0-11 representing each corner.
        /// </summary>
        private byte[] ep = new byte[NUM_EDGES];

        /// <summary>
        /// Constructs a CubieCube from a cube at the facelet level.
        /// </summary>
        /// <param name="fc">The cube at the facelet level.</param>
        public CubieCube(FaceletCube fc)
        {
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="cc">The CubieCube to copy.</param>
        public CubieCube(CubieCube cc)
        {
        }

        public override Cube ApplyMove(Face f, Dir dir)
        {
            throw new NotImplementedException();
        }

        public override void Print()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks number of each piece, orientations of the pieces and the parity of the cube to ensure it is a possible state.
        /// </summary>
        /// <returns>Whether or not this is in a valid state.</returns>
        protected override bool Verify()
        {
            throw new NotImplementedException();
        }
    }
}