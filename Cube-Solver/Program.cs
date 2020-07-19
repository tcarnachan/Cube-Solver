using System;
using Cube_Solver.Cubes;

namespace Cube_Solver
{
    class Program
    {
        static void Main()
        {
            string[] tests =
            {
                "UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD", // Solved cube
                "UUUUUUUUUULLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD", // Incorrect number of each facelet
                "UUUUUUUUFLLLLLLLLLFFRFFFFFFURRRRRRRRBBBBBBBBBDDDDDDDDD", // Twisted corner
                "UBUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBUBBBBBBBDDDDDDDDD", // Flipped edge
                "UUUUUUUUULRLLLLLLLFFFFFFFFFRLRRRRRRRBBBBBBBBBDDDDDDDDD", // Swapped two edges (odd parity)
                "UUUUUULLLLLDLLDLLDFFFFFFFFFURRURRURRBBBBBBBBBRRRDDDDDD"  // Rotated F-face (valid state)
            };
            foreach (string state in tests)
            {
                try
                {
                    Cube cube = new FaceletCube(state);
                    cube = new CubieCube((FaceletCube)cube);
                    Console.WriteLine("Valid state");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Console.ReadLine();
                }
            }
        }
    }
}
