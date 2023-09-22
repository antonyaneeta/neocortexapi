# Azure Cloud Implementation ML22/23-9 Implement Serialization of HtmClassifier -Team Alpha 

Aim of this Azure Cloud implementation of Software Enginenering project to have the project to be up and running in Cloud enviornment.
For this we use the Docker image dpeloyment into Azure Containers in Cloud.
<br/>The Serialization feature we implemented in HTMClassifier class together with the Deserialization feature is verified here and compared with other normal Prediction using Multisequnce learning method.
**Finally the Prediction accuracy for Normal Predictor and the New serialized HTMClassifer predictor using Multisequnce learning is evaluated verified to be same and hence the prrof.**
<br/>The Entire AzureCloud Project is created as a Docker image. This is Deployed to Azure Container Registry Repository, and then Run in Cloud.


<details>
  <summary> Click to see previous SE Project:  Implementation of Serialization in HTMClassifier.
  </summary>
  <br/>

Readme.md file availiiable about the SE project can be found here :- <br/>
https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/README.md  <br/> <br/> The newly intorduced Serilaize and Deserialize method works  as outcome of the Project .The serilization and deseialization features are  validated unit tested and Documneted in <br/>
https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/Implement%20Serialisation%20of%20HTMClassifier.pdf 
</details>

## Cloud Architecture
![Cloud Project Overview](Cloud-Project-Architecture-Team-Alpha.png)

## Cloud Project Links

[My CloudProject](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyCloudProject/Program.cs)

[Cloud Experiment](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/Experiment.cs)

