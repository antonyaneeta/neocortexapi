# Azure Cloud Implementation ML22/23-9 Implement Serialization of HtmClassifier -Team Alpha 


## Table of Contents
1. [Introduction](#introduction)
2. [SE Project Links](#important-links-se-project)
3. [Goal](#goal-of-cloud-experiment)
4. [Cloud Architecture](#cloud-architecture)
5. [Cloud Project Links](#cloud-project-links)
6. [Our Experiment Description](#our-experiment-description)   
7. [Azure Links](#azure-links)
8. [Azure Container Registry Details](#aure-container-registry-details)
9. [How to Run Project in Azure](#how-to-run-project-in-azure)
10. [Azure Deployment Steps](#create-and-deploy-docker-image)



## Introduction:

In the Software Engineering project, Serialization and Deserialization feature was implimented for HTMClassifier Class in NeoCortexAPI.

- The implementation of Serialize() method:
It convert the parameter of the HTMCLassifier instance to a binary stream and save it to a Output text file. Presently the two valid parameters of HTMClassifier is serialized namely
    1) maxRecordedElements[is of datatype integer]  
    2) m_allInputs, a KeyValuePair type dictionary object. 

- In Deserilaize() Method : 
The serilaized output file can be used as input to the Deserialize method to transform the stream of bytes back into an recreate the HTM Classifer Instance with previous data. All Parameters are recreated with previous data.

This implementation is well verified using multiple testcases in the project. 


## Important Links SE Project
For more Details of SE Project, Refer Readme.md :- [Readmefile](MySEProject/README.md) 
<br/> A SE Project Documentation , available for Refence :- [Project PDF](<MySEProject/Implement Serialisation of HTMClassifier.pdf>)
<br/> Serialize Method Code :- [Serialize()](https://github.com/antonyaneeta/neocortexapi/blob/8ff24574d812556e8a9d6d8794b6613e80b2a606/source/NeoCortexApi/Classifiers/HtmClassifier.cs#L509)
<br/> Deserialize Method Code :- [DeSerialize()](https://github.com/antonyaneeta/neocortexapi/blob/8ff24574d812556e8a9d6d8794b6613e80b2a606/source/NeoCortexApi/Classifiers/HtmClassifier.cs#L529)
<br/> Unit Tests :-[Tests Executed](../../UnitTestsProject/Classifiers/HtmClassiferSerializationTests.cs)


## Goal Of Cloud Experiment
Aim of this Azure Cloud implementation of Software Enginenering project to have the project to be up and running in Cloud enviornment.
<br/>The Entire Experiment for Azure Cloud Project is created as a Docker image. This is Deployed to Azure Container Registry Repository, and then Run in Cloud.

Towards achiving this:-    
- An Experimnt will be Run with help of MultisequnceLearning(**MSL**). The Mutisequnce Learning Run Method helps in Learn and Predict elemnts for a Classifier .(In our Case HTMClassifer Class).
- The cloud project main method invokes this MSL Experiment , and in subsequnt steps verify that:
  - NeoCortexAPI.Predictor from a serialized and deserialized created HTMCLassifier instance( will be refered as **SerializedPredictor**) Predicts next element for any given test sequnce.
  - And similarly NeoCortexAPI.Predictor of a simple normal HTMClassifier instance (**Normal Predictor**) Predicts next element for the same test sequnce.
  - Both the above Predictions of next element in sequnce is validated to be same and matching with accuracy of result returned from the Multisequnce method.<br/>


## Cloud Architecture
![Cloud Project Overview](Cloud-Project-Architecture-Team-Alpha.png)


## Cloud Project Links

[My CloudProject Main Method Program.cs ](../MyCloudProject/Program.cs)

[Cloud Experiment](../MyExperiment/Experiment.cs)

[RunMultiSequnceLearningExperiment()](../MyExperiment/InvokeMultisequenceLearning.cs)

[Download Azure Input files ,Upload Experiment Result to Azure Table](../MyExperiment/AzureStorageProvider.cs)

[Docker file for image ](../MyCloudProject/Dockerfile)   
The Azure Cloud Project availible running in Azure cloud as a Docker image in Container Repo.(docker pull ccprojectc.azurecr.io/teamalphamycloudproject:v1)

[Configuration file with connnection string](../MyCloudProject/appsettings.json)


## Our experiment description

![Alt text](CloudExperiment.png)

![Alt text](MultiSequnceExperiment.png)   


1. What is the **input**?<br/>
The Experiment Input is Defined in POJO class ExerimentRequestMessage.cs like
```csharp
    public class ExerimentRequestMessage: IExerimentRequestMessage
    {
        public string ExperimentId { get; set; }
        public string InputFile { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string testFile { get; set; }
    }
```
And a Queue message will be mapped to the above class and will be used in async Task RunQueueListener in the Experiment.cs file 
```json
  {
  "ExperimentId":"ExperimentV1", 
  "InputFile":"sampleinputseq.csv", 
  "TestInputFile":"TestValidationInput.txt"
  }
```

a.The input for Multisequnce learning experiment is a sampleinputsequnce.csv file containing the sequnce with which we input to the MultisequnceLearning Experiment.<br/> [input sequence file](AzureCloudProject/training-files/sampleinputseq.csv) <br/><br/>
b.The Test sequence is also fetched from Azure blob storage.(Used as testing the prediction of next element from the HTMCLassifier)<br/> [input-test sequence sample](AzureCloudProject/training-files/TestValidationInput.txt)
<br/> [input-test sequence sample2](AzureCloudProject/training-files/TestValidation3Input.txt) 


2. What is the **output**?
<br/>
Oputput of the Multisequnce learning experiment is Predicting the Next element after the learning is done.   
We calculate the Predictor accuracy with both **Normal Predictor** and also a **SerializedPredictor**.   
```csharp
   public class ExperimentResult : ITableEntity, IExperimentResult
    {
        public ExperimentResult(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey= rowKey;
        }
        public string PartitionKey { get; set; }       
        public string RowKey { get; set; }     
        public DateTimeOffset? Timestamp { get ; set ; }    
        public ETag ETag { get; set; }
        public string ExperimentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public long DurationSec { get; set; }
        public string InputFileUrl { get; set; }
        public string OutputFiles { get; set; }
        //properties related to experiment.
        public double SerializedPredictorAccuracy { get; set; }
        public double NormalPredAccuracy { get; set; }
        public string TestedSequence { get; set; }
    }
```
<br/>

<br/>  [Console Output showing Accuracy and prediction sequnce](AzureCloudProject/Consoleoutput/Console-Output-3-test-predictions-correct.txt) 

<br/>

~~~
-------------Sequence to test: 8, 1, 2-----------------

 item name : 8
Sequence predicted from Normal Predictor: S2_2-9-10-7-11-8-1 , next Element: 1
Sequence predicted from Serialized Predictor: S2_2-9-10-7-11-8-1 , next Element : 1

 item name : 1
Sequence predicted from Normal Predictor: S1_3-4-2-5-0-1-2 , next Element: 2
Sequence predicted from Serialized Predictor: S1_3-4-2-5-0-1-2 , next Element : 2

 item name : 2
Sequence predicted from Normal Predictor: S1_0-1-2-3-4-2-5 , next Element: 5
Sequence predicted from Serialized Predictor: S1_0-1-2-3-4-2-5 , next Element : 5

For the List of [8, 1, 2] the accuracy calculated as below:
Normal Predictor Accuracy -->100
HTMClassifier serialised Predictor SerializedPredictorAccuracy -->100
------------------------------
Azure.Storage.Blobs.Models.BlobProperties
Uploaded to Table Storage successfully
~~~
#   
**<span style="color: blue">Result of the Experiment</span>**   
The predictor accuracy of both **Normal Predictor** and **SerializedPredictor** is checked using Multisequnce learning and found to be equal and hence the proof.

a) This is saved to Azure Table with Proper ExperimentId and associated , testsequnce tested, and SerializedAccuracy and Normal Accuracy. 
<br/>
b)Also as an output, a serialized output.txt file in each experiment saved to the Azure output container .    

<span style="color: blue"> 

| Experiment Number|Number of sequnce Inputs for MSLearning Experiment|Duration(s)|Testing Sequnce |Prediction Accuracy (%)|Is same for Normal and Serialized HTMClassifer?
|---------------------------------------|------------|------------|------------|------------|------------|
|1)Small Experiment |  S1,S2 (learn for 2 sequence)|25 sec| Next Element Predicted for 2 test Sequnce| 100 | Yes|
|2) Normal Experiment | S1,S2,S3 (learn for 3 sequence)|28 sec| Next Element Predicted for 3 test Sequnce|100 |Yes|
|3) Large Experiment | S1,S2,S4 ...., S9,S10 lear for 10 sequence|142 sec|Next Element Predicted for 2 test Sequnce| 100 |Yes|   

</span>
     
 **We Can input even large file containing 50 or even more sequnce to a input file and call from Message queue for learning purpose and test the prediction in Azure**   
Outup console file added here for reference: [Output file](Large-Experiment-Run-Console-out.txt)

#  
3. **How our algorithm works ?**

    a) The RunMultiSequenceLearningExperiment.Learn()(Learn method) invoked   
    b) experiment.Learn() takes Parameters like,   
            -	Dictionary<string, List<double>> sequences   
            -	ScalarEncoder (with predefined settings parameter) HTMConfig(Standard cfg  for the Cortexlayer ,temporalmemory and spatial pooler) (below CodeRef:3)  
    c) A HTMCllassifer instance(Normal) Learns with expected accuracy   
    d) After Learning , Normal Instance calls HTMCLassifer.Serilaize() method Saves to an output file  
          - A new HTMCLassifer(Serialized) Instance created.   
          -	Use the previous Serilaized foutput file to bring  the  saved state of class.   (below CodeRef:2)
          -	Serialized Instance.Deserialize() invoked   (CodeRef:2)
          - Returns Predictors of HTMClassifer instances : Normal Predictor and Serialized  Predictor   (CodeRef:1)

   
CodeRef:1
~~~csharp
        /// <summary>
        /// Runs a multi-sequence learning experiment.
        /// </summary>
        /// <param name="input">A list of input sequences.</param>
        /// <param name="testSequences">The path to the test sequences file.</param>
        /// <param name="outputFileName">The name of the output file for results.</param>
        /// <returns>A list of key-value pairs, where the key is a sequence and the value is a list of accuracies.</returns>
        public static List<KeyValuePair<string, List<double>>> RunMultiSequenceLearningExperiment(List<double[]> input, string testSequences,string outputFileName)
        {
            // Dictionary to store sequences
            Dictionary<string, List<double>> sequences = new Dictionary<string, List<double>>();

            // Create sequences from input data
            for (int i = 0; i < input.Count; i++)
            {
                sequences.Add("S"+(i+1), new List<double>(input[i]));
            }

            // Read test sequences from a file
            var text = File.ReadAllText(testSequences, Encoding.UTF8);
            var trainingData = JsonSerializer.Deserialize<TestData>(text);

            // Prototype for building the prediction engine.
            // Initialize the experiment and get the predictor
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            Predictor serializedPredictor;
            var predictor = experiment.Learn(sequences, outputFileName, out serializedPredictor);

            // Predict the next element for each test sequence
            var acc = trainingData.TestValidation
             .Select(seq => {
                 var predictionResult = PredictNextElement(predictor, seq, serializedPredictor);
                 List<double> accuracyList = new List<double>();
                 accuracyList.Add(predictionResult.Item2);//Predictor accuracy of HTClassifier serialized predictor
                 accuracyList.Add(predictionResult.Item3);
                 return new KeyValuePair<string,List<double>>(string.Join(", ", seq),accuracyList);                           
             }).ToList();

            //a list of key-value pairs, where the key is a sequence and the value is a list of accuracies
            return acc;
        }
~~~   
CodeRef:2   
~~~csharp
 private Predictor RunExperiment(int inputBits, HtmConfig cfg, EncoderBase encoder, Dictionary<string, List<double>> sequences,string outputFileName, out Predictor serializedPredictor)
        {
       .
            HtmClassifier<string, ComputeCycle> cls = new HtmClassifier<string, ComputeCycle>();

            HtmClassifier<string, ComputeCycle> serClassifier = new HtmClassifier<string, ComputeCycle>();
       .
       .
            //here we actually check for the HTMClassifier newly created serialize method to create a serialized file with current variables od the object.
            using (var writer = new StreamWriter(outputFileName))
            {
                cls.Serialize(cls, null, writer);
            }
            // deserialize the earlier serialized status of the HTMClassifier class to object, and hence create and heck the Correctness of the prediction
            // this serClassifier object is also passed to create a new Predictor 
            // So now we can compare normal Predictor and also a HTMClassifier Predictor.
            using (StreamReader sr = new StreamReader(outputFileName))
            {
                serClassifier = serClassifier.Deserialize(sr);                
            }
            // a new Predictor after serialization deserialization of HTMClassifier class
            serializedPredictor = new Predictor(layer1, mem, serClassifier);
            
            //If Required to DEBUG you cans use below line to check f both the instances of HTMClassifiers are equal
            //bool equal=cls.Equals(serClassifier);

            //returns a normal Predictor
            return new Predictor(layer1, mem, cls);
        }
~~~
   
   CodeRef:3
~~~csharp
            int inputBits = 100;
            int numColumns = 1024;
          HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                Random = new ThreadSafeRandom(42),
                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,
                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),
                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,
                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,
                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
            };
            double max = 20;
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);
~~~   
<br/>
    e)Next, the testing elements is iterated and for each  and we check for predicted nextelemnt and get accuracy as a list

