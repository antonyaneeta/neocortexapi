using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using HtmClassifierUnitTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyExperiment
{
    /// <summary>
    /// This class implements the ML experiment that will run in the cloud. This is refactored code from my SE project.
    /// </summary>
    public class Experiment : IExperiment
    {
        private IStorageProvider storageProvider;

        private ILogger logger;

        private MyConfig config;
        private HtmClassifier<string, ComputeCycle> htmClassifier;

        private Dictionary<string, List<double>> sequences;

        //
        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log)
        {
            this.storageProvider = storageProvider;
            this.logger = log;

            config = new MyConfig();
            configSection.Bind(config);
        }

        public Task<IExperimentResult> Run(string inputFile)
        {


            // TODO read file

            // YOU START HERE WITH YOUR SE EXPERIMENT!!!!

            ExperimentResult res = new ExperimentResult(this.config.GroupId, null);

            res.StartTimeUtc = DateTime.UtcNow;

            //Run your experiment code here.

            // // Serialization check.

            double pdValue;
            double[] pdValues;

            var values = File.ReadAllLines(inputFile)
                       .SelectMany(a => a.Split(';')
                       .Select(str => double.TryParse(str, out pdValue) ? pdValue : 0));

            // input double array to be passed to RunMultisequence learningExp to the the predict method

            pdValues = values.ToArray();


            // The actual lerning and predict method call of our HTM Serialize below

            RunMultiSequenceLearningExperiment(pdValues);


            //res.OutputFiles("SerialiseOutput.txt");



            return Task.FromResult<IExperimentResult>(res); // TODO...
        }




        /// <inheritdoc/>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {


            QueueClient queueClient = new QueueClient(this.config.StorageConnectionString, this.config.Queue);

            
            while (cancelToken.IsCancellationRequested == false)
            {
                QueueMessage message = await queueClient.ReceiveMessageAsync();

                if (message != null)
                {
                    try
                    {

                        string msgTxt = Encoding.UTF8.GetString(message.Body.ToArray());

                        this.logger?.LogInformation($"Received the message {msgTxt}");

                        ExerimentRequestMessage request = JsonSerializer.Deserialize<ExerimentRequestMessage>(msgTxt);

                        var inputFile = await this.storageProvider.DownloadInputFile(request.InputFile);

                        IExperimentResult result = await this.Run(inputFile);

                        //TODO. do serialization of the result.
                        var fileBytes = File.ReadLines("output.txt");


                        //uploaded the serialised output text file to the blob
                        await storageProvider.UploadResultFile("output.txt", null);

                       //TO DO---> Correct uploading the response accuracy of the predictor of the serialised one as well as the original one
                       //await storageProvider.UploadExperimentResult(result);

                        //await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                    catch (Exception ex)
                    {
                        this.logger?.LogError(ex, "TODO...");
                    }
                }
                else
                {
                    await Task.Delay(500);
                    logger?.LogTrace("Queue empty...");
                    
                }
            }

            this.logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }



        #region RunMultisequnce experiment to test serialization of HTM Classifier
        private static void RunMultiSequenceLearningExperiment(double[] input)
        {
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            //sequences.Add("S1", new List<double>(new double[] { 0.0, 1.0, 0.0, 2.0, 3.0, 4.0, 5.0, 6.0, 5.0, 4.0, 3.0, 7.0, 1.0, 9.0, 12.0, 11.0, 12.0, 13.0, 14.0, 11.0, 12.0, 14.0, 5.0, 7.0, 6.0, 9.0, 3.0, 4.0, 3.0, 4.0, 3.0, 4.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 0.0, 3.0, 3.0, 4.0, 5.0, 6.0, 5.0, 7.0, 2.0, 7.0, 1.0, 9.0, 11.0, 11.0, 10.0, 13.0, 14.0, 11.0, 7.0, 6.0, 5.0, 7.0, 6.0, 5.0, 3.0, 2.0, 3.0, 4.0, 3.0, 4.0 }));
            
            // trying with input from azure as one sequnce value.
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
            var list1 = new double[] { 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 };
            var list2 = new double[] { 2.0, 3.0, 4.0 };
            var list3 = new double[] { 8.0, 1.0, 2.0 };

            predictor.Reset();
            PredictNextElement(predictor, list1,serializedPredictor);
          //  PredictNextElement(serializedPredictor, list1);

            //predictor.Reset();
            //PredictNextElement(predictor, list2);

            //predictor.Reset();
            //PredictNextElement(predictor, list3);
        }

        private double[] ReadCsvValues(string filePath)
        {
            double pdValue;
            double[] pdValues;

            var values = File.ReadAllLines(filePath)
                .SelectMany(a => a.Split(';')
                .Select(str => double.TryParse(str, out pdValue) ? pdValue : 0));

            pdValues = values.ToArray();

            return pdValues;
        }

        private static void PredictNextElement(Predictor predictor, double[] list, Predictor serPredictor)

        {
            Debug.WriteLine("------------------------------");

            foreach (var item in list)
            {
                var res = predictor.Predict(item);
                var res1 = serPredictor.Predict(item);


                Console.WriteLine($"Comparing the Input predicted from predictor,  {res[0].PredictedInput} : and from serializedPredictor: {res1[0].PredictedInput}");
           

                if (res.Count > 0)
                {
                    foreach (var pred in res)
                    {
                        Debug.WriteLine($"{pred.PredictedInput} - {pred.Similarity}");
                    }

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Debug.WriteLine($"Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");
                }
                else
                    Debug.WriteLine("Nothing predicted :(");
            }

            Debug.WriteLine("------------------------------");
        }
    }
    #endregion

}

