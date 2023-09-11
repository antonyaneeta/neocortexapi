using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCloudProject.Common;
using NeoCortexApi;
using NeoCortexApi.Classifiers;
using NeoCortexApi.Entities;

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

        private IExerimentRequestMessage exerimentRequest;
        //
        public Experiment(IConfigurationSection configSection, IStorageProvider storageProvider, ILogger log, IExerimentRequestMessage expReq)
        {
            this.storageProvider = storageProvider;
            this.logger = log;
            this.exerimentRequest=expReq;
            config = new MyConfig();
            configSection.Bind(config);
        }


        public Task<List<IExperimentResult>> Run(string inputFile,string testSequences,string outputFileName)
        {


            // TODO read file

            // YOU START HERE WITH YOUR SE EXPERIMENT!!!!
            List<IExperimentResult> resultList = new List<IExperimentResult>();
            ExperimentResult res = new ExperimentResult(this.config.GroupId, null);

            res.StartTimeUtc = DateTime.UtcNow;

            //Learn your experiment code here.

            // // Serialization check.

            // read csv file for input sequnce to be passed to experiment
            List<double[]> pdValues;
            
            // input double array to be passed to RunMultisequence learningExp to the the predict method

            pdValues = ReadCsvValues(inputFile);


            // The actual learning and predict method call of our HTM Serialize below

            var v = InvokeMultisequenceLearning.RunMultiSequenceLearningExperiment(pdValues, testSequences,outputFileName);

            //Get List of results for multiple sequence and loop as result to azure table result
            //var resultArr = InvokeMultisequenceLearning.RunMultiSequenceLearningExperiment(pdValues);
            foreach (var item in v)
            {
                double accuracy = item.Value[0];
                double normalPredictorAcc= item.Value[1];
                string testedPredList=item.Key.ToString();
                //string nextElem = item.nextElement;
                res.EndTimeUtc = DateTime.UtcNow;
               
                var elapsedTime = res.EndTimeUtc - res.StartTimeUtc;
                res.DurationSec = (long)elapsedTime.GetValueOrDefault().TotalSeconds;
                res.Accuracy = accuracy;
                res.NormalPredAccuracy = normalPredictorAcc;
                res.testedPrediction=testedPredList;
                res.Name = this.exerimentRequest.Name;
                res.Description = this.exerimentRequest.Description;
                //res.testFileUrl = this.expReq.testFile;
                //res.InputSequenceFileUrl = this.expReq.InputFile;
                res.Timestamp = DateTime.Now;

                resultList.Add(res);
            }


            //res.Accuracy = accuracy;
            //res.OutputFiles("SerialiseOutput.txt");



            return Task.FromResult (resultList); // TODO...
        }


        private List<double[]> ReadCsvValues(string filePath)
        {
            double pdValue;
            List <double[]> pdValues = new List<double[]>();
            string[] list = File.ReadAllLines(filePath);
            foreach (var fileListLine in list)
            {
                var values = fileListLine.Split(';')
                .Select(str => double.TryParse(str, out pdValue) ? pdValue : 0);

                pdValues.Add(values.ToArray());

            }
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
                        var testSequenceFile = await this.storageProvider.DownloadInputFile(request.TestInputFile);

                        var outputFileName = "output" + $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}" + ".txt";

                        List<IExperimentResult> results = await this.Run(inputFile,testSequenceFile, outputFileName);

                    
                        //uploaded the serialised output text file for the experiment to the blob
                       await storageProvider.UploadResultFile(outputFileName, null);

                        
                       //Correctly uploading the response accuracy of the predictor of the serialised one as well as the original one
                       await storageProvider.UploadExperimentResult(results);

                       await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
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

