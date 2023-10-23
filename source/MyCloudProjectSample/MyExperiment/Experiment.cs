using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCloudProject.Common;
using System;
using System.Collections.Generic;
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
        private IExerimentRequestMessage experimentRequest;
        private ExerimentRequestMessage request;

        /// <summary>
        /// Initializes a new instance of the Experiment class.
        /// </summary>
        /// <param name="configSection">The configuration section containing settings for the experiment.</param>
        /// <param name="storageProvider">The storage provider used for accessing storage resources.</param>
        /// <param name="log">The logger used for logging experiment-related information.</param>
        /// <param name="expReq">The experiment request message containing details of the experiment.</param>
        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log, IExerimentRequestMessage expReq)
        {
            this.storageProvider = storageProvider;
            this.logger = log;
            this.experimentRequest = expReq;
            // Bind the configuration section to the config object
            this.config = new MyConfig();
            configSection.Bind(config);

        }

        /// <summary>
        /// Runs the experiment with the provided input data and test sequences.
        /// </summary>
        /// <param name="inputFile">The path to the input CSV file.</param>
        /// <param name="testSequences">The test sequences for prediction.</param>
        /// <param name="outputFileName">The name of the output file for serialized results.</param>
        /// <returns>A task representing the list of experiment results.</returns>
        public Task<List<IExperimentResult>> Run(string inputFile, string testSequences, string outputFileName)
        {
            // Initialize a list to store experiment results
            List<IExperimentResult> resultList = new List<IExperimentResult>();
            // Create an initial experiment result object
            ExperimentResult res = new ExperimentResult(config.GroupId, null);
            res.StartTimeUtc = DateTime.UtcNow;

            // read csv file for input file to get sequence for experiment
            List<double[]> pdValues;
            pdValues = ReadCsvValues(inputFile);

            // Run the Multisequence Learning Experiment predict sequence for the HTMSerialize class below
            var experimentResult = InvokeMultisequenceLearning.RunMultiSequenceLearningExperiment(pdValues, testSequences, outputFileName);

            // Process the experiment results to upload azure table result
            foreach (var item in experimentResult)
            {
                // Create a new experiment result object for each result
                ExperimentResult response = new ExperimentResult(config.GroupId, null);
                double accuracy = item.Value[0];
                double normalPredictorAcc = item.Value[1];
                string testedPredList = item.Key;

                // Set experiment result properties
                response.EndTimeUtc = DateTime.UtcNow;
                var elapsedTime = response.EndTimeUtc - res.StartTimeUtc;
                response.SerializedPredictorAccuracy = accuracy;
                response.NormalPredAccuracy = normalPredictorAcc;
                response.TestedSequence = testedPredList;
                response.DurationSec = (long)elapsedTime.GetValueOrDefault().TotalSeconds;
                response.Name = experimentRequest.Name;
                response.Description = experimentRequest.Description;
                response.ExperimentId = request.ExperimentId;
                response.OutputFiles = outputFileName;
                response.InputFileUrl = inputFile;
                response.Timestamp = DateTime.Now;

                // Add each result item to the experiment result list
                resultList.Add(response);
            }
            // Return the list of experiment results as a completed task
            return Task.FromResult(resultList);
        }


        /// <summary>
        /// Runs the queue listener asynchronously.
        /// </summary>
        /// <param name="cancelToken">Cancellation token to stop the listener.</param>
        public async Task RunQueueListener(CancellationToken cancelToken)
        {
            // Initialize a QueueClient for processing messages from a queue
            QueueClient queueClient = new QueueClient(config.StorageConnectionString, config.Queue);

            // Continuously process messages until cancellation is requested
            while (!cancelToken.IsCancellationRequested)
            {
                // Receive a message from the queue
                QueueMessage message = await queueClient.ReceiveMessageAsync();
                if (message != null)
                {
                    try
                    {
                        // Convert the message body to text
                        string msgTxt = Encoding.UTF8.GetString(message.Body.ToArray());
                        // Log the received message
                        logger?.LogInformation($"Received the message from Azure Container Queue:\n{msgTxt}");

                        // Deserialize the received message into an Experiment Request object
                        request = JsonSerializer.Deserialize<ExerimentRequestMessage>(msgTxt);

                        // Download input files from Azure Containers
                        var inputFile = await storageProvider.DownloadInputFile(request.InputFile);
                        var testSequenceFile = await storageProvider.DownloadInputFile(request.TestInputFile);
                        // Get a unique output file name for serialized text saving in the experiment
                        string outputFileName = createOutputFile();

                        // Execute the actual experiment
                        List<IExperimentResult> results = await Run(inputFile, testSequenceFile, outputFileName);

                        // Upload the serialized output text file for the experiment to blob storage
                        await storageProvider.UploadResultFile(outputFileName, null);
                        // Upload the accuracy of the predictor from the experiment to Azure Table
                        await storageProvider.UploadExperimentResult(results);

                        // Log success and remove the processed queue message
                        logger?.LogInformation("Uploaded the Experiment result to Azure Table");
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                    catch (Exception ex)
                    {
                        // Log errors during experiment execution
                        logger?.LogError(ex, "Something went wrong while running the experiment");
                    }
                }
                else
                {
                    // Wait for a short time if the queue is empty
                    await Task.Delay(500);
                    logger?.LogTrace("Queue empty...");
                }
            }

            // Log when cancellation is requested and exit the listener loop
            logger?.LogInformation("Cancel pressed. Exiting the listener loop.");
        }


        /// <summary>
        /// A simple method to create a unique output file name with dateTime for each experiment run.
        /// </summary>
        /// <returns></returns>
        private static string createOutputFile()
        {
            //Create a unique name for the output serialized file we keep for future refence in each experiment.
            return "output" + $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}" + ".txt";
        }

        /// <summary>
        /// method to read the csv input file adn generate a readable format and hence to be input as the sequence for mutisequence experiment
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        #region the region userd to read csv data to sequenc array as input for the Multisequnce learning experiment
        private List<double[]> ReadCsvValues(string filePath)
        {
            double pdValue;
            List<double[]> pdValues = new List<double[]>();
            string[] list = File.ReadAllLines(filePath);
            foreach (var fileListLine in list)
            {
                var values = fileListLine.Split(';')
                .Select(str => double.TryParse(str, out pdValue) ? pdValue : 0);
                pdValues.Add(values.ToArray());
            }
            return pdValues;
        }
        #endregion


    }



}

