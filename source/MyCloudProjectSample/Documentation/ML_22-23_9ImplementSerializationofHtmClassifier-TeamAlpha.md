# Azure Cloud Implementation ML22/23-9 Implement Serialization of HtmClassifier -Team Alpha - 

Aim of this Azure Cloud implementation of Software Enginenering project to have the project to be up and running in Cloud enviornment.
For this we use the Docker image dpeloyment in to Azure Containers in Cloud.
The Serialization feature we implemented in HTMClassifier class together withth Deserialization feature added is verified here and compared with other Classifier class using Multisequnce learnign method.Finally the Prediction accuracy for Normal Predictor and the New serialized HTMClassifer predictor is evaluated to be same and hence the prrof.
The Entire AzureCloud Project is created as a Docker image. This is Deployed to Azure Container Registry Repository, and then Run in Cloud.

The Serialization of HTMClassifier is implemented with SE Project.
The neewly intorduced Serilaize and Deserialize method works  as outcome of the Project .
The two new features are unit tested and Documneted in https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/Implement%20Serialisation%20of%20HTMClassifier.pdf   


## Cloud Project Structure

[My CloudProject](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyCloudProject/Program.cs)

[Cloud Experiment](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/Experiment.cs)

[RunMultiSequnceLearningExperiment()](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/InvokeMultisequenceLearning.cs#L21)

[Download Azure Input files](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/AzureStorageProvider.cs#L34)

[Upload Experiment Result to Azure Table](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyExperiment/AzureStorageProvider.cs#L73)

[Docker file for image ](https://github.com/antonyaneeta/neocortexapi/blob/7680dcd535d58381706212faa75dfbe3d57d4ae0/source/MyCloudProjectSample/MyCloudProject/Dockerfile)

The Azure Cloud Project availible running in Azure cloud as a Docker image running, with the following steps.

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
          .
          .
          .

            double predictorAccuracy = matchCount * 100 / (double)totalCount;
            predictorAccuracy= Math.Round(predictorAccuracy, 2);

            double serialisedPredAccuracy = (double)matchCount1 * 100 / (double)totalCount1;
            serialisedPredAccuracy=Math.Round(serialisedPredAccuracy, 2);
            #endregion

}
~~~




## What is your experiment about

Our Project: Validate the Serialization and De-serialization Feature implemented in the HTMClassifier Class by checking Multisequnce learning and Predicted next Elements for a given sequence.
Azure Cloud Project: The implemented SErialization is validated using Multisequnce learning method and the 
SE Project Documentation can be found in - https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/Implement%20Serialisation%20of%20HTMClassifier.pdf   
Readme.md file availiiable about THE SE project can be found here :- https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/README.md   








1. What is the **input**?
    a.The input for Multisequnce learning experiment is a sampleinputsequnce.csv file containing the sequnce with which we input to the MultisequnceLearning Experiment.
    b.The Test sequence is also fetched from Azure blob storage.(Used as testing the prediction of next element from the HTMCLassifier)

2. What is the **output**?
Oputput of the Multisequnce learning experiment is Predicting the Next element after the learning is done. 
We calculate the Predictor accuracy with both normal predictor and also a Serialized predictor
##This easblishes that the newly implemented HTMClassifier Serialization is correct and matching with normal predicotr class.


Result of the Experiment :
The predictor accuracy of both normal and SerializedPredictor is checked and found to be equal and hence the proof.


a.This is saved to Azure Table with Proper ExperimentId and associated , testsequnce tested, and SerializedAccuracy and Normal Accuracy.
b.Also as an output, a serialized output.txt file in each experiment saved to the Azure output container . 


3. How our algorithm works:

  a) The RunMultiSequenceLearningExperiment() is prepared soa as to run our mycloud project.
  b) The Run method (aka Learn method) in RunMultiSequenceLearningExperiment() does the following:
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

  c) As Next step 
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


## How to run our Azure Cloud Experiment

Descripition of the Cloud Experiment based on the Input/Output mentioned in the Previous Section.

THe Queue message currently gives the name of the input file in it.
(File is a csv file with data for the input sequnce for the learning of the Multisequnce Experiment for the HTMCLassifier prediction check.) This file is the fetched from the Inputblobcontainer From the Azure Container Registry.

**_ Azure MessageQueue Json Message you need to use to trigger the experiment:_**  

~~~json
{
"ExperimentId":"ExperimentV1", // any String to identify our experiment
"InputFile":"sampleinputseq.csv", // the input Training filename actually holding the sequence for training required for the Experiment Run
"TestInputFile":"TestValidationInput.txt" // the file name to the Test sequnce we use to predict next element
}
~~~

- ExperimentId : Id of the experiment which is run  
- InputFile: the Csv file contianing data for the input Sequnce for the Dictionary parameter for the learning phase of       RunMultisequnceLearing.  
-We have another file which will hold the test sequnce, this helps us give as much as testing possibility to predict next elements

**_Describe your blob container registry:**  

Detialis of the blob containers :  
- 'training-files' : for input sequnce to be given to the experiment is fetched 
  - the file is currently refenced in Queue msg and used 
- 'result-files' : While execution of the Experiment , we Serilaize an instance of HTMClassifer class, and we will save this as output and for future refence.
  - The file is a txt file which is a result of the serialized output of HTMCLassifier serilaize method  
- "ResultTable": "results1" is used to store the basic Experiment output like accuracy and next predicted elemnt of a SERialized predictor


**_Describe the Result Table_**

 What is expected ?
 
 How many tables are there ? 
 
 How are they arranged ?
 
 What do the columns of the table mean ?
 
 Include a screenshot of your table from the portal or ASX (Azure Storage Explorer) in case the entity is too long, cut it in half or use another format
 
 - Column1 : explaination
 - Column2 : ...
Some columns are obligatory to the ITableEntities and don't need Explaination e.g. ETag, ...
 
