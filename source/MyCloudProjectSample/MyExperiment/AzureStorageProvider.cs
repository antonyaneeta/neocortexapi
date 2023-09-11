﻿using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Configuration;
using MyCloudProject.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MyExperiment
{
    public class AzureStorageProvider : IStorageProvider
    {
        private MyConfig config;

        public AzureStorageProvider(IConfigurationSection configSection)
        {
            config = new MyConfig();
            configSection.Bind(config);
        }

        public async Task<string> DownloadInputFile(string fileName)
        {
            //BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, "inputblobcontainer");
            BlobServiceClient blobServiceClient = new BlobServiceClient(this.config.StorageConnectionString);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("inputblobcontainer");

            await container.CreateIfNotExistsAsync();

            try
            {

                // Get a reference to a blob named "sample-file"
                BlobClient blob = container.GetBlobClient(fileName);

                //throw if not exists:
                //blob.ExistsAsync

                // return "../myinputfilexy.csv"
                // Download the blob's contents and save it to a file
                BlobDownloadInfo download = await blob.DownloadAsync();

                //using (FileStream file = File.OpenWrite(fileName))
                //{
                //    download.Content.CopyTo(file);

                //    return file.Name;
                //}



                await blob.DownloadToAsync(fileName);

                return fileName;


            }
            catch (Exception ex)
            {
                throw new NotImplementedException(); 
            }

            // throw new NotImplementedException();
        }

        public async Task UploadExperimentResult(List<IExperimentResult> results )
        {
            

            try { 
            // New instance of the TableClient class
            TableServiceClient tableServiceClient = new TableServiceClient(this.config.StorageConnectionString);

            // New instance of TableClient class referencing the server-side table
            TableClient tableClient = tableServiceClient.GetTableClient(
                tableName: this.config.ResultTable
            );

            await tableClient.CreateIfNotExistsAsync();

            Random rnd = new Random();

            int randomnumber = rnd.Next(0, 1000);
            string tableName = this.config.ResultTable + "table";
            string partitionKey = randomnumber.ToString();
            int suffixNum = 1;
            

            // Create an instance of the strongly-typed entity and set their properties.

            for (int i = 0; i <results.Count ; i++)
            {
                string rowKey = "Experiment" + "_" + suffixNum.ToString();

                    var stronglyTypedEntity = new ExperimentResult(partitionKey, rowKey)
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey,
                        ExperimentId = results[i].Name+ results[i].testedPrediction,
                        Description= results[i].Description,
                        DurationSec = results[i].DurationSec,
                        StartTimeUtc = results[i].StartTimeUtc,
                        EndTimeUtc = results[i].EndTimeUtc,
                        Accuracy = results[i].Accuracy,
                        NormalPredAccuracy = results[i].NormalPredAccuracy
                        

                };
                    suffixNum++;

                    //(ExperimentResult)results;



                    // Add the newly created entity.
                    await tableClient.AddEntityAsync(stronglyTypedEntity);
                  

            }
            
               
                //thrownew NotImplementedExcepton();

                Console.WriteLine("Uploaded to Table Storage successfully");

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }


            //ExperimentResult res = new ExperimentResult("damir", "123")
            //{
            //    Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),

            //    Accuracy = (float)0.5,
            //};


            //await tableClient.UpsertEntityAsync((ExperimentResult)results);

        }

        public async Task<byte[]> UploadResultFile(string fileName, byte[] data)
        {
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, "outblobcontainer-1");
            await container.CreateIfNotExistsAsync();

            try
            {
                // Get a reference to a blob 
                BlobClient blob = container.GetBlobClient(fileName);

                // Upload file data
                await blob.UploadAsync(fileName);


                // Verify we uploaded some content
                BlobProperties properties = await blob.GetPropertiesAsync();
                Console.WriteLine(properties.ToString());
                return data;
            }
            finally
            {
                // await container.DeleteIfExistsAsync();
            }
        }
        }

    
}
