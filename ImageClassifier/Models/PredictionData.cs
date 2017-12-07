using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageClassifier.Models
{
    public class PredictionData
    {
        public string Tag;
        public double Probability;
        public string PercentageProbability;
    }
}