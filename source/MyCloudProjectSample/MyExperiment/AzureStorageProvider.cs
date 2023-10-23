using Azure;
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
            this.config = new MyConfig();
            configSection.Bind(this.config);
        }

        /// <summary>
        /// Generates a random GUID and uses it as a partition key.
        /// </summary>
        /// <returns>A random GUID string used as a partition key.</returns>
        public static string GenerateRandomPartitionKey()
        {
            // Generate a random GUID and use it as the partition key.
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Downloads an input file from blob storage and saves it locally.
        /// </summary>
        /// <param name="fileName">The name of the file to download.</param>
        /// <returns>The local file path where the downloaded file is saved.</returns>
        public async Task<string> DownloadInputFile(string fileName)
        {
            // Create a BlobServiceClient instance for interacting with blob storage
            BlobServiceClient blobServiceClient = new BlobServiceClient(config.StorageConnectionString);
            // Get the BlobContainerClient for the specified training files container from our azure
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(config.TrainingContainer);
            // Create the container if it doesn't exist
            await container.CreateIfNotExistsAsync();

            try
            {
                // Get a reference to the "teamalpha-training-files" blob 
                BlobClient blob = container.GetBlobClient(fileName);
                // Download the blob's contents and save it to a file with the same name
                BlobDownloadInfo download = await blob.DownloadAsync();
                await blob.DownloadToAsync(fileName);

                // Return the local file path where the downloaded file is saved
                return fileName;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// Uploads a list of experiment results to an Azure Table Storage.
        /// </summary>
        /// <param name="results">A list of experiment results to be uploaded.</param>
        public async Task UploadExperimentResult(List<IExperimentResult> results)
        {
            try
            {
                // Create a TableServiceClient instance for interacting with Azure Table Storage
                TableServiceClient tableServiceClient = new TableServiceClient(config.StorageConnectionString);

                // Create a TableClient instance referring to the specific Azure Table
                TableClient tableClient = tableServiceClient.GetTableClient(tableName: config.ResultTable);

                // Create the Azure Table if it doesn't exist
                await tableClient.CreateIfNotExistsAsync();
                // Generate a random partition key for storing the results
                string partitionKey = GenerateRandomPartitionKey();
                int suffixNum = 1;

                // Iterate through the list of results and upload each one to Azure Table Storage
                for (int i = 0; i < results.Count; i++)
                {
                    string rowKey = results[i].ExperimentId + "_" + suffixNum.ToString();
                    var stronglyTypedEntity = new ExperimentResult(partitionKey, rowKey)
                    {
                        // Set the properties of the strongly-typed entity
                        PartitionKey = partitionKey,
                        RowKey = rowKey + " : " + results[i].TestedSequence,
                        DurationSec = results[i].DurationSec,
                        SerializedPredictorAccuracy = results[i].SerializedPredictorAccuracy,
                        NormalPredAccuracy = results[i].NormalPredAccuracy,
                        StartTimeUtc = results[i].StartTimeUtc,
                        EndTimeUtc = results[i].EndTimeUtc,
                        ExperimentId = results[i].ExperimentId,
                        Name = results[i].Name,
                        Description = results[i].Description,
                        OutputFiles = results[i].OutputFiles,
                        InputFileUrl = results[i].InputFileUrl
                    };
                    suffixNum++;

                    // Add the newly created entity to Azure Table Storage
                    await tableClient.AddEntityAsync(stronglyTypedEntity);


                }
                Console.WriteLine("Uploaded to Table Storage successfully");
            }
            catch (Exception ex)
            {
                // Handle exceptions and log error messages
                Console.Error.WriteLine(ex.ToString(),"Something went wrong during table upload operation");
            }

        }

        /// <summary>
        /// Uploads a result file to a blob storage container.
        /// </summary>
        /// <param name="fileName">The name of the file to be uploaded.</param>
        /// <param name="data">The file data to be uploaded.</param>
        /// <returns>A byte array representing the uploaded file data.</returns>
        public async Task<byte[]> UploadResultFile(string fileName, byte[] data)
        {
            // Create a BlobContainerClient for the result container
            BlobContainerClient container = new BlobContainerClient(config.StorageConnectionString, config.ResultContainer);
            // Create the container if it doesn't exist
            await container.CreateIfNotExistsAsync();

            try
            {
                // Get a reference to a blob within the container
                BlobClient blob = container.GetBlobClient(fileName);

                // Upload the file data to the blob
                await blob.UploadAsync(fileName);

                // Verify that content has been uploaded
                BlobProperties properties = await blob.GetPropertiesAsync();
                Console.WriteLine(properties.ToString());

                return data;
            }
            finally
            {
                // Cleanup (Only enable the below line of code if a delete operation is required)
                // await container.DeleteIfExistsAsync();
            }
        }
    }


}
