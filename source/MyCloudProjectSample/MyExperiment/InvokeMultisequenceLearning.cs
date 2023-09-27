using NeoCortexApi;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyExperiment
{
    internal class InvokeMultisequenceLearning
    {
        /// <summary>
        /// Runs a multi-sequence learning experiment.
        /// </summary>
        /// <param name="input">A list of input sequences.</param>
        /// <param name="testSequences">The path to the test sequences file.</param>
        /// <param name="outputFileName">The name of the output file for results.</param>
        /// <returns>A list of key-value pairs, where the key is a sequence and the value is a list of accuracies.</returns>
        public static List<KeyValuePair<string, List<double>>> RunMultiSequenceLearningExperiment(List<double[]> input, string testSequences,string outputFileName)
        {
            // Dictionary to store sequences
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //for reference,sample sequence added to Dictionary to be like below
            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            // Create sequences from input data
            for (int i = 0; i < input.Count; i++)
            {
                sequences.Add("S"+(i+1), new List<double>(input[i]));
            }

            // Read test sequences from a file
            var text = File.ReadAllText(testSequences, Encoding.UTF8);
            var trainingData = JsonSerializer.Deserialize<TestData>(text);


            // Prototype for building the prediction engine.
            // Initialize the experiment and get the predictor
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            Predictor serializedPredictor;
            var predictor = experiment.Learn(sequences, outputFileName, out serializedPredictor);

            // Predict the next element for each test sequence
            var acc = trainingData.TestValidation
             .Select(seq => {
                 var predictionResult = PredictNextElement(predictor, seq, serializedPredictor);
                 List<double> accuracyList = new List<double>();
                 accuracyList.Add(predictionResult.Item2);//Predictor accuracy of HTClassifier serialized predictor
                 accuracyList.Add(predictionResult.Item3);
                 return new KeyValuePair<string,List<double>>(string.Join(", ", seq),accuracyList);                           
             }).ToList();

            //a list of key-value pairs, where the key is a sequence and the value is a list of accuracies
            return acc;
        }

        #region the PredictNext element to compare if both the Predictor instance for a HTMCLassifier Serialized one and normal Predictor instance has same predictions.
 
        /// <summary>
        /// Predicts the next elements for a given sequence using two predictors.
        /// </summary>
        /// <param name="predictor">The normal predictor.</param>
        /// <param name="testItem">The input sequence to test.</param>
        /// <param name="serPredictor">The serialized predictor.</param>
        /// <returns>
        /// A tuple containing a list of key-value pairs representing predictions, the serialized predictor accuracy,
        /// and the normal predictor accuracy.
        /// </returns>
        private static Tuple<List<KeyValuePair<String, String>>,double,double> PredictNextElement(Predictor predictor, double[] testItem, Predictor serPredictor)

        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("\n-------------"+"Sequence to test: "+ (string.Join(", ", testItem)) +"-----------------");
            List<KeyValuePair<String, String>> listofPredictions = new List<KeyValuePair<String, String>>();
            int matchCount = 0;
            int totalCount = 0;
            // below for serialized pred accuracy calculation help
            int matchCount1 = 0;
            int totalCount1 = 0;
            foreach (var item in testItem)
            {
                //Checking prediction for next predicted element for each item
                Console.WriteLine($"\n item name : {item}");

                predictor.Reset();
                serPredictor.Reset();
                var res = predictor.Predict(item);
                predictor.Reset();
                serPredictor.Reset();
                var resSerializedPred = serPredictor.Predict(item);

                // Thie section below loops on the prediction for each element in the item for htmClassifier serialized predictor
                if (resSerializedPred.Count > 0)
                {
                    foreach (var pred1 in resSerializedPred)
                    {
                       //Console.WriteLine($"{pred1.PredictedInput} - {pred1.Similarity}");
                    }
                    var tokens = resSerializedPred.First().PredictedInput.Split('_');
                    var tokens2 = resSerializedPred.First().PredictedInput.Split('-');
                    // to print out he predicted sequence use line below
                    //Console.WriteLine($"SerializedPredictor-->  Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                    listofPredictions.Add(new KeyValuePair<String, String>($"item name : {item} : for serialized predictor " + tokens[0], tokens2.Last()));

                    matchCount1 += 1;
                    
                }
                else
                {
                    Console.WriteLine("Nothing predicted for serialized predictor  :(");
                }
                totalCount1 += 1;


                //Print similarity of each element of two predictors
                if (res.Count > 0 && resSerializedPred.Count > 0)
                {
                    var nextElementToken = resSerializedPred.First().PredictedInput.Split('-');
                    var nextElementNormaltoken = res.First().PredictedInput.Split('-');

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"Sequence predicted from Normal Predictor: {res[0].PredictedInput} , next Element: {nextElementNormaltoken.Last()}");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Sequence predicted from Serialized Predictor: {resSerializedPred[0].PredictedInput} , next Element : {nextElementToken.Last()}");
                    Console.ResetColor();
                }

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                       //Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var similarity = res.First().Similarity;
                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    // to print out he predicted sequence use line below
                    // Console.WriteLine($"Normal Predictor--> Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                    matchCount += 1;
                    listofPredictions.
                        Add(new KeyValuePair<String, String>($"item name : {item} : for normal predictor " + tokens[0], tokens2.Last()));
                }
                else
                {
                    Console.WriteLine("Nothing predicted for normal predictor :(");
                }
                    totalCount += 1;
            }

            #region Cleanup- This region is helping DEBUG and console print the next element predictions for each element in the testing item 
            //Commented off now to have a good console print
            //foreach (KeyValuePair<string, string> kvp in listofPredictions)
            //{
            //    if (!IsEmptyKeyValuePair(kvp))
            //    {
                    
            //    Console.WriteLine(string.Format("Key: {0} Value: {1}", kvp.Key, kvp.Value));
            //}
            //}

            // helper method to check if kvp is not Empty
            static bool IsEmptyKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
            {
                // Check if both the key and value are null
                return kvp.Key == null && kvp.Value == null;
            }
            #endregion


            #region Prediction Accuracy Calculation below .
            //Calculate predictorAccuracy
            double predictorAccuracy = matchCount * 100 / (double)totalCount;
            predictorAccuracy= Math.Round(predictorAccuracy, 2);

            double serialisedPredAccuracy = (double)matchCount1 * 100 / (double)totalCount1;
            serialisedPredAccuracy=Math.Round(serialisedPredAccuracy, 2);
            #endregion

            // Print the accuracy for the sequence tested here
            Console.WriteLine("\nFor the List of [" + (string.Join(", ", testItem)) + "] the accuracy calculated as below: ");
            Console.WriteLine("Normal Predictor Accuracy -->"+ predictorAccuracy);
            Console.WriteLine("HTMClassifier serialised Predictor SerializedPredictorAccuracy -->"+ serialisedPredAccuracy);
            Console.WriteLine("------------------------------");
            
            return Tuple.Create(listofPredictions,serialisedPredAccuracy, predictorAccuracy);
        }
    }
    #endregion


    public class TestData
    {
        /// <summary>
        /// Method to Loop and provide the test sequnce from an array of elements.
        /// </summary>
        public List<double[]> TestValidation { get; set; } = new();
    }

}

