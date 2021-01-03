using System;
using System.IO;
using System.Collections.Generic;

using Cube_Solver.Cubes;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    public class SearchTables
    {
        private const int NUM_COS = 2187;   // 3^7 cases
        public const int NUM_ESLICE = 495; // 12C4 cases
        private const int NUM_EOS = 1013760; // 2^11 * 12C4 cases
        public const int NUM_CPS = 40320;  // 8! cases
        private const int NUM_EPS = 967680; // 8! * 4! cases

        public const string coPath = "corner_ori";
        public const string eoPath = "edge_ori";
        public const string cpPath = "corner_perm";
        public const string epPath = "edge_perm";
        public const string mtCPPath = "move_cp";
        public const string mtEPPath = "move_ep";

        public PruningTable coTable, eoTable, cpTable, epTable;
        public MoveTable mtCP, mtEP;

        public List<(Face, Dir)> phase1moves = new List<(Face, Dir)>();
        public List<(Face, Dir)> phase2moves = new List<(Face, Dir)>();

        public IdCalculator idCalc;
        private string resourcePath;
        public Action<string> Output;

        public SearchTables(CubieCube solved, string resourcePath, Action<string> Output)
        {
            this.resourcePath = resourcePath;
            this.Output = Output;

            idCalc = new IdCalculator(solved);

            // All moves are valid for phase 1
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                    phase1moves.Add((f, dir));
            }

            // Initialise phase 1 pruning tables
            InitPruningTable(ref coTable, solved, NUM_COS, idCalc.GetCO, phase1moves, coPath, "corner orientation");
            InitPruningTable(ref eoTable, solved, NUM_EOS, cc => idCalc.GetEO(cc) * NUM_ESLICE + idCalc.GetEslice(cc), phase1moves, eoPath, "edge orientation");

            // Phase 2 <U, D, F2, B2, R2, L2>
            foreach (Face f in Enum.GetValues(typeof(Face)))
            {
                if (f == Face.U || f == Face.D)
                {
                    foreach (Dir dir in Enum.GetValues(typeof(Dir)))
                        phase2moves.Add((f, dir));
                }
                else
                    phase2moves.Add((f, Dir.HALF));
            }

            // Initialise phase 2 pruning tables
            InitPruningTable(ref cpTable, solved, NUM_CPS, idCalc.GetCP, phase2moves, cpPath, "corner permutation");
            InitPruningTable(ref epTable, solved, NUM_EPS, idCalc.GetEP, phase2moves, epPath, "edge permutation");

            // Initialise phase 2 move tables
            InitMoveTable(ref mtCP, solved, NUM_CPS, idCalc.GetCP, mtCPPath, "corner permutation");
            InitMoveTable(ref mtEP, solved, NUM_EPS, idCalc.GetEP, mtEPPath, "edge permutation");
        }

        private void InitPruningTable(ref PruningTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Cube.Face, Cube.Dir)> applicableMoves, string filename, string description)
        {
            if (table != null) return;
            string filepath = resourcePath + filename;
            if (File.Exists(filepath))
                table = new PruningTable(filepath);
            else
            {
                Output($"Generating {description} pruning tables...");
                table = new PruningTable(solved, size, GetID, applicableMoves, filepath);
                Output("done\n");
            }
        }

        private void InitMoveTable(ref MoveTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, string filename, string description)
        {
            if (table != null) return;
            string filepath = resourcePath + filename;
            if (File.Exists(filepath))
                table = new MoveTable(filepath);
            else
            {
                Output($"Generating {description} move tables...");
                table = new MoveTable(solved, size, GetID, filepath);
                Output("done\n");
            }
        }
    }
}
