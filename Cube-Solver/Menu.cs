using System;

namespace Cube_Solver
{
    class Menu
    {
        private MenuItem[] items;

        public Menu(MenuItem[] items)
        {
            this.items = items;
        }

        public void Update()
        {
            Display();
            items[ReadInt(1, items.Length) - 1].function();
        }

        private void Display()
        {
            Console.Clear();
            for (int i = 0; i < items.Length; i++)
                Console.WriteLine($" [{i + 1}] {items[i].message}");
            Console.Write("Select an option: ");
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
