// Copyright (c) Damir Dobric. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
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
            //TODO

            HtmSerializer ser = new HtmSerializer();
            ser.SerializeBegin(nameof(HtmClassifier<TIN, TOUT>), sw);

            ser.SerializeValue(maxRecordedElements, sw);
            //if (typeof(double) == typeof(TIN))
            //{
            //    ser.SerializeValue(inputSequence.Cast<double>().ToList(), sw);
            //}
            //else if (typeof(string) == typeof(TIN))
            //{
            //    ser.SerializeValue(inputSequence.Cast<string>().ToList(), sw);
            //}


            // ser.SerializeValue(inputSequence, sw);


            ser.SerializeDictionaryValue(m_AllInputs, sw);

            // ser.SerializeValue(inputSequence, sw);
            ser.SerializeValue(m_AllInputs, sw);
            ser.SerializeEnd(nameof(HtmClassifier<TIN, TOUT>), sw);
        }
        #endregion

        #region DeserializationTrial
        public static object DeserializeTrial(StreamReader sr, string name)
        {
            //// TODO
            //int maxRecordedElements = default;
            //List<TIN> m_AllInputs = default;
            HtmClassifier<TIN, TOUT> htm = new HtmClassifier<TIN, TOUT>();

            while (sr.Peek() > 0)
            {
                var content = sr.ReadLine();
                if (content.StartsWith("Begin") && content.Contains(name))
                {
                    continue;
                }
                if (content.StartsWith("End") && content.Contains(name))
                {
                    break;
                }
                if (content.Contains(nameof(HtmClassifier<TIN, TOUT>.maxRecordedElements)))
                {
                    htm.maxRecordedElements = HtmSerializer.Deserialize<int>(sr, nameof(HtmClassifier<TIN, TOUT>.maxRecordedElements));
                }
                if (content.Contains(nameof(HtmClassifier<TIN, TOUT>.m_AllInputs)))
                {
                    //htm.m_AllInputs = HtmSerializer.Deserialize<List<TIN>>(sr, nameof(HtmClassifier<TIN, TOUT>.m_AllInputs));
                }
            }

            return htm ;
        }

        #endregion

        #region Deserialize
        /// <summary>
        /// Deserialize the Classifier Private fileds
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        //public HtmClassifier<TIN, TOUT> Deserialize(StreamReader sr)
        //{
        //    //throw new NotImplementedException();
        //    HtmSerializer ser = new HtmSerializer();
        //    HtmClassifier<TIN, TOUT> cls = new HtmClassifier<TIN, TOUT>();
        //    Dictionary<TIN, int[]> keyValues = new Dictionary<TIN, int[]>();

        //    while (sr.Peek() >= 0)
        //    {
        //        string data = sr.ReadLine();
        //        if (data == string.Empty || data == ser.ReadBegin(nameof(HtmClassifier<TIN, TOUT>)))
        //        {
        //            continue;
        //        }
        //        else if (data == ser.ReadEnd(nameof(HtmClassifier<TIN, TOUT>)))
        //        {
        //            break;
        //        }
        //        else if (data.Contains(HtmSerializer.KeyValueDelimiter))
        //        {
        //            //string[] str = data.Split(HtmSerializer.ParameterDelimiter);
        //            //for (int i = 0; i < str.Length; i++)
        //            // {
        //            //string[] str1 = data.Split(HtmSerializer.ParameterDelimiter);
        //            for (int j = 0; j < data.Length - 1; j++)
        //            {
        //                switch (j)
        //                {
        //                    case 0:
        //                        cls.m_AllInputs = ser.ReadDictSIarray1<TIN>(cls.m_AllInputs, data);
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }
        //        //}
        //        else
        //        {
        //            string[] str = data.Split(HtmSerializer.ParameterDelimiter);
        //            for (int i = 0; i < str.Length; i++)
        //            {
        //                switch (i)
        //                {
        //                    case 0:
        //                        {
        //                            cls.maxRecordedElements = ser.ReadIntValue(str[i]);
        //                            break;
        //                        }
        //                        //case 1:
        //                        //    {
        //                        //        string[] str1 = data.Split(HtmSerializer.ParameterDelimiter);
        //                        //        for (int j = 0; i < str.Length; i++)
        //                        //        {
        //                        //            switch (i)
        //                        //            {
        //                        //                case 0:
        //                        //                    cls.m_AllInputs = ser.ReadDictSIarray1<TIN>(str[j]);
        //                        //                    break;
        //                        //                default:
        //                        //                    break;
        //                        //            }
        //                        //        }

        //                        //        //cls.m_AllInputs = ser.ReadKeyISValue(str[i]);
        //                        //        break;
        //                        //    }


        //                }
        //            }
        //            //return cls;


        //        }
        //    }

        //    return cls;
        //}
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
                    if (str.Length == 0)
                        continue;

                    // If the first parameter is an integer, set it as the maxRecordedElements property
                    if (int.TryParse(str[0], out int maxRecordedElements))
                        cls.maxRecordedElements = maxRecordedElements;
                }
            }

            // Return the deserialized HtmClassifier
            return cls;
        }


        #endregion




        #region For refernce

        //public void Serialize(object obj, string name, StreamWriter sw)
        //{
        //    var excludeMembers = new List<string>
        //    {
        //        nameof(EncoderBase.Properties),
        //        nameof(EncoderBase.halfWidth),
        //        nameof(EncoderBase.rangeInternal),
        //        nameof(EncoderBase.nInternal),
        //        nameof(EncoderBase.encLearningEnabled),
        //        nameof(EncoderBase.flattenedFieldTypeList),
        //        nameof(EncoderBase.decoderFieldTypes),
        //        nameof(EncoderBase.topDownValues),
        //        nameof(EncoderBase.bucketValues),
        //        nameof(EncoderBase.topDownMapping),

        //    };
        //    HtmSerializer.SerializeObject(obj, name, sw, ignoreMembers: excludeMembers);
        //}
        #endregion
    }

}