[RunMultiSequnceLearningExperiment()](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/InvokeMultisequenceLearning.cs#L21)

[Download Azure Input files](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/AzureStorageProvider.cs#L34)

[Upload Experiment Result to Azure Table](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/AzureStorageProvider.cs#L73)

[Docker file for image ](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyCloudProject/Dockerfile)

The Azure Cloud Project availible running in Azure cloud as a Docker image in Container Repo.
##
[Configuration file with connnection string](../MyCloudProject/appsettings.json)
#

## Azure Links

Docker image pull : docker pull ccprojectc.azurecr.io/teamalphamycloudproject:v1


[Azure Queue](https://portal.azure.com/#view/Microsoft_Azure_Storage/QueueMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd60f2036-12f5-499d-af22-ef3afc698896%2FresourceGroups%2FCCProjectR%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fccprojectsd/queueName/teamalpha-trigger-queue)

[Azure Training Input Files ](https://portal.azure.com/#view/Microsoft_Azure_Storage/ContainerMenuBlade/~/overview/storageAccountId/%2Fsubscriptions%2Fd60f2036-12f5-499d-af22-ef3afc698896%2FresourceGroups%2FCCProjectR%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2Fccprojectsd/path/teamalpha-training-files/etag/%220x8DBB89FBA595D97%22/defaultEncryptionScope/%24account-encryption-key/denyEncryptionScopeOverride~/false/defaultId//publicAccessVal/None)

[Azure Output Result Table](
https://portal.azure.com/#@msdndaenet.onmicrosoft.com/resource/subscriptions/d60f2036-12f5-499d-af22-ef3afc698896/resourceGroups/CCProjectR/providers/Microsoft.Storage/storageAccounts/ccprojectsd/storagebrowser)


## What is your experiment about

Our Project: Validate the Serialization and De-serialization Feature implemented in the HTMClassifier Class by checking Multisequnce learning and Predicted next Elements for a given sequence.
Azure Cloud Project: The implemented Serialization is validated using Multisequnce learning method and the acurracy checked.




1. What is the **input**?<br/>
    a.The input for Multisequnce learning experiment is a sampleinputsequnce.csv file containing the sequnce with which we input to the MultisequnceLearning Experiment.<br/> [input sequence file](AzureCloudProject/training-files/sampleinputseq.csv) <br/><br/>
    b.The Test sequence is also fetched from Azure blob storage.(Used as testing the prediction of next element from the HTMCLassifier)<br/> [input-test sequence sample](AzureCloudProject/training-files/TestValidationInput.txt)
<br/> [input-test sequence sample2](AzureCloudProject/training-files/TestValidation3Input.txt) 



2. What is the **output**?
<br/>
Oputput of the Multisequnce learning experiment is Predicting the Next element after the learning is done. 
We calculate the Predictor accuracy with both normal predictor and also a Serialized predictor
<br/>

**This easblishes that the newly implemented HTMClassifier Serialization is correct and matching with normal predicotr class.**

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


 **Result of the Experiment** :
The predictor accuracy of both normal and SerializedPredictor is checked and found to be equal and hence the proof.

a) This is saved to Azure Table with Proper ExperimentId and associated , testsequnce tested, and SerializedAccuracy and Normal Accuracy. 
<br/>
b)Also as an output, a serialized output.txt file in each experiment saved to the Azure output container . 


3. **How our algorithm works:**

    a) The RunMultiSequenceLearningExperiment() is prepared soa as to run our mycloud project.
    b) The Run method (Learn method) in RunMultiSequenceLearningExperiment() does the following:

~~~csharp
...
            // Prototype for building the prediction engine.
            MultiSequenceLearning experiment = new MultiSequenceLearning();
            Predictor serializedPredictor;

            // as a "out" param we pass get also serializedPredictor for the New HTMClassifier class.
            var predictor = experiment.Learn(sequences, outputFileName, out serializedPredictor);
            .
            .
            .
~~~
<br/>
    c)Next, the training sequnce is iterated and for each we check for predicted nextelemnt for both normal predictor and HTMClassiferSerialized predictor and accuracy of both saved to result to azure entity. 

~~~csharp
              #region  The Prediction of next element for Normal Predictor and a HTMClassifierSeialized Predicotr logic below.

            // below code we tet he prediction with the test downloaded from Azure container
            //ie. we check for each PredictNextElement in the TestValidation List 

            // These testItem are used to see how the prediction works.
            // By providing more elements to the prediction, the predictor delivers more precise result.


            var acc = trainingData.TestValidation
             .Select(seq => {

                 var predictionResult = PredictNextElement(predictor, seq, serializedPredictor);
                 List<double> accuracyList = new List<double>();
                 accuracyList.Add(predictionResult.Item2);
                 accuracyList.Add(predictionResult.Item3);
                 return new KeyValuePair<string,List<double>>(string.Join(", ", seq),accuracyList);
                            
             }).ToList();

            #endregion
~~~
d) Predict Next Element Logic below:

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

            #region Prediction Accuracy Calculation below .
            // Calculate predictorAccuracy
          ...

            double predictorAccuracy = matchCount * 100 / (double)totalCount;
            predictorAccuracy= Math.Round(predictorAccuracy, 2);

            double serialisedPredAccuracy = (double)matchCount1 * 100 / (double)totalCount1;
            serialisedPredAccuracy=Math.Round(serialisedPredAccuracy, 2);
            #endregion

}
~~~
e)Azure Result Upload  to Table 
~~~

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
~~~

## How to run our Azure Cloud Experiment

Descripition of the Cloud Experiment based on the Input/Output mentioned in the Previous Section.

The Queue message currently gives the name of the input file in it.
(File is a csv file with data for the input sequnce for the learning of the Multisequnce Experiment for the HTMCLassifier prediction check.) This file is the fetched from the Inputblobcontainer From the Azure Container Registry.

###  Azure Container Registry Details
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

##
**Azure MessageQueue Json Message you need to use to trigger the experiment**  

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


##
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

#
## Describe the Result Table


Expected Result: 
 THe Result tabel demonstrates the predicion of each experiment result on the HTMCLassifiers accuracy, compared to the normal predictor accuracy as two entity in the Result table. Additionally the time for the execution of the experiment is also availabe in the table entry.

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
 
##
# Docker commands and operations to create and push docker image
~~~bat

#Steps to Docker image to our cloud project
docker build -f "C:\Users\docker file location\Dockerfile" --force-rm -t mycloudproject

#list docker images
docker images
~~~

##
# Azure commands tag and push docker image to Azure Cloud
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
##
