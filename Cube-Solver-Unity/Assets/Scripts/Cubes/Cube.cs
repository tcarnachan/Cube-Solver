using System;

namespace Cube_Solver.Cubes
{
    /// <summary>
    /// Abstract class storing useful values
    /// </summary>
    public abstract class Cube
    {
        protected const int NUM_FACES = 6;
        protected const int DIM = 3;
        public const int DIM_SQR = DIM * DIM;
        public const string FACE_CHARS = "ULFRBD";

        public const int NUM_EDGES = 12;
        public const int NUM_CORNERS = 8;

        protected static Random random = new Random(0);

        // The six faces of the cube (Up, Left, Front, Right, Back, Down)
        public enum Face { U, L, F, R, B, D }

        // Direction for applying moves (clockwise, half-turn, counter-clockwise)
        public enum Dir {  CW, HALF, CCW }

        public enum Edge { UB, UR, UF, UL, DB, DR, DF, DL, BL, BR, FR, FL };
        public enum Corner { ULB, UBR, URF, UFL, DBL, DRB, DFR, DLF };

        /// <summary>
        /// Returns this cube with the move applied to it without modifying this object.
        /// </summary>
        /// <param name="f">The face to turn</param>
        /// <param name="dir">The direction to turn the face in</param>
        public abstract Cube ApplyMove(Face f, Dir dir);

        /// <summary>
        /// Verifies that this cube is in a solvable state
        /// </summary>
        protected abstract string Verify();
        protected const string VALID_STATE = "";

        /// <summary>
        /// Returns whether this cube is in a solved state
        /// </summary>
        public abstract bool IsSolved();

        /// <summary>
        /// Prints a representation of this cube to the terminal
        /// </summary>
        public abstract void Print();

        // Gets the three facelets for a corner cubie
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

        // Gets the adjacent faces which makes up a corner cubie
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

        // Gets the two facelets for an edge cubie
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

        // Gets the adjacent faces which makes up an edge cubie
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
