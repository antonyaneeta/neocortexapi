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
        /// RunMultisequence experiment to test serialization of HTM Classifier
        /// </summary>
        /// <param name="input"></param>
        public static List<KeyValuePair<string, List<double>>> RunMultiSequenceLearningExperiment(List<double[]> input, string testSequences,string outputFileName)
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            // Create a new sequence "S1" using the input form  Azure  storage container data.
            for (int i = 0; i < input.Count; i++)
            {
                sequences.Add("S"+(i+1), new List<double>(input[i]));
            }
          

            // sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
            // sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));




            // These testItem are used to see how the prediction works.
            // Predictor is traversing the testItem element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            var text = File.ReadAllText(testSequences, Encoding.UTF8);
            var trainingData = JsonSerializer.Deserialize<TestData>(text);


            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            Predictor serializedPredictor;
            var predictor = experiment.Learn(sequences, outputFileName, out serializedPredictor);

            
            //predictor.Reset();
            //serializedPredictor.Reset();
            Tuple<List<KeyValuePair<string, string>>, double,double> tuple = null;
            //tuple=PredictNextElement(predictor, list1, serializedPredictor);

            List<KeyValuePair<string, List<double>>> kvp = new List<KeyValuePair<string, List<double>>> ();

            //foreach (var testInp in trainingData.TestValidation)
            //{
            //    predictor.Reset();
            //    serializedPredictor.Reset();
            //    tuple = PredictNextElement(predictor, testInp, serializedPredictor);
            //    List<KeyValuePair<string, string>> item1 = tuple.Item1;

            //    kvp.Add(new KeyValuePair<string, float>(string.Join(", ", testInp), tuple.Item2));

            //}


            // below code we tet he prediction with the test downloaded from Azure container
            //ie. we check for each predictionItem in the accuracyList 

            // These testItem are used to see how the prediction works.
            // Predictor is traversing the testItem element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.


            var acc = trainingData.TestValidation
             .Select(seq => {
                 predictor.Reset();
                 serializedPredictor.Reset();
                 var predictionResult = PredictNextElement(predictor, seq, serializedPredictor);
                 List<double> accuracyList = new List<double>();
                 accuracyList.Add(predictionResult.Item2);
                 accuracyList.Add(predictionResult.Item3);
                 return new KeyValuePair<string,List<double>>(string.Join(", ", seq),accuracyList);
                // var accuracy = predictionResult.Item2;               
             }).ToList();



            //predictor.Reset();
            //serializedPredictor.Reset();
            //PredictNextElement(predictor, list2);
            //PredictNextElement(predictor, list2, serializedPredictor);


            //predictor.Reset();
            //serializedPredictor.Reset();
            //PredictNextElement(predictor, list3);
            //PredictNextElement(predictor, list3, serializedPredictor);


            //var v= tuple.Item2;

           
            return acc;
        }

        #region the PredictNext element to compare if both the serialized Predictor and normal Predictor has same prediction.
        /// <summary>
        /// This is now returning the serialized Predictors accuracy in predicting the next element correct as normal Predictor
        /// </summary>
        /// <param name="predictor"></param>
        /// <param name="testItem"></param>
        /// <param name="serPredictor"></param>
        /// <returns></returns>
        private static Tuple<List<KeyValuePair<String, String>>,double,double> PredictNextElement(Predictor predictor, double[] testItem, Predictor serPredictor)

        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("-------------"+"Sequence to test: "+ (string.Join(", ", testItem)) +"-----------------");
            List<KeyValuePair<String, String>> listofPredictions = new List<KeyValuePair<String, String>>();
            int matchCount = 0;
            int totalCount = 0;
            int matchCount1 = 0;
            int totalCount1 = 0;
            foreach (var item in testItem)
            {
                //Checking prediction for next predicted element for each item
                Console.WriteLine($" item name : {item}");

                var resSerializedPred = serPredictor.Predict(item);
                var res = predictor.Predict(item);
               



                // Thie section below loops on the prediction for each element in the item for htmClassifier serialized predictor

                if (resSerializedPred.Count > 0)
                {
                    foreach (var pred1 in resSerializedPred)
                    {
                       //Console.WriteLine($"{pred1.PredictedInput} - {pred1.Similarity}");
                    }

                    var tokens = resSerializedPred.First().PredictedInput.Split('_');
                    var tokens2 = resSerializedPred.First().PredictedInput.Split('-');
                    Console.WriteLine($"SerializedPredictor-->  Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                    listofPredictions.Add(new KeyValuePair<String, String>($"item name : {item} : for serialized predictor " + tokens[0], tokens2.Last()));

                    matchCount1 += 1;
                    Console.WriteLine(matchCount1 + "in serialized updated");
                    
                }
                else
                {
                    Console.WriteLine("Nothing predicted for serialized predictor  :(");
                }
                totalCount1 += 1;
                Console.WriteLine(totalCount1 + "total count in serialized updated");


                //try to get similarity of each element of two predictors

                if (res.Count > 0 && resSerializedPred.Count > 0)
                {
                    Console.WriteLine($"Comparing the Sequence predicted from normal predictor:  {res[0].PredictedInput} and from serializedPredictor: {resSerializedPred[0].PredictedInput}");
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
                    Console.WriteLine($"Normal Predictor--> Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");

                    matchCount += 1;
                    Console.WriteLine(totalCount1 + "match count in normal");

                    listofPredictions.
                        Add(new KeyValuePair<String, String>($"item name : {item} : for normal predictor " + tokens[0], tokens2.Last()));
                }
                else
                {
                    Console.WriteLine("Nothing predicted for normal predictor :(");
                }
                    totalCount += 1;
                Console.WriteLine(totalCount1 + "total t in normal "); 

              

            }


            Console.WriteLine("------------accuracyList of  kvp------------------" + listofPredictions.Count);
            foreach (KeyValuePair<string, string> kvp in listofPredictions)
            {
                if (!IsEmptyKeyValuePair(kvp))
                {
                    
                Console.WriteLine(string.Format("Key: {0} Value: {1}", kvp.Key, kvp.Value));
            }
            }

            static bool IsEmptyKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> kvp)
            {
                // Check if both the key and value are null
                return kvp.Key == null && kvp.Value == null;
            }

            // Calculate predictorAccuracy

            Console.WriteLine("------------matchCount final------------------" + matchCount);
            Console.WriteLine("------------totalCount final------------------" + totalCount);
            //double accuracy = (double)matches / (double)sequenceKeyPair.Value.Count * 100.0;
            double predictorAccuracy = matchCount * 100 / (double)totalCount;
            Console.WriteLine("------------matchCount ser final------------------" + matchCount1);
            Console.WriteLine("------------totalCount ser final------------------" + totalCount1);
            double serialisedPredAccuracy = (double)matchCount1 * 100 / (double)totalCount1;
            // Create a Multisequence object and set predictorAccuracy



            Console.WriteLine("------------predictorAccuracy------------------"+ predictorAccuracy);
            Console.WriteLine("------------serialisedpredictorAccuracy------------------"+ serialisedPredAccuracy);
            //Console.WriteLine(Boolean.Equals(serialisedPredAccuracy, predictorAccuracy));
            Console.WriteLine("------------------------------");
            
            return Tuple.Create(listofPredictions,serialisedPredAccuracy, predictorAccuracy);
        }
    }
    #endregion
    public class TestData
    {
        public List<double[]> TestValidation { get; set; } = new();
    }

}

