﻿using MyCloudProject.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyExperiment
{
    public class ExerimentRequestMessage : IExerimentRequestMessage
    {
        public string ExperimentId { get; set; }
        public string InputFile { get; set; }
        public string TestInputFile { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}


/*
 
 {
    "ExperimentId": "sasa",
    "InputFile":"sasss",

}
 
 */ 