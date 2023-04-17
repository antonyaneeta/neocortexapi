﻿// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NeoCortexApi.Classifiers
{
    /// <summary>
    /// Defines the predicting input.
    /// </summary>
    public class ClassifierResult<TIN>
    {
        /// <summary>
        /// The predicted input value.
        /// </summary>
        public TIN PredictedInput { get; set; }

        /// <summary>
        /// Number of identical non-zero bits in the SDR.
        /// </summary>
        public int NumOfSameBits { get; set; }

        /// <summary>
        /// The similarity between the SDR of  predicted cell set with the SDR of the input.
        /// </summary>
        public double Similarity { get; set; }
        }


    /// <summary>
    /// Classifier implementation which memorize all seen values.
    /// </summary>
    /// <typeparam name="TIN"></typeparam>
    /// <typeparam name="TOUT"></typeparam>
    
    public class HtmClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>,ISerializable


    {
        private int maxRecordedElements = 10;

        //private List<TIN> inputSequence = new List<TIN>();

        //private Dictionary<int[], int> inputSequenceMap = new Dictionary<int[], int>();

        /// <summary>
        /// Recording of all SDRs. See maxRecordedElements.
        /// </summary>
        public HtmClassifier()
        { }
        private Dictionary<TIN, List<int[]>> m_AllInputs = new Dictionary<TIN, List<int[]>>();

        /// <summary>
        /// Mapping between the input key and the SDR assootiated to the input.
        /// </summary>
        //private Dictionary<TIN, int[]> m_ActiveMap2 = new Dictionary<TIN, int[]>();

        /// <summary>
        /// Clears th elearned state.
        /// </summary>
        public void ClearState()
        {
            m_AllInputs.Clear();
        }

        /// <summary>
        /// Checks if the same SDR is already stored under the given key.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sdr"></param>
        /// <returns></returns>
        private bool ContainsSdr(TIN input, int[] sdr)
        {
            foreach (var item in m_AllInputs[input])
            {
                if (item.SequenceEqual(sdr))
                    return true;
                else
                    return false;
            }

            return false;
        }


        private int GetBestMatch(TIN input, int[] cellIndicies, out double similarity, out int[] bestSdr)
        {
            int maxSameBits = 0;
            bestSdr = new int[1];

            foreach (var sdr in m_AllInputs[input])
            {
                var numOfSameBitsPct = sdr.Intersect(cellIndicies).Count();
                if (numOfSameBitsPct >= maxSameBits)
                {
                    maxSameBits = numOfSameBitsPct;
                    bestSdr = sdr;
                }
            }

            similarity = Math.Round(MathHelpers.CalcArraySimilarity(bestSdr, cellIndicies), 2);

            return maxSameBits;
        }


        /// <summary>
        /// Assotiate specified input to the given set of predictive cells.
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP.</param>
        public void Learn(TIN input, Cell[] output)
        {
            var cellIndicies = GetCellIndicies(output);

            Learn(input, cellIndicies);
        }

        /// <summary>
        /// Assotiate specified input to the given set of predictive cells. This can also be used to classify Spatial Pooler Columns output as int array
        /// </summary>
        /// <param name="input">Any kind of input.</param>
        /// <param name="output">The SDR of the input as calculated by SP as int array</param>
        public void Learn(TIN input, int[] cellIndicies)
        {
            if (m_AllInputs.ContainsKey(input) == false)
                m_AllInputs.Add(input, new List<int[]>());

            // Store the SDR only if it was not stored under the same key already.
            if (!ContainsSdr(input, cellIndicies))
                m_AllInputs[input].Add(cellIndicies);
            else
            {
                // for debugging
            }

            //
            // Make sure that only few last SDRs are recorded.
            if (m_AllInputs[input].Count > maxRecordedElements)
            {
                Debug.WriteLine($"The input {input} has more ");
                m_AllInputs[input].RemoveAt(0);
            }

            var previousOne = m_AllInputs[input][Math.Max(0, m_AllInputs[input].Count - 2)];

            if (!previousOne.SequenceEqual(cellIndicies))
            {
                // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / Math.Max((double)cellIndicies.Length, this.activeMap2[input].Length)));
                // double numOfSameBitsPct = (double)(((double)(this.activeMap2[input].Intersect(cellIndicies).Count()) / (double)this.activeMap2[input].Length));
                var numOfSameBitsPct = previousOne.Intersect(cellIndicies).Count();
                Debug.WriteLine($"Prev/Now/Same={previousOne.Length}/{cellIndicies.Length}/{numOfSameBitsPct}");
            }
        }

        /// <summary>
        /// Gets multiple predicted values.
        /// </summary>
        /// <param name="predictiveCells">The current set of predictive cells.</param>
        /// <param name="howMany">The number of predections to return.</param>
        /// <returns>List of predicted values with their similarities.</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(Cell[] predictiveCells, short howMany = 1)
        {
            var cellIndicies = GetCellIndicies(predictiveCells);

            return GetPredictedInputValues(cellIndicies, howMany);
        }

        /// <summary>
        /// Gets multiple predicted values. This can also be used to classify Spatial Pooler Columns output as int array
        /// </summary>
        /// <param name="predictiveCells">The current set of predictive cells in int array.</param>
        /// <param name="howMany">The number of predections to return.</param>
        /// <returns>List of predicted values with their similarities.</returns>
        public List<ClassifierResult<TIN>> GetPredictedInputValues(int[] cellIndicies, short howMany = 1)
        {
            List<ClassifierResult<TIN>> res = new List<ClassifierResult<TIN>>();
            double maxSameBits = 0;
            TIN predictedValue = default;
            Dictionary<TIN, ClassifierResult<TIN>> dict = new Dictionary<TIN, ClassifierResult<TIN>>();

            var predictedList = new List<KeyValuePair<double, string>>();
            if (cellIndicies.Length != 0)
            {
                int indxOfMatchingInp = 0;
                Debug.WriteLine($"Item length: {cellIndicies.Length}\t Items: {this.m_AllInputs.Keys.Count}");
                int n = 0;

                List<int> sortedMatches = new List<int>();

                Debug.WriteLine($"Predictive cells: {cellIndicies.Length} \t {Helpers.StringifyVector(cellIndicies)}");

                foreach (var pair in this.m_AllInputs)
                {
                    if (ContainsSdr(pair.Key, cellIndicies))
                    {
                        Debug.WriteLine($">indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{cellIndicies.Length}, Same Bits = {cellIndicies.Length.ToString("D3")}\t, Similarity 100.00 %\t {Helpers.StringifyVector(cellIndicies)}");

                        res.Add(new ClassifierResult<TIN> { PredictedInput = pair.Key, Similarity = (float)100.0, NumOfSameBits = cellIndicies.Length });
                    }
                    else
                    {
                        // Tried following:
                        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(arr).Count()) / Math.Max(arr.Length, pair.Value.Count())));
                        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(celIndicies).Count()) / (double)pair.Value.Length));// ;
                        double similarity;
                        int[] bestMatch;
                        var numOfSameBitsPct = GetBestMatch(pair.Key, cellIndicies, out similarity, out bestMatch);// pair.Value.Intersect(cellIndicies).Count();
                        //double simPercentage = Math.Round(MathHelpers.CalcArraySimilarity(pair.Value, cellIndicies), 2);
                        dict.Add(pair.Key, new ClassifierResult<TIN> { PredictedInput = pair.Key, NumOfSameBits = numOfSameBitsPct, Similarity = similarity });
                        predictedList.Add(new KeyValuePair<double, string>(similarity, pair.Key.ToString()));

                        if (numOfSameBitsPct > maxSameBits)
                        {
                            Debug.WriteLine($">indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{bestMatch.Length}, Same Bits = {numOfSameBitsPct.ToString("D3")}\t, Similarity {similarity.ToString("000.00")} % \t {Helpers.StringifyVector(bestMatch)}");
                            maxSameBits = numOfSameBitsPct;
                            predictedValue = pair.Key;
                            indxOfMatchingInp = n;
                        }
                        else
                            Debug.WriteLine($"<indx:{n.ToString("D3")}\tinp/len: {pair.Key}/{bestMatch.Length}, Same Bits = {numOfSameBitsPct.ToString("D3")}\t, Similarity {similarity.ToString("000.00")} %\t {Helpers.StringifyVector(bestMatch)}");
                    }
                    n++;
                }
            }

            int cnt = 0;
            foreach (var keyPair in dict.Values.OrderByDescending(key => key.Similarity))
            {
                res.Add(keyPair);
                if (++cnt >= howMany)
                    break;
            }

            return res;
        }



        /// <summary>
        /// Gets predicted value for next cycle
        /// </summary>
        /// <param name="predictiveCells">The list of predictive cells.</param>
        /// <returns></returns>
        [Obsolete("This method will be removed in the future. Use GetPredictedInputValues instead.")]
        public TIN GetPredictedInputValue(Cell[] predictiveCells)
        {
            throw new NotImplementedException("This method will be removed in the future. Use GetPredictedInputValues instead.");
            // bool x = false;
            //double maxSameBits = 0;
            //TIN predictedValue = default;

            //if (predictiveCells.Length != 0)
            //{
            //    int indxOfMatchingInp = 0;
            //    Debug.WriteLine($"Item length: {predictiveCells.Length}\t Items: {m_ActiveMap2.Keys.Count}");
            //    int n = 0;

            //    List<int> sortedMatches = new List<int>();

            //    var celIndicies = GetCellIndicies(predictiveCells);

            //    Debug.WriteLine($"Predictive cells: {celIndicies.Length} \t {Helpers.StringifyVector(celIndicies)}");

            //    foreach (var pair in m_ActiveMap2)
            //    {
            //        if (pair.Value.SequenceEqual(celIndicies))
            //        {
            //            Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length}\tsimilarity 100pct\t {Helpers.StringifyVector(pair.Value)}");
            //            return pair.Key;
            //        }

            //        // Tried following:
            //        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(arr).Count()) / Math.Max(arr.Length, pair.Value.Count())));
            //        //double numOfSameBitsPct = (double)(((double)(pair.Value.Intersect(celIndicies).Count()) / (double)pair.Value.Length));// ;
            //        var numOfSameBitsPct = pair.Value.Intersect(celIndicies).Count();
            //        if (numOfSameBitsPct > maxSameBits)
            //        {
            //            Debug.WriteLine($">indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");
            //            maxSameBits = numOfSameBitsPct;
            //            predictedValue = pair.Key;
            //            indxOfMatchingInp = n;
            //        }
            //        else
            //            Debug.WriteLine($"<indx:{n}\tinp/len: {pair.Key}/{pair.Value.Length} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Value)}");

            //        n++;
            //    }
            //}

            //return predictedValue;
        }
        /*
        //
        // This loop peeks the best input
        foreach (var pair in this.activeMap)
        {
            //
            // We compare only outputs which are similar in the length.
            // This is important, because some outputs, which are not related to the comparing output
            // might have much mode cells (length) than the current output. With this, outputs with much more cells
            // would be declared as matching outputs even if they are not.
            if ((Math.Min(arr.Length, pair.Key.Length) / Math.Max(arr.Length, pair.Key.Length)) > 0.9)
            {
                double numOfSameBitsPct = (double)((double)(pair.Key.Intersect(arr).Count() / (double)arr.Length));
                if (numOfSameBitsPct > maxSameBits)
                {
                    Debug.WriteLine($"indx:{n}\tbits/arrbits: {pair.Key.Length}/{arr.Length}\t{pair.Value} = similarity {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                    maxSameBits = numOfSameBitsPct;
                    predictedValue = pair.Value;
                    indxOfMatchingInp = n;
                }

                //if (maxSameBits > 0.9)
                //{
                //    sortedMatches.Add(n);
                //    // We might have muliple matchin candidates.
                //    // For example: Let the matchin input be i1
                //    // I1 - c1, c2, c3, c4
                //    // I2 - c1, c2, c3, c4, c5, c6

                //    Debug.WriteLine($"cnt:{n}\t{pair.Value} = bits {numOfSameBitsPct}\t {Helpers.StringifyVector(pair.Key)}");
                //}
            }
            n++;
        }

        foreach (var item in sortedMatches)
        {

        }

        Debug.Write("[ ");
        for (int i = Math.Max(0, indxOfMatchingInp - 3); i < Math.Min(indxOfMatchingInp + 3, this.activeMap.Keys.Count); i++)
        {
            if (i == indxOfMatchingInp) Debug.Write("* ");
            Debug.Write($"{this.inputSequence[i]}");
            if (i == indxOfMatchingInp) Debug.Write(" *");

            Debug.Write(", ");
        }
        Debug.WriteLine(" ]");

        return predictedValue;
        //return activeMap[ComputeHash(FlatArray(output))];
    }
    return default(TIN);
    }*/


        /// <summary>
        /// Traces out all cell indicies grouped by input value.
        /// </summary>
        public string TraceState(string fileName = null)
        {
            StringWriter strSw = new StringWriter();

            StreamWriter sw = null;

            if (fileName != null)
                sw = new StreamWriter(fileName);

            List<TIN> processedValues = new List<TIN>();

            //
            // Trace out the last stored state.
            foreach (var item in this.m_AllInputs)
            {
                strSw.WriteLine("");
                strSw.WriteLine($"{item.Key}");
                strSw.WriteLine($"{Helpers.StringifyVector(item.Value.Last())}");
            }

            strSw.WriteLine("........... Cell State .............");

            foreach (var item in m_AllInputs)
            {
                strSw.WriteLine("");

                strSw.WriteLine($"{item.Key}");

                strSw.Write(Helpers.StringifySdr(new List<int[]>(item.Value)));

                //foreach (var cellState in item.Value)
                //{
                //    var str = Helpers.StringifySdr(cellState);
                //    strSw.WriteLine(str);
                //}
            }

            if (sw != null)
            {
                sw.Write(strSw.ToString());
                sw.Flush();
                sw.Close();
            }

            Debug.WriteLine(strSw.ToString());
            return strSw.ToString();
        }



        /*
    /// <summary>
    /// Traces out all cell indicies grouped by input value.
    /// </summary>
    public void TraceState2(string fileName = null)
    {

        List<TIN> processedValues = new List<TIN>();

        foreach (var item in activeMap.Values)
        {
            if (processedValues.Contains(item) == false)
            {
                StreamWriter sw = null;

                if (fileName != null)
                    sw = new StreamWriter(fileName.Replace(".csv", $"_Digit_{item}.csv"));

                Debug.WriteLine("");
                Debug.WriteLine($"{item}");

                foreach (var inp in this.activeMap.Where(i => EqualityComparer<TIN>.Default.Equals((TIN)i.Value, item)))
                {
                    Debug.WriteLine($"{Helpers.StringifyVector(inp.Key)}");

                    if (sw != null)
                        sw.WriteLine($"{Helpers.StringifyVector(inp.Key)}");
                }

                if (sw != null)
                {
                    sw.Flush();
                    sw.Close();
                }

                processedValues.Add(item);
            }
        }
    }
     */


        private string ComputeHash(byte[] rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(rawData);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        private static byte[] FlatArray(Cell[] output)
        {
            byte[] arr = new byte[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = (byte)output[i].Index;
            }
            return arr;
        }

        private static int[] GetCellIndicies(Cell[] output)
        {
            int[] arr = new int[output.Length];
            for (int i = 0; i < output.Length; i++)
            {
                arr[i] = output[i].Index;
            }
            return arr;
        }

        private int PredictNextValue(int[] activeArr, int[] predictedArr)
        {
            var same = predictedArr.Intersect(activeArr);

            return same.Count();
        }

        #region Serialization
        /// <summary>
        /// Implement Serialization for HtmClassifer
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="sw"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize(object obj, string name, StreamWriter sw)
        {
            //Serialization code below.

            HtmSerializer ser = new HtmSerializer();
            ser.SerializeBegin(nameof(HtmClassifier<TIN, TOUT>), sw);
            ser.SerializeValue(maxRecordedElements, sw);
            ser.SerializeDictionaryValue(m_AllInputs, sw);
            ser.SerializeEnd(nameof(HtmClassifier<TIN, TOUT>), sw);
        }

        #endregion

        #region Deserialize
        /// <summary>
        /// Deserialize the Classifier Private fields and assigns them to the fields of the Class instance.
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        
        public HtmClassifier<TIN, TOUT> Deserialize(StreamReader sr)
        {
            // Create a new HtmSerializer and HtmClassifier
            HtmSerializer ser = new HtmSerializer();
            HtmClassifier<TIN, TOUT> cls = new HtmClassifier<TIN, TOUT>();

            // Read the input stream line by line
            while (!sr.EndOfStream)
            {
                // Read the current line
                string data = sr.ReadLine();

                // Skip empty lines and the beginning and end of the HtmClassifier
                if (string.IsNullOrEmpty(data))
                    continue;

                if (data == ser.ReadBegin(nameof(HtmClassifier<TIN, TOUT>)))
                    continue;

                if (data == ser.ReadEnd(nameof(HtmClassifier<TIN, TOUT>)))
                    break;

                // If the line contains a key-value pair, deserialize it
                if (data.Contains(HtmSerializer.KeyValueDelimiter))
                {

                    var kvp = ser.ReadDictSIarrayList<TIN>(cls.m_AllInputs, data);
                    cls.m_AllInputs = kvp;

                }
                // Otherwise, parse the parameters in the line and set them in the HtmClassifier
                else
                {
                    // Split the line into its parameters
                    string[] str = data.Split(HtmSerializer.ParameterDelimiter);


                    // Skip lines with no parameters
                    foreach (string value in str)
                    {
                        String.IsNullOrWhiteSpace(value);
                        continue;
                    }
                            

                    // If the first parameter is an integer, set it as the maxRecordedElements property
                    if (int.TryParse(str[0], out int maxRecordedElements))
                        cls.maxRecordedElements = maxRecordedElements;
                }
            }

            // Return the deserialized HtmClassifier
            return cls;
        }
        #endregion


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


        /// <summary>
        /// HashCode override to keep the Equals HashCode contract 
        /// Here GetHAshCode is override. 
        /// Prime number hashing used
        /// The hash value is calculated as aggregate of  each parameters of HTMClassifier class. 
        /// So for the Complex type dictionary parameter(m_allInputs) we need to calculate the hash value accordingly.
        /// </summary>
        /// <returns></returns>
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

    }

}


