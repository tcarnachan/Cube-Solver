using System;

namespace Cube_Solver
{
    /// <summary>
    /// Abstract class storing useful values
    /// </summary>
    public abstract class Cube
    {
        protected const int NUM_FACES = 6;
        protected const int DIM = 3;

        public enum Face { U, L, F, R, B, D }

        protected abstract bool Verify();
    }

    /// <summary>
    /// Represents a cube on the facelet level.
    /// Each 'sticker' on the cube is stored in a char[][][] which is accessed by [FACE][Row][Col].
    /// </summary>
    public class FaceletCube : Cube
    {
        private char[][][] faces;

        /// <summary>
        /// Constructs a facelet cube from a string representation.
        /// </summary>
        /// <param name="state">The state of the facelet cube.</param>
        public FaceletCube(string state)
        {
        }

        /// <summary>
        /// Checks the number of each facelet to ensure it is a possible state
        /// </summary>
        /// <returns>Whether or not this is a valid state</returns>
        protected override bool Verify()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
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