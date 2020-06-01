namespace Cube_Solver.Cubes
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

        protected abstract bool Verify();

        public abstract void Print();
    }
}