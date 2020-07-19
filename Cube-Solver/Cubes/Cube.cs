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

        protected const int NUM_EDGES = 12;
        protected const int NUM_CORNERS = 8;

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

        protected enum Corner { ULB, UBR, URF, UFL, DBL, DRB, DFR, DLF }

        protected enum Edge { UB, UR, UF, UL, DB, DR, DF, DL, BL, BR, FR, FL }

        protected static readonly (Face f, int r, int c)[,] cornerPieces =
        {
            { (Face.U, 0, 0), (Face.L, 0, 0), (Face.B, 0, 2) },
            { (Face.U, 0, 2), (Face.B, 0, 0), (Face.R, 0, 2) },
            { (Face.U, 2, 2), (Face.R, 0, 0), (Face.F, 0, 2) },
            { (Face.U, 2, 0), (Face.F, 0, 0), (Face.L, 0, 2) },
            { (Face.D, 2, 0), (Face.B, 2, 2), (Face.L, 2, 0) },
            { (Face.D, 2, 2), (Face.R, 2, 2), (Face.B, 2, 0) },
            { (Face.D, 0, 2), (Face.F, 2, 2), (Face.R, 2, 0) },
            { (Face.D, 0, 0), (Face.L, 2, 2), (Face.F, 2, 0) }
        };

        protected static readonly (Face f, int r, int c)[,] edgePieces =
        {
            { (Face.U, 0, 1), (Face.B, 0, 1) }, { (Face.U, 1, 2),  (Face.R, 0, 1) }, { (Face.U, 2, 1), (Face.F, 0, 1) }, { (Face.U, 1, 0), (Face.L, 0, 1) },
            { (Face.D, 2, 1), (Face.B, 2, 1) }, { (Face.D, 1, 2),  (Face.R, 2, 1) }, { (Face.D, 0, 1), (Face.F, 2, 1) }, { (Face.D, 1, 0), (Face.L, 2, 1) },
            { (Face.B, 1, 2), (Face.L, 1, 0) }, { (Face.B, 1, 0),  (Face.R, 1, 2) }, { (Face.F, 1, 2), (Face.R, 1, 0) }, { (Face.F, 1, 0), (Face.L, 1, 2) }
        };
    }
}