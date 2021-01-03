using Cube_Solver.Cubes;

using System;
using System.Collections.Generic;
using System.IO;

namespace Cube_Solver.Solver
{
    public class PruningTable
    {
        private byte[] table;

        public PruningTable(string filepath)
        {
            table = File.ReadAllBytes(filepath);
        }

        public PruningTable(CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Cube.Face, Cube.Dir)> applicableMoves, string filepath)
        {
            table = new byte[size];
            for (int i = 0; i < size; i++)
                table[i] = 0xff;
            table[GetID(solved)] = 0;

            byte moves = 1;
            int states = 1;

            List<CubieCube> curr = new List<CubieCube>();
            curr.Add(solved);

            while(states < size)
            {
                List<CubieCube> next = new List<CubieCube>();

                foreach(CubieCube cc in curr)
                {
                    foreach(var move in applicableMoves)
                    {
                        CubieCube c = (CubieCube)cc.ApplyMove(move.Item1, move.Item2);
                        int id = GetID(c);
                        if(table[id] == 0xff)
                        {
                            states++;
                            table[id] = moves;
                            next.Add(c);
                        }
                    }
                }

                curr = next;
                Console.WriteLine($"Depth: {(int)moves} States: {states}");
                moves++;
            }

            File.WriteAllBytes(filepath, table);
        }

        public byte this[int i]
        {
            get
            {
                return table[i];
            }
        }
    }
}
