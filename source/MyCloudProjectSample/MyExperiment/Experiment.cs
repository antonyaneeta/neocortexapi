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

            // read csv file for input sequnce to be passed to experiment
            double[] pdValues;
            
            // input double array to be passed to RunMultisequence learningExp to the the predict method

            pdValues = ReadCsvValues(inputFile);         


            // The actual learning and predict method call of our HTM Serialize below

            InvokeMultisequenceLearning.RunMultiSequenceLearningExperiment(pdValues);


            //res.OutputFiles("SerialiseOutput.txt");



            return Task.FromResult<IExperimentResult>(res); // TODO...
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
                       await storageProvider.UploadExperimentResult(result);

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


}

       

}

