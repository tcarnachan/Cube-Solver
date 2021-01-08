using Cube_Solver.Cubes;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    /// <summary>
    /// Used to quickly apply moves in phase 2 of Search
    /// </summary>
    public class MoveTable
    {
        private Dictionary<Face, int[]> table;

        /// <summary>
        /// Loads the move table from a file
        /// </summary>
        public MoveTable(string filepath)
        {
            Stream stream = new FileStream(filepath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            table = (Dictionary<Face, int[]>)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Generates the move table and saves it to a file
        /// </summary>
        /// <param name="solved">A CubieCube in the solved state</param>
        /// <param name="size">The number of elements in the move table</param>
        /// <param name="GetID">A function which converts a CubieCube into an index in the move table</param>
        /// <param name="filename">The filename where the table is to be stored</param>
        public MoveTable(CubieCube solved, int size, Func<CubieCube, int> GetID, string filepath)
        {
            // Initialise the table
            table = new Dictionary<Face, int[]>();
            foreach (Face f in Enum.GetValues(typeof(Face)))
                table[f] = new int[size];

            // Breadth-first search
            Queue<CubieCube> path = new Queue<CubieCube>();
            path.Enqueue(solved);

            HashSet<int> seen = new HashSet<int>();
            seen.Add(GetID(solved));

            while (path.Count > 0)
            {
                // Get current node
                CubieCube curr = path.Dequeue();
                int currID = GetID(curr);
                // For each child node of the current node
                foreach (Face face in Enum.GetValues(typeof(Face)))
                {
                    Dir dir = Dir.HALF;
                    if (face == Face.U || face == Face.D) dir = Dir.CW;
                    CubieCube cc = (CubieCube)curr.ApplyMove(face, dir);
                    int id = GetID(cc);
                    table[face][currID] = id;
                    // If the child has not been seen
                    if (seen.Add(id))
                        path.Enqueue(cc);
                }
            }

            // Save to file
            Stream stream = new FileStream(filepath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, table);
        }

        // Convenience accessor
        public int this[int i, Face f]
        {
            get
            {
                return table[f][i];
            }
        }
    }
}