~~~csharp
...
            // Predict the next element for each test sequence
            var acc = trainingData.TestValidation
             .Select(seq => {
                 var predictionResult = PredictNextElement(predictor, seq, serializedPredictor);
                 List<double> accuracyList = new List<double>();
                 accuracyList.Add(predictionResult.Item2);//Predictor accuracy of HTClassifier serialized predictor
                 accuracyList.Add(predictionResult.Item3);
                 return new KeyValuePair<string,List<double>>(string.Join(", ", seq),accuracyList);                           
             }).ToList();

            //a list of key-value pairs, where the key is a sequence and the value is a list of accuracies
            return acc;
~~~
f) Prediction of  Next Element Logic for the test sequence
InvokeMultisequenceLearning.PredictNextElement()   
	-Calls NeoCortexAPI.Predictor().Predict() method , Predicts Next element for both Normal Predictor and Serialized Predictor   
	-Calculates accuracy of both Normal Predictor and Serialized Predictor   
	-Returns Accuracy and Predicted Nextelemnt for each test sequnce   

~~~csharp
 private static Tuple<List<KeyValuePair<String, String>>,double,double> PredictNextElement(Predictor predictor, double[] testItem, Predictor serPredictor)
{
    ...
                predictor.Reset();
                serPredictor.Reset();
                var res = predictor.Predict(item);
                var resSerializedPred = serPredictor.Predict(item);

                    var similarity = res.First().Similarity;

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Console.WriteLine($"From actualPredictor--> Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");

        ...
            #region Prediction Accuracy Calculation below .
            //Calculate predictorAccuracy
            double predictorAccuracy = matchCount * 100 / (double)totalCount;
            predictorAccuracy= Math.Round(predictorAccuracy, 2);

            double serialisedPredAccuracy = (double)matchCount1 * 100 / (double)totalCount1;
            serialisedPredAccuracy=Math.Round(serialisedPredAccuracy, 2);
            #endregion
}
~~~
g)  Azure Result Uploads  to Table  with below code:   
TableServiceClient is created to communicate with azure connection   
  -IExperimentResult returned is prepared as stronglyTypedEntity  for Azure Storage Table Upload(Duration, Serilaized Accuracy, Normal Accuracy for each test is upload in Table.)

