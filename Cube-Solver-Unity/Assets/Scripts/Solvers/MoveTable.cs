using Cube_Solver.Cubes;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    public class MoveTable
    {
        private Dictionary<Face, int[]> table;

        public MoveTable(string filepath)
        {
            Stream stream = new FileStream(filepath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            table = (Dictionary<Face, int[]>)formatter.Deserialize(stream);
        }

        public MoveTable(CubieCube solved, int size, Func<CubieCube, int> GetID, string filepath)
        {
            table = new Dictionary<Face, int[]>();
            foreach (Face f in Enum.GetValues(typeof(Face)))
                table[f] = new int[size];

            Queue<CubieCube> path = new Queue<CubieCube>();
            path.Enqueue(solved);

            HashSet<int> seen = new HashSet<int>();
            seen.Add(GetID(solved));

            while (path.Count > 0)
            {
                CubieCube curr = path.Dequeue();
                int currID = GetID(curr);
                foreach (Face face in Enum.GetValues(typeof(Face)))
                {
                    Dir dir = Dir.HALF;
                    if (face == Face.U || face == Face.D) dir = Dir.CW;
                    CubieCube cc = (CubieCube)curr.ApplyMove(face, dir);
                    int id = GetID(cc);
                    table[face][currID] = id;
                    if (seen.Add(id))
                        path.Enqueue(cc);
                }
            }

            Stream stream = new FileStream(filepath, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, table);
        }

        public int this[int i, Face f]
        {
            get
            {
                return table[f][i];
            }
        }
    }
}
