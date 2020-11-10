using System;

namespace Cube_Solver.Cubes
{
    /// <summary>
    /// Abstract class storing useful values
    /// </summary>
    abstract class Cube
    {
        protected const int NUM_FACES = 6;
        protected const int DIM = 3;
        public const int DIM_SQR = DIM * DIM;
        public const string FACE_CHARS = "ULFRBD";

        protected const int NUM_EDGES = 12;
        protected const int NUM_CORNERS = 8;

        protected static Random random = new Random(0);

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

        protected abstract string Verify();
        protected const string VALID_STATE = "";

        public abstract bool IsSolved();

        public abstract void Print();

        protected static (int f, int r, int c)[] GetCorner(int cubie)
        {
            int[] faces = GetCornerFaces(cubie);
            int t = (cubie / 4) * 2;
            (int x, int y) udCoord = (t, 0);
            for (int i = 0; i < cubie % 4; i++)
                udCoord = Rotate(udCoord, t == 0);
            return new (int, int, int)[]
            {
                (faces[0], udCoord.x, udCoord.y),
                (faces[1], t, t),
                (faces[2], t, 2 - t)
            };
        }

        protected static int[] GetCornerFaces(int cubie)
        {
            int t = cubie / 4;
            return new int[]
            {
                t * 5,
                (8 - cubie - t) % 4 + 1,
                (7 - cubie + t) % 4 + 1
            };
        }

        protected static (int f, int r, int c)[] GetEdge(int cubie)
        {
            int[] faces = GetEdgeFaces(cubie);
            // U/D faces
            if (cubie < 8)
            {
                int t = cubie / 4 * 2;
                (int x, int y) udCoord = (t, 1);
                for (int i = 0; i < cubie % 4; i++)
                    udCoord = Rotate(udCoord, t == 0);
                return new (int, int, int)[]
                {
                    (faces[0], udCoord.x, udCoord.y),
                    (faces[1], t, 1)
                };
            }
            // Middle layer
            else
            {
                int t = (cubie - 8) % 2;
                return new (int, int, int)[]
                {
                    (faces[0], 1, 2 * (1 - t)),
                    (faces[1], 1, 2 * t)
                };
            }
        }

        // Rotates (x, y) around (1, 1)
        private static (int, int) Rotate((int x, int y) coord, bool cw)
        {
            // Clockwise
            if (cw)
                return (coord.y, 2 - coord.x);
            // Counter-clockwise
            else
                return (2 - coord.y, coord.x);

        }

        protected static int[] GetEdgeFaces(int cubie)
        {
            // U/D faces
            if (cubie < 8)
            {
                return new int[]
                {
                    cubie / 4 * 5,
                    (7 - cubie) % 4 + 1
                };
            }
            // Middle layer
            else
            {
                int t = cubie - 8;
                return new int[]
                {
                    4 - t / 2 * 2, // B, B, F, F
                    (t + 1) / 2 % 2 * 2 + 1 // L, R, R, L
                };
            }
        }
    }
}