~~~
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
~~~   

   
## Azure Links

Docker image pull : docker pull ccprojectc.azurecr.io/teamalphamycloudproject:v1


[Azure Queue](https://portal.azure.com/#view/Microsoft_Azure_Storage/QueueMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd60f2036-12f5-499d-af22-ef3afc698896%2FresourceGroups%2FCCProjectR%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fccprojectsd/queueName/teamalpha-trigger-queue)

[Azure Training Input Files ](https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd60f2036-12f5-499d-af22-ef3afc698896%2FresourceGroups%2FCCProjectR%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fccprojectsd/path/teamalpha-training-files/etag/%220x8DBB89FBA595D97%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride~/false/defaultId//publicAccessVal/None)

[Azure Output Result Table](
https://portal.azure.com/#@msdndaenet.onmicrosoft.com/resource/subscriptions/d60f2036-12f5-499d-af22-ef3afc698896/resourceGroups/CCProjectR/providers/Microsoft.Storage/storageAccounts/ccprojectsd/storagebrowser)
   

##  Azure Container Registry Details
Login Server: ccprojectc.azurecr.io   

<details>
  <summary>ACR Credentials</summary>
Username:
CCProjectC

Password:
xtRLaU37ww09FWh25uD08ZrH4+yxz2txOC2yzyKRbU+ACRBs/qKv
</details>

<br />

[Message QUEUE -SAS URL]( https://ccprojectsd.queue.core.windows.net/?sv=2022-11-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-09-20T05:47:51Z&st=2023-09-19T21:47:51Z&spr=https&sig=71GvMVqayi1zL5lAGV7fMhyESA4OIW3iFrOdrSwhnwo%3D)

[Blob Storage- Input training files -SAS URL]( https://ccprojectsd.blob.core.windows.net/teamalpha-training-files?sp=racwl&st=2023-09-19T21:26:22Z&se=2025-09-20T05:26:22Z&spr=https&sv=2022-11-02&sr=c&sig=r8LTefm35NvKEXaVx1iV2%2FH3UZejpRIJ7Lb8wsLB6J8%3D)

[Blob Storage- Output Result files -SAS URL](https://ccprojectsd.blob.core.windows.net/teamalpha-result-files?sp=racwl&st=2023-09-19T21:42:32Z&se=2025-09-20T05:42:32Z&spr=https&sv=2022-11-02&sr=c&sig=EpyF28ab120NHBblJgbtAI3wlsUAg%2FBuTg%2BTwXm9j%2F4%3D)

[Result-tabes -SAS URL](https://ccprojectsd.table.core.windows.net/?sv=2022-11-02&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-09-20T05:47:51Z&st=2023-09-19T21:47:51Z&spr=https&sig=71GvMVqayi1zL5lAGV7fMhyESA4OIW3iFrOdrSwhnwo%3D)

<br/>

<br/>
Console output in azure container instance

![Console output in azure container instance](Project-Azure-ContainerInstance-Console.png)
<br/>
<br/>

## How to Run Project in Azure
Descripition on how to run Cloud Experiment based on the Input/Output mentioned in the Previous Section.   
**Prepare an azure MessageQueue Json Message you need to use to trigger the experiment**     
  - To run our Azure Cloud Experiment     
      The Queue message currently gives the name of the input file in it.
      (File is a csv file with data for the input sequnce for the learning of the Multisequnce Experiment for the HTMCLassifier prediction check.) This file is the fetched from the Inputblobcontainer From the Azure Container Registry.
~~~json

{
"ExperimentId":"ExperimentV1", // any String to identify our experiment
"InputFile":"sampleinputseq.csv", // the input Training filename actually   holding the sequence for training required for the Experiment Run
"TestInputFile":"TestValidationInput.txt"
}

~~~
<br/>

[Queue sample](AzureCloudProject/training-files/Azure-Queue-Msg.txt)
<br/>

![Alt text](team-alpha-trigger-queue.png)

![Alt text](teamalpha-queue-addition.png)

<br/>


Table 1: Queue Message parameter details

| Parameter                         	  |   Description|
|---------------------------------------|------------|
| ExperimentId   	|    Id of the experiment which is run|
| InputFile | 1. The Csv file contianing data for the input Sequnce for the Dictionary parameter for the learning phase of RunMultisequnceLearing.    2. We have another file which will hold the test sequnce, this helps us give as much as testing possibility to predict next elements       	|
|TestInputFile |  The file name refence to the training files blob storage which fetches the Test sequnce we use to predict next element|



**Blob container registry:**  

Detialis of the blob containers :  
- **'teamalpha-training-files'** : for input sequnce to be given to the experiment is fetched from this blob container
  - the files is currently refenced from Queue msg and used 
  ![Alt text](teamalpha-training-files.png)
  
- **'teamalpha-result-files'** : While execution of the Experiment , we Serilaize an instance of HTMClassifer class, and we will save this as output and for future refence.
  - The output file is a txt file which is a result of the serialized output of HTMCLassifier serilaize method  
![Alt text](teamalpha-result-files.png)
- **"teamalpharesultstable"** : Is used to store the basic Experiment output like accuracy and next predicted elemnt of a SERialized predictor



<br/>


**Describe the Result Table:**


Expected Result: 
 THe Result tabel demonstrates the predicion of each experiment result on the Serialized HTMCLassifiers accuracy, compared to the normal predictor accuracy as two entity in the Result table. Additionally the time for the execution of the experiment is also availabe in the table entry.

Results arranged as below:
Corresponding to each experiment run and the number of testing sequnce we used to verify the prediction accuracy of the serialized predictor, a set of rows are saved. and each row can be identified  with the unique Roykey contianing Experiment ID + and corresponding test sequence used. 


 #### Table 2: Columns in the result table.
| Entity                         	  |Description|
|---------------------------------------|------------|
|PartitionKey |  Unique GUID for every experiment run |
|RowKey | Corresponds to the number of tested rows generated by each experiment run (ExperimentId + TestSequnce )  |
|ExperimentId | Id of the experiment that was run.|
|StartTimeUtc| Start time of the experiment. |
|EndTimeUtc | End time of the experiment. |
|DurationSec | Total time elapsed during run of the experiment |
|Name | The name of the Experiment. |
|Description | Description about the Serialization Multisequnce experiment |
|SerializedPredictorAccuracy |Accuracy of predicting next elemnt and sequnce for the HTMClassiferPredictor |
|NormalPredAccuracy | Accuracy of predicting next elemnt and sequnce for the Normal Predictor. |
|OutputFiles |reference to the serialized output.txt saved as part of experiment for future refernce |
|InputFileUrl | reference the input sequnce file used in experiment |

<br/>
<br/>

![Result Table-part1](teamalpha-result-table-2nd.png)
<br/>
<br/>
![ Result Table-part1](teamalpha-result-table-1st.png)


## Create and deploy Docker Image

### Docker commands and operations to create and push docker image
~~~bat

#Steps to Docker image to our cloud project
docker build -f "C:\Users\docker file location\Dockerfile" --force-rm -t mycloudproject

#list docker images
docker images
~~~

### Azure commands tag and push docker image to Azure Cloud
~~~bat
az login

#login using proper tennant ID to access correct subscription
az login --tenant 45ad6974-849d-4d24-8584-456affacc0f8

#azure login to container registry in the correct REsourse Group
#Already created Resourse-Group in Azure UI Portal, also a Container REgistry , and Blob #Container in the same RGroup , Will need to crete a Container instance to run the image in the Container Repository.

az acr login --name ccprojectc


#tag appropriate name f or image 
docker tag mycloudproject:latest cloudprojectteamalpha.azurecr.io/mycloudproject:v1
docker tag mycloudproject:latest ccprojectc.azurecr.io/teamalphamycloudproject:v1

#push to container registry repository
docker push cloudprojectteamalpha.azurecr.io/mycloudproject:v1
docker push ccprojectc.azurecr.io/teamalphamycloudproject:v1
~~~
#
