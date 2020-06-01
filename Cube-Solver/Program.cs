using System;
using Cube_Solver.Cubes;

namespace Cube_Solver
{
    class Program
    {
        static void Main()
        {
            Cube cube = new FaceletCube("UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD");
            cube = cube.ApplyMove(Cube.Face.F, Cube.Dir.CW);
            cube.Print();
            cube = new CubieCube((FaceletCube)cube);
            cube.Print();

            /*foreach(Cube.Face face in Enum.GetValues(typeof(Cube.Face)))
            {
                foreach(Cube.Dir dir in Enum.GetValues(typeof(Cube.Dir)))
                {
                    Console.ReadKey(true);
                    cube.ApplyMove(face, dir).Print();
                }
            }*/
        }
    }
}
