using Cube_Solver.Cubes;

using System;
using System.Collections.Generic;
using System.IO;

namespace Cube_Solver.Solver
{
    /// <summary>
    /// Used as a heuristic lookup to prune branches in ida* search
    /// </summary>
    public class PruningTable
    {
        private byte[] table;

        /// <summary>
        /// Loads the pruning table from a file
        /// </summary>
        public PruningTable(string filepath)
        {
            table = File.ReadAllBytes(filepath);
        }

        /// <summary>
        /// Generates the pruning table and saves it to a file
        /// </summary>
        /// <param name="solved">A CubieCube in the solved state</param>
        /// <param name="size">The number of elements in the pruning table</param>
        /// <param name="GetID">A function which converts a CubieCube into an index in the pruning table</param>
        /// <param name="applicableMoves">The moves which can be applied to the cube to generate this table</param>
        /// <param name="filename">The filename where the table is to be stored</param>
        public PruningTable(CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Cube.Face, Cube.Dir)> applicableMoves, string filepath)
        {
            // Initialise table
            table = new byte[size];
            for (int i = 0; i < size; i++)
                table[i] = 0xff;
            // The solved state requires 0 moves to solve
            table[GetID(solved)] = 0;

            byte moves = 1;
            int states = 1;

            List<CubieCube> curr = new List<CubieCube>();
            curr.Add(solved);

            // Run breadth-first search
            while(states < size)
            {
                List<CubieCube> next = new List<CubieCube>();

                // For every node in this layer
                foreach(CubieCube cc in curr)
                {
                    // Get each child node
                    foreach(var move in applicableMoves)
                    {
                        CubieCube c = (CubieCube)cc.ApplyMove(move.Item1, move.Item2);
                        int id = GetID(c);
                        // If this node has not already been visited
                        if(table[id] == 0xff)
                        {
                            // Update this node
                            states++;
                            table[id] = moves;
                            next.Add(c);
                        }
                    }
                }

                curr = next;
                moves++;
            }

            // Save table to file
            File.WriteAllBytes(filepath, table);
        }

        // Convenience accessor
        public byte this[int i]
        {
            get
            {
                return table[i];
            }
        }
    }
}
