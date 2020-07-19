using System;
using Cube_Solver.Cubes;

namespace Cube_Solver
{
    class Program
    {
        static void Main()
        {
            string solved = "UUUUUUUUULLLLLLLLLFFFFFFFFFRRRRRRRRRBBBBBBBBBDDDDDDDDD";

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
