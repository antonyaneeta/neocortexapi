# SE-PROJECT ML22/23-9 Implement Serialization of HtmClassifier  

# Implementation
Inorder to have serialization functionality to HTM Classifier class one should implement ISerializable.
So add the required specific and unique implementation for Serialisation and De-serialization in HtmClassifier Class. 
  
Methods added:  
# Create a method Serialization()
Snippet of method added in-order to achieve the serialization functionality for the HtmClassifier class.
[Refer code in Git for Serialize method](https://github.com/antonyaneeta/neocortexapi/blob/98ca46e17b8efe842471deaed73c530068ded8ef/source/NeoCortexApi/Classifiers/HtmClassifier.cs#L501)
~~~csharp
        public void Serialize(object obj, string name, StreamWriter sw)
        {
            //Serialization code below.

            HtmSerializer ser = new HtmSerializer();
            ser.SerializeBegin(nameof(HtmClassifier<TIN, TOUT>), sw);
            ser.SerializeValue(maxRecordedElements, sw);
            ser.SerializeDictionaryValue(m_AllInputs, sw);
            ser.SerializeEnd(nameof(HtmClassifier<TIN, TOUT>), sw);
        }
~~~
      
       
~~~csharp
    public void SerializeDictionaryValue<TIN>(Dictionary<TIN, List<int[]>> value, StreamWriter sw) {}
~~~

  
  Steps:   
    - Note that new HtmSerializer() class initialised here is a consolidated class containing various implementations for Serialisation of different generic datatypes and parameters in connection with SpatialPooler encoders.  
    - The serialization of ´int´ is reused from old code, for the first parameter `maxRecordedElements` of the Classifier.
    - The `m_Allinput` is a keyValuePair ` Dictionary<TIN, List<int[]>> parameter used to map all input (sparse array called as sparse distributed representation) SDR associated to the input to HTMClassifier.  
    - A new generic method was created to serialize this values of format List<int[]> was added in HTMSerializer class. -->  [Method in Serialize.cs file](https://github.com/antonyaneeta/neocortexapi/blob/cec4993b41577690613e4b24bf1510471527e7f4/source/NeoCortexEntities/HtmSerializer.cs#L1990)  
    - Similarly when other new parameters come in picture we need to correspondilgy add further implementations 
~~~csharp
 using (StreamWriter sw = new StreamWriter(fileName))
            {
                htmClassifier.Serialize(htmClassifier, null, sw);
            }
~~~
Use above code to call serialization and StreamWriter helps to save it to any text file .

## SERIALIZED FILE OUTPUT
Sample of a Serialised and saved text file :   [TestSerializationHtmClassifier.txt](https://github.com/UniversityOfAppliedSciencesFrankfurt/se-cloud-2022-2023/files/11265114/TestSerializationHtmClassifier.txt)

  
# Create method Deserialization()  
 After serialisation we have a text file in a particular format defined by out implementation. Various parameters are seperated using Parameter seperators like ´|´ pipe operator, or ´coma´ or ´Keyvalue Seperater : ´ etc.
 Add proper logic to Read the file and fetch it back to the Classifier instance. Below code 
 
 ~~~csharp
    public HtmClassifier<TIN, TOUT> Deserialize(StreamReader sr)
        {
            .
            .

                // If the line contains a key-value pair, deserialize it
                if (data.Contains(HtmSerializer.KeyValueDelimiter))
                {

                    var kvp = ser.ReadDictSIarrayList<TIN>(cls.m_AllInputs, data);
                    cls.m_AllInputs = kvp;

                }
                // Otherwise, parse the parameters in the line and set them in the HtmClassifier
                else
                {
                    .
                    .
                    .

                    // If the first parameter is an integer, set it as the maxRecordedElements property
                    if (int.TryParse(str[0], out int maxRecordedElements))
                        cls.maxRecordedElements = maxRecordedElements;
                        
                }
            }

            // Return the deserialized HtmClassifier
            return cls;
        }
  ~~~
Description about Deserialize function implemented in HtmClassifier class:  
    - The input stream is read and properly mapped to the parameters of the Htm Classifier class.  
    - The file can be fond in forked project location -> [Deserialize method](https://github.com/antonyaneeta/neocortexapi/blob/98ca46e17b8efe842471deaed73c530068ded8ef/source/NeoCortexApi/Classifiers/HtmClassifier.cs#L522)

### Auxiliary method added for De-serialization
~~~csharp
 public Dictionary<TIN, List<int[]>> ReadDictSIarrayList<TIN>(Dictionary<TIN, List<int[]>> m_AllInputs, String reader) {}
 ~~~
For deserialization logic added to read and add to keyValueMap  _ReadDictSIarrayList<TIN>(Dictionary<TIN, List<int[]>> m_AllInputs, String reader)_  [Click to see methods.](https://github.com/antonyaneeta/neocortexapi/blob/cec4993b41577690613e4b24bf1510471527e7f4/source/NeoCortexEntities/HtmSerializer.cs#L1935)


## If you need to compare 2 instance of Classifier , must override  Equals() 

[click to view changes in Git : Equals()](https://github.com/antonyaneeta/neocortexapi/blob/98ca46e17b8efe842471deaed73c530068ded8ef/source/NeoCortexApi/Classifiers/HtmClassifier.cs#L587)
1)The default Equals is override for HtmClassifier parameters.
2)It carefully checks all conditions required for the Equality of two instances of the Classifier Object
3)The refence object and compared object is checked for null, checked for type compatibility, presence of each fields,also checking deep inside each complex type object (m_AllInputs) if all keys and Values in KVP are also matching .
4)If all conditions pass the objects are found Equal and returns true.
5)If any conditions fails then method returns false.
<details>
    <summary> Click and open to see Code implementation for Equals </summary>
<!-- empty line -->  
~~~csharp  

        /// <summary>
        /// 
        /// The default Equals is override for HtmClassifier parameters.
        /// Ith carefully checks all conditions required for the Equality of two instances of the Classifier Object
        /// The refence object and compared object is checked for null, checked for type compatibility, presence of each fields, 
        /// also checking deep inside each complex type object (m_AllInputs) if all keys and Values in KVP are also matching .
        /// If all conditions pass the objects are found Equal and returns true.
        /// If any conditions fails then method returns false.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //Checks if the compared object is null or not
            if (obj == null)
                return false;
            //The type of objects compared here should be of type HTMClassifier or else equals fails
            if (typeof(HtmClassifier<TIN, TOUT>) != obj.GetType())
                return false;
            HtmClassifier<TIN, TOUT> other = (HtmClassifier<TIN, TOUT>)obj;
            if (maxRecordedElements != other.maxRecordedElements)
                return false;

            //check condition--> other.m_AllInputs  is null or empty and consequently with the first value 
            if (other.m_AllInputs == null || other.m_AllInputs.Count <= 0)
            {
                //if m_AllInputs  has a value this means 2 instances not equal.
                if (null != m_AllInputs || m_AllInputs.Count > 0)
                    return false;
            }

            //check condition--> m_AllInputs  is null and consequently  the other compared object is non null means the objects are unequal
            if (m_AllInputs == null || m_AllInputs.Count <= 0)
            {
                //if other.m_AllInputs  has a value this means 2 instances not equal.
                if (other.m_AllInputs != null || other.m_AllInputs.Count > 0)
                    return false;
            }
            //else if condition check --> "m_AllInputs" have same number of key value pairs as "other.m_AllInputs" 
            else if (m_AllInputs.Count != other.m_AllInputs.Count)
                return false;

            //check each key values in the m_AllInputs equality check if not same for any then only return false
            else
            {
                foreach (KeyValuePair<TIN, List<int[]>> val in other.m_AllInputs)
                {

                    foreach (KeyValuePair<TIN, List<int[]>> kvp in m_AllInputs)
                    {
                        if (kvp.Key.Equals(val.Key))
                        {

                            for (int i = 0; i < kvp.Value.Count; i++)
                            {
                                bool result = kvp.Value[i].ElementsEqual(val.Value[i]);

                                if (result == false)
                                    return false;
                            }

                        }

                    }
                }
            }

            // returns true if all parameters are found Equal
            return true;
        }  
          ~~~
</details>
<!-- empty line -->



## GetHashCode override    
A hash code is a numeric value that is used to insert and identify an object in a hash-based collection such as the Dictionary class, the Hashtable class, or a type derived from the DictionaryBase class. The GetHashCode method provides this hash code for algorithms that need quick checks of object equality.  
  - The Prime number hashing is used. Firstly, a prime odd prime number is chosen to calculate hash. We choose 31.
  -  Here as part of implementation in code, overriding Equals requires that you also override the GetHashCode method, otherwise, hash tables might not work correctly.   -The cumulative hash value of each parameter in the HTM Classifier in method GetHashCode    
  -The first parameter, maxRecordedElements being an int value we could directly get the hash. Now we need to aggregate the hash value of the second m_ AllInputs too.   -Next, the complex dictionary Key Value parameter needs to be taken care separately as it contains a List. For each value in the Dictionary collection, in consecutive steps we carefully consider the hash value of Keys along.
  <details>
    <summary> Click and open to see Code implementation for GetHashCode </summary>
<!-- empty line -->  
~~~csharp  

          public override int GetHashCode()
        {
            //TODO see code below the  HashCode Override implemented 
            unchecked 
            {
                //prime number as hash, results in distinct hashCode for distinct object
                //Here we choose 31 like in many implementations in our  neocortex code. As it is an Odd Prime .
                int prime = 31;
                int result = 1;

                // Calculate hash value considering integer value "maxRecordedElements"
                result = prime * result + maxRecordedElements;

                //Debug.WriteLine(String.Format("Hash value  -->: {0}", GetHashForDictionary(m_AllInputs)));
                /// The final hashCode value of the HTMClassifier is calculated
                result = prime * result + ((m_AllInputs != null && m_AllInputs.Count > 0) ? GetHashForDictionary(m_AllInputs) : 0);


                return result;
            }
        }

        /// <summary>
        /// Part of HashCode implementation
        /// The hash value is calculated as aggregate of keys and values in the Dictionary KeyValuePair parameter
        /// complex type dictionary parameter --> m_allInputs.
        /// </summary>
        public int GetHashForDictionary(Dictionary<TIN, List<int[]>> m_AllInputs)
        {
            int prime = 31;
            int hash = 0;
            foreach (var pair in m_AllInputs)
            {
                int miniHash = 17;

                //calculate hash value for the Dictionary params key
                miniHash = miniHash * prime + EqualityComparer<TIN>.Default.GetHashCode(pair.Key);

                ///calculate hash value for the Dictionary "Values" of each keyValuePair (in our case Value it is a List<int[]>)
                int listHashVal = GetHashCodeIntList(pair.Value);
                // Debug.WriteLine(String.Format("Hash value of List: {0}", listHashVal));

                ///Calculate aggregate hash value of keys and Values
                miniHash = miniHash * prime + listHashVal;
                
                //XOR the  hashCode of keys and  hashCode of value is XOR ed and total has calculated (the dictionary parameter m_AllInputs)
                hash ^= miniHash;
            }
            Debug.WriteLine(String.Format("key value pair hashCode : {0}", hash));
            return hash;

        }


        /// <summary>
        /// Calculate hash for all elements in the list of integer array in the m_allInputs Value
        /// </summary>
        public int GetHashCodeIntList(List<int[]> l)

        {
            unchecked
            {
                int prime = 31;
                int hash = 19;
                foreach (var foo in l)
                {
                    hash = hash * prime + GetHashIntArray(foo);
                }
                return hash;
            }
        }

        /// <summary>
        /// Calculate hash for all elements in the integer array of the parameter m_allInputs and return to the called method
        /// </summary>
        public int GetHashIntArray(int[] array)
        {
            unchecked
            {
                if (array == null)
                {
                    return 0;
                }
                int prime = 31;
                int hash = 17;
                foreach (int element in array)
                {
                    hash = hash * prime + EqualityComparer<int>.Default.GetHashCode(element);
                }
                return hash;
            }
        }  
          ~~~
</details>
<!-- empty line -->
  
  
## Testing

The Below figure 1 shows a snippet from serialised file saved. Figure 2 is capture durign deserialisation of this exact fiel and after parameter was properly set.

![Screenshot,serialized file](https://user-images.githubusercontent.com/74894210/232880842-f6f2260a-19f0-4e30-862f-2d75e5ef4065.png)

![parameters after deserialization](https://user-images.githubusercontent.com/74894210/232880963-98d753fd-5e74-41e6-aa7e-e220243b7363.png)


## How to evaluate the Test cases 
Unit Tests added available under various test category [TestCategory("ProjectUnitTests")],[TestCategory("Test-Serialization")], [TestCategory("Equals-Override-UnitTests")],[TestCategory("GetHashCode-Testing")]
[HtmClassiferSerializationTests.cs](https://github.com/antonyaneeta/neocortexapi/blob/98ca46e17b8efe842471deaed73c530068ded8ef/source/UnitTestsProject/Classifiers/HtmClassiferSerializationTests.cs#L42)
      
 * Find tests and the supplementary test resource in this location.
 
![UnitTestFiles](https://user-images.githubusercontent.com/74894210/228073005-bc4f4b8b-6294-4ac3-a87e-cc53e75c1979.png)  
 * Right click and Run Tests in namespace HtmClassifierUnitTest  

Picture below shows the UT code coverage of the Project.

![FinalTestRssultOutput](https://user-images.githubusercontent.com/74894210/232874030-a3729243-bee1-41d2-89cc-928c8c2d8b49.png)

#


Team Name : Team_Alpha
Team Members: Amith Nair, Rohit Suresh, Aneeta Antony.  

Link to Project:  [Team Alpha -Group Branch ](https://github.com/antonyaneeta/neocortexapi/compare/master...antonyaneeta:neocortexapi:team_alpha)  
Forked from: https://github.com/ddobric/neocortexapi

# **Outcome of Project**
Objective of this project is implementing serialization functionality to the HtmClassifier class.
The current state or values of the Classifier object is serialized or copied to a file with the help of StreamWriter.
The outputs are validated and demonstrated with help of Unit Test cases.  
  - [x] Serialize, Deserialize function ,Equals() Override GetHashCode() override... implemented in HtmClassifier class. 
  - [x] 2 params were identified for HtmClassifier class for serialization.   
  - [x] UT added for all new methods created.

#### Team Contribution Links:  
Consolidated commits and changes of all members available at -->  [ Team Alpha Branch ](https://github.com/antonyaneeta/neocortexapi/compare/master...antonyaneeta:neocortexapi:team_alpha)


