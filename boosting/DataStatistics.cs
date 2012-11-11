﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class DataStatistics
    {
        public static double standardDeviation(List<double> values)
        {
            double mean = values.Average();
            double sd = 0;
            foreach (double val in values) sd += Math.Pow(Math.Abs(val - mean), 2);
            sd /= values.Count;
            sd = Math.Pow(sd, 0.5);
            return sd;
        }

        public static double entropy(List<Case> cases)
        {
            int numOfclassificationValues = cases.GroupBy(c => c.classification).Count();
            double entropy = 0;
            int totalWeight = (int) cases.Sum(c => c.weight);
            var groupings = cases.GroupBy(c => c.classification).ToList();
            foreach (var g in groupings)
            {
                double proportion = (double) g.Sum(c => c.weight) / totalWeight;
                entropy -= proportion * Math.Log(proportion, numOfclassificationValues);
            }
            return entropy;
        }
    }
}