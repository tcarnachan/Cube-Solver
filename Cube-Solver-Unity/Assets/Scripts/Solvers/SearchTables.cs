using System;
using System.IO;
using System.Collections.Generic;

using Cube_Solver.Cubes;

namespace Cube_Solver.Solver
{
    using Face = Cube.Face;
    using Dir = Cube.Dir;

    /// <summary>
    /// A class which creates, saves, and loads pruning tables and move tables
    /// which are used to speed up computation in Search
    /// </summary>
    public class SearchTables
    {
        private const int NUM_COS = 2187;   // 3^7 cases
        public const int NUM_ESLICE = 495; // 12C4 cases
        private const int NUM_EOS = 1013760; // 2^11 * 12C4 cases
        public const int NUM_CPS = 40320;  // 8! cases
        private const int NUM_EPS = 967680; // 8! * 4! cases

        // The filenames of where the various tables are stored
        private const string coPath = "corner_ori";
        private const string eoPath = "edge_ori";
        private const string cpPath = "corner_perm";
        private const string epPath = "edge_perm";
        private const string mtCPPath = "move_cp";
        private const string mtEPPath = "move_ep";

        // Pruning tables used to reduce the search space
        public PruningTable coTable, eoTable, cpTable, epTable;
        // Move tables to speed up applying moves in phase 2
        public MoveTable mtCP, mtEP;

        // Lists which store the valid moves in each phase of the solve
        public List<(Face, Dir)> phase1moves = new List<(Face, Dir)>();
        public List<(Face, Dir)> phase2moves = new List<(Face, Dir)>();

        public IdCalculator idCalc;
        private string resourcePath;
        public Action<string> Output;

        public SearchTables(string resourcePath, Action<string> Output)
        {
            this.resourcePath = resourcePath;
            this.Output = Output;

            CubieCube solved = CubieCube.SolvedCube();
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

        /// <summary>
        /// Either finds a pruning table which has already been generated, or generates a
        /// pruning table and saves it to a file for when it is next used
        /// </summary>
        /// <param name="table">The pruning table to initialise</param>
        /// <param name="solved">A CubieCube in the solved state</param>
        /// <param name="size">The number of elements in the pruning table</param>
        /// <param name="GetID">A function which converts a CubieCube into an index in the pruning table</param>
        /// <param name="applicableMoves">The moves which can be applied to the cube to generate this table</param>
        /// <param name="filename">The filename where the table is stored</param>
        /// <param name="description">A description of the table being generated for debugging purposes</param>
        private void InitPruningTable(ref PruningTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, List<(Face, Dir)> applicableMoves, string filename, string description)
        {
            // If the table has already been initialised
            if (table != null) return;
            string filepath = resourcePath + filename;
            // See if the table has been saved to a file
            if (File.Exists(filepath))
                table = new PruningTable(filepath);
            // Otherwise generate the table and save it to a file
            else
            {
                Output($"Generating {description} pruning tables...");
                table = new PruningTable(solved, size, GetID, applicableMoves, filepath);
                Output("done\n");
            }
        }

        /// <summary>
        /// Either finds a move table which has already been generated, or generates a
        /// move table and saves it to a file for when it is next used
        /// </summary>
        /// <param name="table">The move table to initialise</param>
        /// <param name="solved">A CubieCube in the solved state</param>
        /// <param name="size">The number of elements in the move table</param>
        /// <param name="GetID">A function which converts a CubieCube into an index in the move table</param>
        /// <param name="filename">The filename where the table is stored</param>
        /// <param name="description">A description of the table being generated for debugging purposes</param>
        private void InitMoveTable(ref MoveTable table, CubieCube solved, int size, Func<CubieCube, int> GetID, string filename, string description)
        {
            // If the table has already been initialised
            if (table != null) return;
            string filepath = resourcePath + filename;
            // See if the table has been saved to a file
            if (File.Exists(filepath))
                table = new MoveTable(filepath);
            // Otherwise generate the table and save it to a file
            else
            {
                Output($"Generating {description} move tables...");
                table = new MoveTable(solved, size, GetID, filepath);
                Output("done\n");
            }
        }
    }
}
