using NeoCortexApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment
{
    internal class InvokeMultisequenceLearning
    {
        /// <summary>
        /// RunMultisequence experiment to test serialization of HTM Classifier
        /// </summary>
        /// <param name="input"></param>
        public static void RunMultiSequenceLearningExperiment(double[] input)
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));

            // Create a new sequence "S1" using the azure data.
            //sequences.Add("S1", new List<double>(input));

            sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0, }));
            sequences.Add("S2", new List<double>(new double[] { 8.0, 1.0, 2.0, 9.0, 10.0, 7.0, 11.00 }));

            //
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            Predictor serializedPredictor;
            var predictor = experiment.Run(sequences, out serializedPredictor);

            //
            // These list are used to see how the prediction works.
            // Predictor is traversing the list element by element. 
            // By providing more elements to the prediction, the predictor delivers more precise result.
            //var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
            var list1 = new double[] { 9, 10 };
            // var list1 = new double[] { 2.0, 5.0 };
            var list2 = new double[] { 2.0, 3.0, 4.0 };
            var list3 = new double[] { 8.0, 1.0, 2.0 };

            predictor.Reset();
            serializedPredictor.Reset();
            PredictNextElement(predictor, list1, serializedPredictor);
            //PredictNextElement(serializedPredictor, list1);

            //predictor.Reset();
            //serializedPredictor.Reset();
            //PredictNextElement(predictor, list2);
            //PredictNextElement(predictor, list2, serializedPredictor);


            //predictor.Reset();
            //serializedPredictor.Reset();
            //PredictNextElement(predictor, list3);
            //PredictNextElement(predictor, list3, serializedPredictor);

        }

        #region the PredictNext element to compare if both the serialized Predictor and normal Predictor has same prediction.
        private static void PredictNextElement(Predictor predictor, double[] list, Predictor serPredictor)

        {
            Debug.WriteLine("------------------------------");
            List<KeyValuePair<String, String>> listofPrediction = new List<KeyValuePair<String, String>>();
            int matchCount = 0;
            int totalCount = 0;
            int matchCount1 = 0;
            int totalCount1 = 0;
            foreach (var item in list)
            {
                //Checking prediction for next predicted element for each item
                Console.WriteLine($" item name : {item}");



                var res = predictor.Predict(item);
                var res1 = serPredictor.Predict(item);

                //try to get siliary of each elemnt of two predictions
                //for (int k = 0; k < res.Count; k++)
                //{

                //}

                if (res.Count > 0 && res1.Count > 0)
                {
                    Console.WriteLine($"Comparing the Input predicted from predictor,  {res[0].PredictedInput} : and from serializedPredictor: {res1[0].PredictedInput}");
                }


                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Console.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }


                    var similarity = res.First().Similarity;

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Console.WriteLine($"From actualPredictor--> Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");

                    matchCount += 1;

                    listofPrediction.
                        Add(new KeyValuePair<String, String>($"item name : {item} : for normal predictor " + tokens[0], tokens2.Last()));
                }
                else
                    Console.WriteLine("Nothing predicted :(");
                    totalCount += 1;
                

                if (res1.Count > 0)
                {
                    foreach (var pred1 in res1)
                    {
                        Console.WriteLine($"{pred1.PredictedInput} - {pred1.Similarity}");
                    }
                    var similarity1 = res.First().Similarity;

                    var tokens = res1.First().PredictedInput.Split('_');
                    var tokens2 = res1.First().PredictedInput.Split('-');
                    Console.WriteLine($"\"SerializedPredictor-->  Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                    listofPrediction.Add(new KeyValuePair<String, String>($"item name : {item} : for serialized predictor " + tokens[0], tokens2.Last()));

                    matchCount1 += 1;
                }
                else
                    Console.WriteLine("Nothing predicted :(");
                    totalCount1 += 1;
                
            }
            foreach (KeyValuePair<string, string> kvp in listofPrediction)
            {
                Console.WriteLine(string.Format("Key: {0} Value: {1}", kvp.Key, kvp.Value));
            }

            // Calculate predictorAccuracy
            var predictorAccuracy = (matchCount * 100) / totalCount;
            var serialisedPredAccuracy = (matchCount1 * 100) / totalCount1;
            // Create a Multisequence object and set predictorAccuracy



            Console.WriteLine("------------predictorAccuracy------------------"+ predictorAccuracy);
            Console.WriteLine("------------serialisedpredictorAccuracy------------------"+ serialisedPredAccuracy);
            Console.WriteLine(Boolean.Equals(serialisedPredAccuracy, predictorAccuracy));
            Debug.WriteLine("------------------------------");
        }
    }
    #endregion
}

