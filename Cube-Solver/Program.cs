using System;
using Cube_Solver.Cubes;
using Cube_Solver.Solvers;

namespace Cube_Solver
{
    class Program
    {
        static void Main()
        {
            string solved = "UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD";

            #region Testing
            FaceletCube fc = new FaceletCube(solved);
            CubieCube cc = new CubieCube(fc);
            Solver solver = new Solver(cc);

            cc = (CubieCube)ApplyAlgorithm(cc, "R U R' F' R2 D' F2 L");
            solver.Solve(cc);

            /*
            fc = new FaceletCube((CubieCube)cc);
            fc.Print();

            Console.ReadKey(true);*/
            return;
            #endregion

            MenuItem[] menuItems = new MenuItem[] {
                new MenuItem("Interact with a solved cube", () => { Interact(new FaceletCube(solved)); }),
                new MenuItem("Enter a custom cube state", () => { Interact(ReadCube()); }),
                new MenuItem("Quit", () => { Environment.Exit(0); })
            };

            while (true)
            {
                DisplayMenu(menuItems);
                menuItems[ReadInt(1, menuItems.Length) - 1].function();
            }
        }

        private static Cube ApplyAlgorithm(Cube cube, string algorithm)
        {
            foreach(string s in algorithm.Split(' '))
            {
                Cube.Face f = (Cube.Face)Cube.FACE_CHARS.IndexOf(s[0]);
                if (s.Length == 1)
                    cube = cube.ApplyMove(f, Cube.Dir.CW);
                else
                    cube = cube.ApplyMove(f, (Cube.Dir)" 2'".IndexOf(s[1]));
            }
            return cube;
        }

        private static void DisplayMenu(MenuItem[] menuItems)
        {
            Console.Clear();
            for (int i = 0; i < menuItems.Length; i++)
                Console.WriteLine($" [{i + 1}] {menuItems[i].message}");
            Console.Write("Select an option: ");
        }

        private static void Interact(Cube cube)
        {
            while (true)
            {
                Console.Clear();
                cube.Print();

                Console.WriteLine("Enter a cube face to apply a move [ULFRBD]\nor press ESC to go back");
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

        private static int ReadInt(int min, int max)
        {
            string input = Console.ReadLine();
            int res;
            while (!int.TryParse(input, out res) || res < min || res > max)
            {
                Console.WriteLine($"Please enter a valid integer between {min} and {max}");
                input = Console.ReadLine();
            }
            
            return res;
        }

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

    struct MenuItem
    {
        public string message;
        public Action function;

        public MenuItem(string message, Action function)
        {
            this.message = message;
            this.function = function;
        }
    }
}
