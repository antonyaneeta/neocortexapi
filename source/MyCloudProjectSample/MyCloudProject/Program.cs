using MyCloudProject.Common;
using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using MyExperiment;
using System.Threading.Tasks;

namespace MyCloudProject
{
    class Program
    {
        /// <summary>
        /// Your project ID from the last semester.
        /// </summary>
        private static string projectName = "ML22/23-9 Implement Serialization of HtmClassifier -Team Alpha";
        private static string projectDesc = "Use the HTMClassifier SerialializedPredictor to predict next element of the test sequence with accuracy" +
            " and compare with normalPredictor and find out to be equally accurate.";


        static async Task Main(string[] args)
        {
            CancellationTokenSource tokeSrc = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tokeSrc.Cancel();
            };

            Console.WriteLine($"Started experiment: {projectName}");

            //init configuration
            ///-------------------------------
            var cfgRoot = Common.InitHelpers.InitConfiguration(args);

            var cfgSec = cfgRoot.GetSection("MyConfig");

            // InitLogging
            var logFactory = InitHelpers.InitLogging(cfgRoot);
            var logger = logFactory.CreateLogger("Train.Console");

            logger?.LogInformation($"{DateTime.Now} -  Started experiment: {projectName}");

            IStorageProvider storageProvider = new AzureStorageProvider(cfgSec);

            // Create an experiment request message
            IExerimentRequestMessage expReq = new ExerimentRequestMessage();
            expReq.Name = projectName;
            expReq.Description = projectDesc;

            ///create Experiment and pass all required and additional config params
            Experiment experiment = new Experiment(cfgSec, storageProvider, logger ,expReq);
            
            await experiment.RunQueueListener(tokeSrc.Token);

            logger?.LogInformation($"{DateTime.Now} -  Experiment exit: {projectName}");
        }


    }
}
