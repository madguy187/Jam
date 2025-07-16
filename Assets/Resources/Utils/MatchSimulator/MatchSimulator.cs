using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Utils.Tests
{
    public class MatchSimulator : MonoBehaviour
    {
        [SerializeField] private SlotConfig slotConfig;
        private SlotGrid grid;
        private MatchDetector detector;
        private StreamWriter logWriter;
        private int scenarioCount = 0;

        void Start()
        {
            if (!Global.RUN_SIMULATION)
            {
                return;
            }

            if (slotConfig == null)
            {
                Debug.LogWarning("[MatchSimulator] SlotConfig is missin");
                return;
            }
                        
            grid = new SlotGrid(slotConfig.gridRows, slotConfig.gridColumns);
            detector = new MatchDetector(grid);
            InitializeLogFile();
            
            LogMessage("=== Starting Match Simulation Tests ===\n");

            if (slotConfig.gridRows == 3 && slotConfig.gridColumns == 3)
            {
                RunAllTests3x3();
            }
            else 
            {

                RunBasicTests();
            }
            
            LogMessage("\n=== Match Simulation Tests Complete ===");
            LogMessage($"Total Scenarios Run: {scenarioCount}");
            
            if (logWriter != null) logWriter.Close();
        }

        private void RunBasicTests()
        {
            SymbolType[,] fullGrid = new SymbolType[slotConfig.gridRows, slotConfig.gridColumns];
            for (int row = 0; row < slotConfig.gridRows; row++)
                for (int col = 0; col < slotConfig.gridColumns; col++)
                    fullGrid[row, col] = SymbolType.ATTACK;
            RunTest("Full Grid Pattern", fullGrid);

            SymbolType[,] alternating = new SymbolType[slotConfig.gridRows, slotConfig.gridColumns];
            for (int row = 0; row < slotConfig.gridRows; row++)
                for (int col = 0; col < slotConfig.gridColumns; col++)
                    alternating[row, col] = ((row + col) % 2 == 0) ? SymbolType.ATTACK : SymbolType.DEFENSE;
            RunTest("Alternating Pattern", alternating);

        }

        private void RunAllTests3x3()
        {
            // Original 3x3 test cases
            RunTest("Full Grid Pattern", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.ATTACK, SymbolType.ATTACK },
                { SymbolType.ATTACK, SymbolType.ATTACK, SymbolType.ATTACK },
                { SymbolType.ATTACK, SymbolType.ATTACK, SymbolType.ATTACK }
            });

            // 2. X-Shape with Center
            RunTest("X-Shape with Center", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.EMPTY, SymbolType.ATTACK },
                { SymbolType.EMPTY, SymbolType.DEFENSE, SymbolType.EMPTY },
                { SymbolType.ATTACK, SymbolType.EMPTY, SymbolType.ATTACK }
            });

            // 3. Alternating Pattern
            RunTest("Alternating Pattern", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.ATTACK },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.ATTACK }
            });

            // 4. Triple Symbol Mix
            RunTest("Triple Symbol Mix", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.DEFENSE, SymbolType.SPECIAL, SymbolType.ATTACK },
                { SymbolType.SPECIAL, SymbolType.ATTACK, SymbolType.DEFENSE }
            });

            // 5. Enclosed Pattern
            RunTest("Enclosed Pattern", new SymbolType[3,3] {
                { SymbolType.DEFENSE, SymbolType.DEFENSE, SymbolType.DEFENSE },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.DEFENSE, SymbolType.DEFENSE, SymbolType.DEFENSE }
            });

            // 6. Diagonal Chain
            RunTest("Diagonal Chain", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.EMPTY, SymbolType.EMPTY },
                { SymbolType.EMPTY, SymbolType.ATTACK, SymbolType.EMPTY },
                { SymbolType.EMPTY, SymbolType.EMPTY, SymbolType.ATTACK }
            });

            // 7. Mixed Complex
            RunTest("Mixed Complex", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.SPECIAL, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.DEFENSE, SymbolType.SPECIAL, SymbolType.ATTACK }
            });

            // 8. Double X-Shape
            RunTest("Double X-Shape", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK },
                { SymbolType.SPECIAL, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK }
            });

            // 9. Horizontal-Vertical Mix
            RunTest("Horizontal-Vertical Mix", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.ATTACK, SymbolType.ATTACK },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.SPECIAL },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.SPECIAL }
            });

            // 10. Complex Distribution
            RunTest("Complex Distribution", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.DEFENSE },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.SPECIAL },
                { SymbolType.SPECIAL, SymbolType.DEFENSE, SymbolType.ATTACK }
            });

            // 11. Nested X Pattern
            RunTest("Nested X Pattern", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.ATTACK },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.ATTACK }
            });

            // 12. Triple Diagonal Intersection
            RunTest("Triple Diagonal Intersection", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.DEFENSE },
                { SymbolType.SPECIAL, SymbolType.ATTACK, SymbolType.SPECIAL },
                { SymbolType.DEFENSE, SymbolType.SPECIAL, SymbolType.ATTACK }
            });

            // 13. Spiral Pattern
            RunTest("Spiral Pattern", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.ATTACK, SymbolType.ATTACK },
                { SymbolType.SPECIAL, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.SPECIAL, SymbolType.SPECIAL, SymbolType.DEFENSE }
            });

            // 14. Alternating Corners
            RunTest("Alternating Corners", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.DEFENSE, SymbolType.EMPTY, SymbolType.DEFENSE },
                { SymbolType.SPECIAL, SymbolType.DEFENSE, SymbolType.ATTACK }
            });

            // 15. Double Diagonal Cross
            RunTest("Double Diagonal Cross", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK },
                { SymbolType.DEFENSE, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK }
            });

            // 16. Zigzag Chain
            RunTest("Zigzag Chain", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.EMPTY },
                { SymbolType.EMPTY, SymbolType.ATTACK, SymbolType.DEFENSE },
                { SymbolType.EMPTY, SymbolType.EMPTY, SymbolType.ATTACK }
            });

            // 17. Symmetrical Distribution
            RunTest("Symmetrical Distribution", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK },
                { SymbolType.DEFENSE, SymbolType.DEFENSE, SymbolType.DEFENSE },
                { SymbolType.ATTACK, SymbolType.SPECIAL, SymbolType.ATTACK }
            });

            // 18. Triple Symbol Cascade
            RunTest("Triple Symbol Cascade", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.DEFENSE, SymbolType.SPECIAL, SymbolType.ATTACK },
                { SymbolType.SPECIAL, SymbolType.ATTACK, SymbolType.DEFENSE }
            });

            // 20. Complex Corner Connection
            RunTest("Complex Corner Connection", new SymbolType[3,3] {
                { SymbolType.ATTACK, SymbolType.EMPTY, SymbolType.ATTACK },
                { SymbolType.SPECIAL, SymbolType.DEFENSE, SymbolType.SPECIAL },
                { SymbolType.ATTACK, SymbolType.EMPTY, SymbolType.ATTACK }
            });
        }

        private void RunTest(string name, SymbolType[,] pattern)
        {
            scenarioCount++;
            LogMessage($"\nScenario {scenarioCount}: {name}");
            LogMessage("----------------------------------------");
            
            for (int row = 0; row < slotConfig.gridRows; row++)
                for (int col = 0; col < slotConfig.gridColumns; col++)
                    grid.SetSlot(row, col, pattern[row, col]);

            // Print grid
            StringBuilder sb = new StringBuilder("\nCurrent Grid State:\n");
            string horizontalLine = new string('-', slotConfig.gridColumns * 2 + 3);
            sb.AppendLine(horizontalLine);
            for (int row = 0; row < slotConfig.gridRows; row++)
            {
                sb.Append("| ");
                for (int col = 0; col < slotConfig.gridColumns; col++)
                {
                    string symbol = grid.GetSlot(row, col) switch
                    {
                        SymbolType.ATTACK => "A",
                        SymbolType.DEFENSE => "D",
                        SymbolType.SPECIAL => "S",
                        _ => "-"
                    };
                    sb.Append($"{symbol} ");
                }
                sb.AppendLine("|");
            }
            sb.AppendLine(horizontalLine);
            LogMessage(sb.ToString());

            var matches = detector.DetectMatches();
            LogMessage("\n=== Raw Detector Output ===");
            LogMessage($"Total Raw Matches Found: {matches.Count}");

            foreach (MatchType type in new[] { MatchType.SINGLE, MatchType.HORIZONTAL, MatchType.VERTICAL, MatchType.DIAGONAL })
            {
                LogMessage($"\n{type} Matches:");
                foreach (var match in matches.Where(m => m.GetMatchType() == type))
                {
                    LogMessage($"  - {match.GetSymbol()} at positions: {string.Join(", ", match.GetReadablePositions())}");
                }
            }

            LogMessage("\n>>> End of Scenario <<<\n");
            LogMessage("========================================\n");
        }

        private void InitializeLogFile()
        {
            try
            {
                string directory = Path.Combine(Application.dataPath, "MatchLogs");
                Directory.CreateDirectory(directory);
                foreach (string file in Directory.GetFiles(directory, "match_test_*.txt"))
                    File.Delete(file);
                
                logWriter = new StreamWriter(Path.Combine(directory, 
                    $"match_test_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt"));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MatchSimulator] Failed to create log file: {e.Message}");
            }
        }

        private void LogMessage(string message)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine(message);
                logWriter.Flush();
            }
        }
    }
} 