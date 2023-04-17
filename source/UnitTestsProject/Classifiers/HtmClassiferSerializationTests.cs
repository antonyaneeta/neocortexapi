using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


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

        /// <summary>
        /// Test method to check serialization with help of Equals method
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTest")]
        [TestCategory("Test-Serialization")]
        public void TestSerializationHtmClassifier()
        {
            //Given
            //HtmClassifier Learning method is called in [TestInitialize] Setup() method
            HtmClassifier<string, ComputeCycle> htmClassifier1;

            //When
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                //The second instance of HTMClassifier after Deserialization  
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
        /// Here we intend to check failure of serialization deserialization with Equals()
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Test-Serialization")]
        public void TestSerializeDeserializeHtmClassifierFailure()
        {
            //Given
            sequences.Add("S6", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S7", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            sequences.Add("S8", new List<double>(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 2.0, 3.0 }));
            LearnHtmClassifier();
            //Second HTMClassifier instance initialized.
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
        /// Test method to test the Serialization using Equals and also file comparison method.
        /// add multiple Sequence and train for 60 cycles ,with SP+TM. SP is pretrained on the given input pattern set.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Test-Serialization")]
        public void TestSerializeHtmClassifierByFileComparison()
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


            //File comparison method to check if serialize deserialize worked.
            HtmSerializer htmSerializer = new HtmSerializer();
            var isSameFile = htmSerializer.FileCompare("deserialize-retest.txt", $"{TestContext.TestName}.txt");
            Debug.WriteLine(String.Format("*************File compared and found :  {0}", isSameFile));

        }

        /// <summary>
        /// Test method to test serialization by reading the saved stream of data.
        /// An expected htmClassifier serialized value is compared with actual obtained serialized stream.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Test-Serialization")]
        public void TestSerializeHtmClassifierByUsingStreamComparison()
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


        /// <summary>
        /// Test method to test serialization with different inputs results in 2 different objects.
        /// An expected htmClassifier serialized value is compared with actual obtained serialized stream for a different file for test files, with help of method private string SerializeHtmClassifier()
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Test-Serialization")]
        public void TestHtmClassifierSerializationFailsWhenDifferentwithStreamComparison()
        {
            // Arrange
            HtmClassifier<string, ComputeCycle> expected = new HtmClassifier<string, ComputeCycle>();
            string E_inFolder = "Classifiers/ClassifierTestsInputs";
            string expectedSerialized = SerializeHtmClassifier(htmClassifier);

            // Act
            string actualSerialized;
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
            {
                HtmClassifier<string, ComputeCycle> actual = htmClassifier.Deserialize(sr);
                actualSerialized = SerializeHtmClassifier(actual);
            }

            // Assert
            //Both will not be same
            Assert.AreNotEqual(expectedSerialized, actualSerialized);
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
        /// This method is indented to test the Hash code generated for two equal HTMClassifier instance.
        /// This tests the Hash code generated by the overridden GetHasCode() .
        /// second instances is created by deserialization by using a serialized text file.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("GetHashCode-Testing")]
        public void HashcodeSameForEqualObjectsTest()
        {
            //Given
            //New instance initialised for HTMClassifier to be used in deserialization process.
            HtmClassifier<string, ComputeCycle> htmClassifierNew;

            //Serialisation begins and save to a File
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }

            //The saved serialized File used as input to Deserialize method and creates a new instance
            using (StreamReader sr = new StreamReader(fileName))
            {
                htmClassifierNew = htmClassifier.Deserialize(sr);
            }

            //Then

            //Assert Equality , Check if 2 instances are equal with the Equals override method!
            Assert.IsTrue(htmClassifier.Equals(htmClassifierNew));

            //Compare Hash values of 2 instances are also equal
            var hashValFirstInstance = htmClassifier.GetHashCode(); // override for GetHashCode  in classifier is called
            Debug.WriteLine(String.Format("Hash value : {0}", hashValFirstInstance));

            var hashValNewInstance = htmClassifierNew.GetHashCode();
            Debug.WriteLine(String.Format("Hash value : {0}", hashValNewInstance));

            //Assert if the two has same Hashcode
            Assert.IsTrue(hashValFirstInstance.Equals(hashValNewInstance));

        }

        /// <summary>
        /// This method is indented to test if the Hash code generated for two different HTMClassifier instance will be different.
        /// This tests the Hash code generated by the overridden GetHasCode() .
        /// second instances is created by deserialization by using pre-saved .txl file to get a different object of Classifier.
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("GetHashCode-Testing")]
        public void HashcodeDifferentWhenUnequalObjectsTest()
        {
            //Given
            //New instance initialised for HTMClassifier to be used in deserialization process.
            HtmClassifier<string, ComputeCycle> htmClassifierNew;

            string E_inFolder = "Classifiers/ClassifierTestsInputs";


            //Serialisation begins and save to a File
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }

            //Test sample .txt file used to create a different instance of HtmClassifier for the test file folder /Classifiers/ClassifierTestsInputs
            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
            {
                htmClassifierNew = htmClassifier.Deserialize(sr);
            }

            //Then

            //Assert Equality , Check if 2 instances are not same but different
            Assert.IsFalse(htmClassifier.Equals(htmClassifierNew));

            //Compare Hash values of 2 instances are also equal
            var hashValFirstInstance = htmClassifier.GetHashCode(); // override for GetHashCode  in classifier is called
            Debug.WriteLine(String.Format("Hash value of Fist instance of HtmClassifier: {0}", hashValFirstInstance));

            var hashValNewInstance = htmClassifierNew.GetHashCode();
            Debug.WriteLine(String.Format("Hash value second instance object: {0}", hashValNewInstance));

            //Assert if the two has different HashCode
            Assert.IsFalse(hashValFirstInstance.Equals(hashValNewInstance));

        }

        /// <summary>
        /// Get HashCode value of the HTMClassifier instance using the override GetHashCode().
        /// Checking if the method returns hashCode.
        /// </summary>  
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("GetHashCode-Testing")]
        public void HashCodeValueTestForAnHtmClassifierObject()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifierInstance;
            // read a test input file and deserialize to get an HtmClassifier instance
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When
            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
            {
                htmClassifierInstance = htmClassifier.Deserialize(sr);
            }

            //Then
            //call GetHashCode method and get hash as return
            var hashValue = htmClassifierInstance.GetHashCode();

            Debug.WriteLine(String.Format("Hash value : {0}", hashValue));

        }



        /// <summary>        
        /// Below we unit test for Equals override method
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenTwoDifferentInstances()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifieractual =new HtmClassifier<string, ComputeCycle>();
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
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
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenTwoDifferentObjectTypePassed()
        {
            //Given
            HtmSerializer htmSerializer = new HtmSerializer();
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
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
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenComparedObjectIsNull()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifieractual = null;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
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
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenParametersAreDifferent()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier2;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
            {
                htmClassifier1 = htmClassifier.Deserialize(sr);
            }

            //using the below test file we set maxRecordedElements as a different value that the actual.
            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassiferObject2.txt"))
            {
                htmClassifier2 = htmClassifier.Deserialize(sr);
            }

            //Then
            Assert.IsFalse(htmClassifier1.Equals(htmClassifier2));
        }

        /// <summary>        
        /// Below we unit test failure of for Equals method if test object when one does not have all class parameters
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenParametersMissingValuesInside()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier3;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
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
        /// Below we unit test failure of for Equals method if test object when one doesnot have all class parameters for m_AllInputs KeyVlauePairs
        /// </summary>        
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Equals-Override-UnitTests")]
        public void TestHtmClassifierEqualsReturnFalseWhenKeyValueParametersDifferent()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifier3 ;
            HtmClassifier<string, ComputeCycle> htmClassifier1;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
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
        /// The below test checks the condition when in a compared instance of Classifier class 
        /// where the parameter m_AllInputs or other.m_AllInput is null
        /// </summary>
        [TestMethod]
        [TestCategory("ProjectUnitTests")]
        [TestCategory("Equals-Override-UnitTests")]
        public void TestEqualsReturnFalseWhenInstancesHasNullParameters()
        {
            //Given
            HtmClassifier<string, ComputeCycle> htmClassifierWithNullParameter;
            HtmClassifier<string, ComputeCycle> htmClassifierActualObject;
            string E_inFolder = "Classifiers/ClassifierTestsInputs";

            //When

            using (StreamReader sr = new StreamReader($"{E_inFolder}\\ExpectedHtmClassifierObjectInputFile.txt"))
            {
                htmClassifierActualObject = htmClassifier.Deserialize(sr);
            }

            //The Test file we read below does not contain m_AllInput params so the Condition in the Equals method would be tested accordingly
            using (StreamReader sr = new StreamReader($"{E_inFolder}\\null_m_AllInput_TestValues.txt"))
            {
                htmClassifierWithNullParameter = htmClassifier.Deserialize(sr);
            }

            //Then

            // passing the null object as compared instance to Equal method.

            var isFirstEqualSecond = htmClassifierActualObject.Equals(htmClassifierWithNullParameter);
            Assert.IsFalse(isFirstEqualSecond);

            //Passsing the instance with null param as reference object in comparison
            var isSecondEqualFirst = htmClassifierWithNullParameter.Equals(htmClassifierActualObject);
            Assert.IsFalse(isSecondEqualFirst);
          
           
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