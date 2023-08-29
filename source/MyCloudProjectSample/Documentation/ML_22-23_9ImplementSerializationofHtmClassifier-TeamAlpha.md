# ML2223-9 Implement Serialization of HtmClassifier -Team Alpha - Azure Cloud Implementation

The Serialization of HTMClassifier is implemented with this Project.
The Newly intorduced Serilaize Deserialize method works towards the goal of the Project .
The Methods are unit tested and Documneted in https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/Implement%20Serialisation%20of%20HTMClassifier.pdf   


With  and aim to make the Project availible running in Azure cloud as a Docker image running in Azure the folloing steps are done.

Use this file to describe your experiment.
This file is the whole documentation you need.
It should include images, best with relative path in Documentation. For Example "/pic/image.png"  
Do not paste code-snippets here as image. Use rather markdoown (MD) code documentation.
For example:

~~~csharp
private static void PredictNextElement(Predictor predictor, double[] list, Predictor serPredictor)
{
    
                    var similarity = res.First().Similarity;

                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    Console.WriteLine($"From actualPredictor--> Predicted Sequence: {tokens[0]}, predicted next element {tokens2.Last()}");

                    // Calculate predictorAccuracy
            var predictorAccuracy = (matchCount * 100) / totalCount;
            var serialisedPredAccuracy = (matchCount1 * 100) / totalCount1;

}
~~~


## What is your experiment about

Describe here what your experiment is doing. Provide a reference to your SE project documentation (PDF)*) - https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/Implement%20Serialisation%20of%20HTMClassifier.pdf   
Readme.md file availiiable about project  here :- https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/blob/team_alpha/MySEProject/Documentation/README.md   

1. What is the **input**?
The input for Multisequnce learnign experiment is a csv file containing the sequnce with which we indent to train teh HTMCLassifier class

2. What is the **output**?
Oputput of the Multisequnce learning experiment is Predicting hte Next elemnt after the learning done. 
WE calculate the Predictor accuracy with both normal predicotor and ALso a Serialized predictor
THis easblisheds that the newly implemented HTMClassifier Serialization  is correct and matching with normal predicotr class.

a. The utput of a  serialized output .txt file in each experiment saved to the Azure output container.
b. THe Important 

3. What your algorithmas does? How ?

## How to run experiment

Describe Your Cloud Experiment based on the Input/Output you gave in the Previous Section.

THe Quee message currently gives the name of the input file in it.(File is a csv file with data for the input sequnce for the learning of the Multisequnce Experiment for the HTMCLassifier prediction check.)
This file is the fetched from the Inputblobcontainer From the Azure Container Registry.

**_Describe the Queue Json Message you used to trigger the experiment:_**  

~~~json
{
     ExperimentId = "123",
     InputFile : "sampleinputseq.csv",
     .. // see project sample for more information 
};
~~~

- ExperimentId : Id of the experiment which is run  
- InputFile: the Csv file contian g data for the input sequnce for the Dictionary parameter for the learning phase.  

**_Describe your blob container registry:**  

what are the blob containers you used e.g.:  
- 'inputblobcontainer' : for input sequnce to be given to the experiment is fetched 
  - the file is currently refenced in Queue msg and used 
- 'outblobcontainer-1' : saving output written file  
  - The file i s a txt file which is a result of the serialized output of HTMCLassifier serilaize method  
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
 
