using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using HtmClassifierUnitTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.IO;
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


            //htmClassifier = new HtmClassifier<string, ComputeCycle>();

            //sequences = new Dictionary<string, List<double>>();
            //sequences.Add("S1", new List<double>(new double[] { 0.9, 1.0, 2.0, 3.0, 4.0, 2.0, 5.0 }));
            //sequences.Add("S2", new List<double>(new double[] { 0.8, 2.0, 3.0, 4.0, 5.0, 2.0, 5.0 }));

            //HtmClassiferSerializationTests htmClassiferSerializationTests = new HtmClassiferSerializationTests();

            //htmClassiferSerializationTests.LearnHtmClassifier();

            LearnHtmClassifierForSerialization learnHtmClassifierForSerialization = new LearnHtmClassifierForSerialization();
           htmClassifier= learnHtmClassifierForSerialization.Train();


            using (StreamWriter sw = new StreamWriter("SerialiseOutput.txt"))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }


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
                        await storageProvider.UploadResultFile("outputfile.txt", null);

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


        #region Private Methods


        #endregion
    }
}
