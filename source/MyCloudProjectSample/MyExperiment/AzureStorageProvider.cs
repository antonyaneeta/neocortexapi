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
            BlobContainerClient container = new BlobContainerClient(this.config.StorageConnectionString, "inputblobcontainer");
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

                using (FileStream file = File.OpenWrite(fileName))
                {
                    download.Content.CopyTo(file);

                    return file.Name;
                }


            }
            catch (Exception ex)
            {
                throw new NotImplementedException(); ;
            }

            // throw new NotImplementedException();
        }

        public async Task UploadExperimentResult(IExperimentResult result)
        {
            var client = new TableClient(this.config.StorageConnectionString, this.config.ResultTable);

            await client.CreateIfNotExistsAsync();

            ExperimentResult res = new ExperimentResult("damir", "123")
            {
                //Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),

                Accuracy = (float)0.5,
            };


            await client.UpsertEntityAsync((ExperimentResult)result);

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
