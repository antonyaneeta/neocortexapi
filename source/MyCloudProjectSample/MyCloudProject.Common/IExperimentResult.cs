
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCloudProject.Common
{
    public interface IExperimentResult
    {
        string ExperimentId { get; set; }

        string Name { get; set; }

        string Description { get; set; }
        
        DateTime? StartTimeUtc { get; set; }

        DateTime? EndTimeUtc { get; set; }

        public long DurationSec { get; set; }
        double Accuracy { get; set; }
        
        // the field to hold the normal Predictors accuracy for the test sequence we check
        double NormalPredAccuracy { get; set; }


        public string testedPrediction { get; set; }

    }

}
