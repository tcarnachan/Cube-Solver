using System;
using System.Collections.Generic;

namespace Cube_Solver
{
    class Solver
    {
        List<SolverStage> stages = new List<SolverStage>();

        public Solver() { }

        public Solver(SolverStage[] stages) { }

        public int[] Solve() { throw new NotImplementedException(); }
    }

    abstract class SolverStage
    {
        protected const string solved = "UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD";

        protected Cube.Face[] applicableMoves;

        public abstract int[] Solve(Cube cube);
    }

    class KociembaPhase1 : SolverStage
    {
        public override int[] Solve(Cube cube)
        {
            throw new NotImplementedException();
        }
    }

    class KociembaPhase2 : SolverStage
    {
        public override int[] Solve(Cube cube)
        {
            throw new NotImplementedException();
        }
    }
}