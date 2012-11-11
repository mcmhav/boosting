﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class ADABoost
    {
        public static List<Hypotheses> weightedMajorityHypotheses(List<Case> examples, Func<List<Case>, Hypotheses> L, int M)
        {
            int N = examples.Count;
            List<Hypotheses> h = new List<Hypotheses>();
            List<double> z = new List<double>();
            double weightTotal = 0;

            for (int i = 0; i < N; i++)
			{
			    examples[i].weight = ((double)1/N);
			}
            for (int m = 0; m < M; m++)
            {
                h.Add(L(examples));
                double error = 0;
                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) != examples[j].classification)
                        error += examples[j].weight;

                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) == examples[j].classification)
                        examples[j].weight *= error / (1 - error);

                double wTotal = examples.Sum(c => c.weight);
                for (int j = 0; j < N; j++) examples[j].weight /= wTotal;
                
                h[m].weight = Math.Log(error / (1- error));
                weightTotal += h[m].weight;
            }

            for (int m = 0; m < M; m++)
                h[m].weight /= weightTotal;

            return h;
        }

        public static double test(List<Hypotheses> hypotheses, List<Case> testSet)
        {
            double tse = 0;
            foreach (Case c in testSet)
            {
                Console.WriteLine("our: " + classify(hypotheses, c.attributes) + "   real: " + c.classification);
                tse += Math.Pow((classify(hypotheses, c.attributes) - c.classification), 2);
            }
            return tse / testSet.Count;
        }

        private static double classify(List<Hypotheses> hypotheses, List<double> attributes)
        {
            double classification = 0;
            foreach (Hypotheses h in hypotheses)
            {
                classification += h.classify(attributes) * h.weight;
            }
            return classification;
        }
    }
}