using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text.Json;

namespace HtmClassifierUnitTest
{
    [TestClass()]
    public class HtmClassiferSerializationTests
    {
        public TestContext TestContext { get; set; }


        private int numColumns = 1024;
        private int cellsPerColumn = 25;
        private HtmClassifier<string, ComputeCycle> htmClassifier;
        private Dictionary<string, List<double>> sequences;
        private string fileName;
        private List<Cell> lastActiveCells = new List<Cell>();

        [TestInitialize]
        public void Setup()
        {
            htmClassifier = new HtmClassifier<string, ComputeCycle>();

            sequences = new Dictionary<string, List<double>>();
            sequences.Add("S1", new List<double>(new double[] { 0.9, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
            sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 3.0, 4.0, 5.0, 2.0, 5.0 }));

            LearnHtmClassifier();

            fileName = $"{TestContext.TestName}.txt";
            HtmSerializer.Reset();

        }

        [TestMethod]
        [TestCategory("ProjectUnitTest")]
        public void TestSerializationHtmClassifier()
        {
            //Given
            //HtmClassifier Lerrning method is called in [TestInitialize] Setup() method
            HtmClassifier<string, ComputeCycle> htmClassifier1;

            //When
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                //The second instance of HTMCLassifier after Deserialisation  
                htmClassifier1 = htmClassifier.Deserialize(sr);
                //the below is for writing to files and Do File comparison to verify serialization
                using (StreamWriter sw = new StreamWriter($"{TestContext.TestName}FileTest.txt"))
                {
                    htmClassifier.Serialize(htmClassifier1, null, sw);
                }
            }

            //Then
            //Check if 2 instances are equal 
            Assert.IsTrue(htmClassifier.Equals(htmClassifier1));

        }


        /// <summary>
        /// add multiple Squence and train for 60 cycles ,with SP+TM. SP is pretrained on the given input pattern set.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestSerializeDeserializeHtmClassifier()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier1;
           
            sequences.Add("S3", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S4", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S5", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            LearnHtmClassifier();

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {

                htmClassifier1 = htmClassifier.Deserialize(sr);
               
                using (StreamWriter sw = new StreamWriter("deserialize-retest.txt"))
                {
                    htmClassifier.Serialize(htmClassifier1, null, sw);
                }
            }

            //Then
            //Check if 2 instances are equal 
            Assert.IsTrue(htmClassifier.Equals(htmClassifier1));


            //File comparison method to check if serialize deserialised worked.
            HtmSerializer htmSerializer = new HtmSerializer();

            var isSameFile = htmSerializer.FileCompare("deserialize-retest.txt", $"{TestContext.TestName}.txt");
            Console.WriteLine("*************File compared and found : " + isSameFile);

        }

        /// <summary>        
        /// Here we intend to check failure of serialization deserialization.
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestSerializeDeserializeHtmClassifierFailure()
        {
            //Given
            sequences.Add("S6", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S7", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S8", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            LearnHtmClassifier();
            HtmClassifier<string, ComputeCycle> htmClassifier1;

            //When
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader("deserialize-retest.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier.Equals(htmClassifier1));
        }

        /// <summary>        
        /// Below we unit test for Equals overide method
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEquals()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifieractual =new HtmClassifier<string, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifieractual.Equals(htmClassifier1));
        }

        /// <summary>        
        /// Below we unit test for Equals method when objects are of different type
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEqualsTest1()
        {
            //Given
            HtmSerializer htmSerializer = new HtmSerializer();
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier1.Equals(htmSerializer));
        }

        /// <summary>        
        /// Below we unit test for Equals method if test object is null
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEqualsTest2()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifieractual = null;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier1.Equals(htmClassifieractual));
        }
        /// <summary>        
        /// Below we unit test failure of for Equals method if test object different
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEqualsTest3()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier2;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject2.txt"))
            {
                htmClassifier2 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier1.Equals(htmClassifier2));
        }
        /// <summary>        
        /// Below we unit test failure of for Equals method if test object when one doesnot have all class parameters
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEqualsTest4()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier3;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject3.txt"))
            {
                htmClassifier3 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier3.Equals(htmClassifier1));
        }

        /// <summary>        
        /// Below we unit test failure of for Equals method if test object when one doesnot have all class parameters
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        public void TestHtmClassifierEqualsTest5()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier3 ;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject3.txt"))
            {
                htmClassifier3 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier3.Equals(htmClassifier1));
        }

        private void LearnHtmClassifier()
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