using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment
{


    internal class LearnHtmClassifierForSerialization
    {

        private int numColumns = 1024;
        private int cellsPerColumn = 25;
        private HtmClassifier<string, ComputeCycle> htmClassifier;
        private Dictionary<string, List<double>> sequences;
        private string fileName;
        private List<Cell> lastActiveCells = new List<Cell>();

        public HtmClassifier<string, ComputeCycle> Train()
        {

            htmClassifier = new HtmClassifier<string, ComputeCycle>();

            sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 0.9, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
            sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 3.0, 4.0, 5.0, 2.0, 5.0 }));

            LearnHtmClassifier();
            return htmClassifier;

        }
        public void LearnHtmClassifier()
        {
            int maxCycles = 100;

            foreach (var sequenceKeyPair in sequences)
            {
                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    foreach (var input in sequenceKeyPair.Value)
                    {
                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > maxPrevInputs + 1)
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the same SDR as 1-2-3-4-5-6,
                        // Which will result in returning of 4-5-6 instead of 1-2-3-4-5-6.
                        // HtmClassifier allways return the first matching sequence. Because 4-5-6 will be as first
                        // memorized, it will match as the first one.
                        if (previousInputs.Count < maxPrevInputs)
                            continue;

                        string key = GetKey(previousInputs, input, sequenceKeyPair.Key);
                        List<Cell> actCells = getMockCells(CellActivity.ActiveCell);
                        htmClassifier.Learn(key, actCells.ToArray());
                    }
                }
            }
        }


        /// <summary>
        /// Mock the cells data that we get from the Temporal Memory
        /// </summary>
        private List<Cell> getMockCells(CellActivity cellActivity)
        {
            List<Cell> cells = new List<Cell>();
            for (int k = 0; k < Random.Shared.Next(5, 20); k++)
            {
                int parentColumnIndx = Random.Shared.Next(0, numColumns);
                int numCellsPerColumn = Random.Shared.Next(0, cellsPerColumn);
                int colSeq = Random.Shared.Next(0, cellsPerColumn);

                cells.Add(new Cell(parentColumnIndx, colSeq, numCellsPerColumn, cellActivity));
            }

            if (cellActivity == CellActivity.ActiveCell)
            {
                lastActiveCells = cells;
            }
            else if (cellActivity == CellActivity.PredictiveCell)
            {
                // Append one of the cell from lastActiveCells to the randomly generated preditive cells to have some similarity
                cells.AddRange(lastActiveCells.GetRange
                    (
                        Random.Shared.Next(lastActiveCells.Count), 1
                    )
                );
            }

            return cells;
        }

        private string GetKey(List<string> prevInputs, double input, string sequence)
        {
            string key = string.Empty;

            for (int i = 0; i < prevInputs.Count; i++)
            {
                if (i > 0)
                    key += "-";

                key += prevInputs[i];
            }

            return $"{sequence}_{key}";
        }

    }
}

