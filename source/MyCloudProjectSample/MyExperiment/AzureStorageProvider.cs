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
            config = new MyConfig();
            configSection.Bind(config);
        }

        public static string GenerateRandomPartitionKey()
        {
            // Generate a random GUID and use it as the partition key.
            return Guid.NewGuid().ToString();
        }

        public async Task<string> DownloadInputFile(string fileName)
        {
            //BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, "inputblobcontainer");
            BlobServiceClient blobServiceClient = new BlobServiceClient(this.config.StorageConnectionString);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(this.config.TrainingContainer);

            await container.CreateIfNotExistsAsync();

            try
            {

                // Get a reference to a blob named "sample-file"
                BlobClient blob = container.GetBlobClient(fileName);

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

            
            string partitionKey = GenerateRandomPartitionKey();
            int suffixNum = 1;
            

            // Create an instance of the strongly-typed entity and set their properties.

            for (int i = 0; i <results.Count ; i++)
            {
                string rowKey = results[i].ExperimentId + "_" + suffixNum.ToString();

                    var stronglyTypedEntity = new ExperimentResult(partitionKey, rowKey)
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey+ " : "+results[i].TestedSequence,                 
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
              
                    // Add the newly created entity to Azure.
                    await tableClient.AddEntityAsync(stronglyTypedEntity);
                  

            }
               
                //throw new NotImplementedException();

                Console.WriteLine("Uploaded to Table Storage successfully");

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }

        }

        public async Task<byte[]> UploadResultFile(string fileName, byte[] data)
        {
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, this.config.ResultContainer);
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
