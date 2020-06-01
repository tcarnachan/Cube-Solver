using System;

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

