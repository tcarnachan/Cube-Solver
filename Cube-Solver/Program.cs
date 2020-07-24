using System;
using System.Threading;
using Cube_Solver.Cubes;
using Cube_Solver.Solver;

namespace Cube_Solver
{
    class Program
    {
        static void Main()
        {
            string solved = "UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD";

            /* Test case:
             * Input state: lflburfldfdrllururuflbffdlrbburrdublbudrbdufdfubrdlbdf
             * Algorithm:   U' B' R2 B2 D' R2 U B2 U' B2 D' B2 F2 L R2 U2 B U2 B2 L2
            */

            Search search = new Search(solved);

            Menu interactMenu = new Menu(new MenuItem[]
            {
                new MenuItem("Interact with a solved cube", () => { Interact(new FaceletCube(solved)); }),
                new MenuItem("Interact with a random cube", () => { Interact(new FaceletCube(CubieCube.RandomCube())); }),
                new MenuItem("Interact with a custom cube", () => { Interact(ReadCube()); })
            });

            Menu mainMenu = new Menu(new MenuItem[]
            {
                new MenuItem("Interact with a cube", () => { interactMenu.Update(); }),
                new MenuItem("Solve a cube", () =>
                {
                    FaceletCube cube = (FaceletCube)ReadCube();
                    Console.Clear();
                    Console.WriteLine("Press any key to exit");

                    // Start solving on a separate thread
                    new Thread(() => { search.Solve(new CubieCube(cube)); }).Start();
                    // Exit thread on key press
                    Console.ReadKey();
                    search.exit = true;
                }),
                new MenuItem("Quit", () => { Environment.Exit(0); })
            }); ;

            while (true)
                mainMenu.Update();
        }

        private static void Interact(Cube cube)
        {
            while (true)
            {
                Console.Clear();
                cube.Print();

                Console.WriteLine("Enter a cube face to apply a move [ULFRBD]\nor press ESC to exit");
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                    return;
                else
                {
                    char c = key.ToString()[0];
                    if (Cube.FACE_CHARS.Contains(c))
                        cube = cube.ApplyMove((Cube.Face)Cube.FACE_CHARS.IndexOf(c), Cube.Dir.CW);
                }
            }
        }

        /*
          The stickers are input in the following order (holding a fixed orientation)
                   .----------.
                   | 0  1  2  |
			       | 3  4  5  |
			       | 6  7  8  |
		.----------+----------+----------.----------.
		|  9 10 11 | 18 19 20 | 27 28 29 | 36 37 38 |
		| 12 13 14 | 21 22 23 | 30 31 32 | 39 40 41 |
		| 15 16 17 | 24 25 26 | 33 34 35 | 42 43 44 |
        '----------+----------+----------'----------'
			       | 45 46 47 |
			       | 48 49 50 |
                   | 51 52 53 |
                   '----------'
         */
        private static Cube ReadCube()
        {
            while(true)
            {
                Console.Write("Enter a cube state: ");
                string input = Console.ReadLine();
                try
                {
                    FaceletCube fc = new FaceletCube(input);
                    CubieCube cc = new CubieCube(fc);
                    return fc;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
