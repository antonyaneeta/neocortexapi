// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NeoCortexEntities.NeuroVisualizer;


namespace HtmClassifierUnitTest
{

    /// <summary>
    /// Check out student paper in the following URL: https://github.com/ddobric/neocortexapi/blob/master/NeoCortexApi/Documentation/Experiments/ML-19-20_20-5.4_HtmSparsityExperiments_Paper.pdf
    /// </summary>
    [TestClass]
    public class HtmClassifierTest
    {
        public TestContext TestContext { get; set; }


        private int numColumns = 1024;
        private int cellsPerColumn = 25;
        private HtmClassifier<string, ComputeCycle> htmClassifier;
        private Dictionary<string, List<double>> sequences;
        private string fileName;


        [TestInitialize]
        public void Setup()
        {
            htmClassifier = new HtmClassifier<string, ComputeCycle>();

            sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 0.9, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
          
            LearnHtmClassifier();

            fileName = $"{TestContext.TestName}.txt";
            HtmSerializer.Reset();


        }

        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierSerialization()
        {
    //Given
    //HtmClassifier Learning method is called in [TestInitialize] Setup() method

    //When

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }

            using (StreamReader sr = new StreamReader(fileName))
            {
                // HtmClassifier<string, ComputeCycle> htmClassifierDeserialized = new HtmClassifier<string, ComputeCycle>();
                HtmClassifier<string, ComputeCycle> htmClassifier1 = htmClassifier.Deserialize(sr);

    //Then
    //Check if 2 instances are equal by overriding Equals method in Classifier class
    Assert.IsTrue(htmClassifier.Equals(htmClassifier1));

                using (StreamWriter sw = new StreamWriter("deserialize-retest.txt"))
                {
                    htmClassifier.Serialize(htmClassifier1, null, sw);
                }

            }

    //File comparison of Serialised and deserialized HtmClassifier instances.
            HtmSerializer htmSerializer = new HtmSerializer();
            var bol = htmSerializer.FileCompare("deserialize-retest.txt", $"{TestContext.TestName}.txt");
            Console.WriteLine("*************Files compared and found : " + bol);

        }



        /// <summary>
        /// add multiple Sequence and train for 60 cycles ,with SP+TM. SP is pretrained on the given input pattern set.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierSerializeDeserialize()
        {
    //Given
            sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 3.0, 4.0, 5.0, 2.0, 5.0 }));
            sequences.Add("S3", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S4", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S5", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            LearnHtmClassifier();
    //When
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                // HtmClassifier<string, ComputeCycle> htmClassifierDeserialized = new HtmClassifier<string, ComputeCycle>();
                HtmClassifier<string, ComputeCycle> htmClassifier1 = htmClassifier.Deserialize(sr);

                using (StreamWriter sw = new StreamWriter("deserialize-retest.txt"))
                {
                    htmClassifier.Serialize(htmClassifier1, null, sw);
                }


            }

            HtmSerializer htmSerializer = new HtmSerializer();
    //Then
    //File comparison of Serialised and deserialized HtmClassifier instances.
            var bol = htmSerializer.FileCompare("deserialize-retest.txt", $"{TestContext.TestName}.txt");
            Console.WriteLine("*************Files compared and found : " + bol);
        }

        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierSerializationAgain()
        {
            // Arrange
            HtmClassifier<string, ComputeCycle> expected = new HtmClassifier<string, ComputeCycle>();
           // LearnHtmClassifier();
            string expectedSerialized = SerializeHtmClassifier(htmClassifier);

            // Act
            string actualSerialized;
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                HtmClassifier<string, ComputeCycle> actual = htmClassifier.Deserialize(sr);
                actualSerialized = SerializeHtmClassifier(actual);
            }

            // Assert
            Assert.AreEqual(expectedSerialized, actualSerialized);
        }

        private string SerializeHtmClassifier(HtmClassifier<string, ComputeCycle> htmClassifier)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    htmClassifier.Serialize(htmClassifier, null, sw);
                    sw.Flush();
                    ms.Position = 0;
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        ///  To check if only white space in serialised .txt file then during deserialise() method ,it  will Skip lines with no parameters
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierDeserializeEmptySpace()
        {
            
            //When
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.Write("|");
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
               
                HtmClassifier<string, ComputeCycle> htmClassifierDeserilalized = htmClassifier.Deserialize(sr);

            }

        }


        /// <summary>
        /// Here our taget is to whether we are getting any predicted value for input we have given one sequence s1
        /// and check from this sequence each input, will we get prediction or not.
        /// </summary>
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [TestCategory("Prod")]
        [TestMethod]
        public void CheckNextValueIsNotEmpty(int input)
        {
            //var tm = layer1.HtmModules.FirstOrDefault(m => m.Value is TemporalMemory);
            //((TemporalMemory)tm.Value).Reset(mem);

            var predictiveCells = getMockCells(CellActivity.PredictiveCell);

            var res = htmClassifier.GetPredictedInputValues(predictiveCells.ToArray(), 3);

            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                HtmSerializer.Serialize(cls, null, sw);
            }

            var tokens = res.First().PredictedInput.Split('_');
            var tokens2 = res.First().PredictedInput.Split('-');
            Debug.WriteLine($"->{tokens2[tokens.Length - 1]}");
            var predictValue = Convert.ToInt32(tokens2[tokens.Length - 1]);
            Assert.IsTrue(predictValue > 0);
        }

        /// <summary>
        ///Here we are checking if cells count is zero
        ///will we get any kind of exception or not
        /// </summary>
        [TestMethod]
        [TestCategory("Prod")]
        public void NoExceptionIfCellsCountIsZero()
        {
            Cell[] cells = new Cell[0];
            var res = htmClassifier.GetPredictedInputValues(cells, 3);
            Assert.AreEqual(res.Count, 0);
        }


        /// <summary>
        ///Check how many prediction results will be retrieved.
        /// </summary>
        [DataTestMethod]
        [TestCategory("Prod")]
        [DataRow(3)]
        [DataRow(4)]
        [TestMethod]
        public void CheckHowManyOfGetPredictedInputValues(int howMany)
        {
            var predictiveCells = getMockCells(CellActivity.PredictiveCell);

            var res = htmClassifier.GetPredictedInputValues(predictiveCells.ToArray(), Convert.ToInt16(howMany));

            Assert.IsTrue(res.Count == howMany);
        }

        private void LearnHtmClassifier()
        {
            int maxCycles = 100;

            foreach (var sequenceKeyPair in sequences)
            {
                int maxPrevInputs = sequenceKeyPair.Value.Count - 1;

                List<string> previousInputs = new List<string>();

                previousInputs.Add("-1.0");

                //
                // Now training with SP+TM. SP is pretrained on the given input pattern set.
                for (int i = 0; i < maxCycles; i++)
                {
                    foreach (var input in sequenceKeyPair.Value)
                    {
                        previousInputs.Add(input.ToString());
                        if (previousInputs.Count > maxPrevInputs + 1)
                            previousInputs.RemoveAt(0);

                        // In the pretrained SP with HPC, the TM will quickly learn cells for patterns
                        // In that case the starting sequence 4-5-6 might have the sam SDR as 1-2-3-4-5-6,
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


        private List<Cell> lastActiveCells = new List<Cell>();

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