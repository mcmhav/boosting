using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class ADABoost
    {
        public static List<Hypotheses> weightedMajorityHypotheses(List<Case> examples, Func<List<Case>, Hypotheses> L, int M, double binaryRatio, bool log)
        {
            int N = examples.Count;
            List<Hypotheses> h = new List<Hypotheses>();
            double weightTotal = 0;

            for (int i = 0; i < N; i++) examples[i].weight = ((double)1/N);

            for (int m = 0; m < M; m++)
            {
                h.Add(L(examples));
                double error = 0;
                double hError = 0;
                for (int j = 0; j < N; j++)
                {
                    if (h[m].classify(examples[j].attributes) != examples[j].classification)
                    {
                        error += examples[j].weight;
                        hError += (double)1 / N;
                    }
                }
                error *= binaryRatio;
                hError *= binaryRatio;

                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) == examples[j].classification)
                        examples[j].weight *= error / (1 - error);

                //if (hError > 0.5)
                //{
                //    h.RemoveAt(h.Count - 1);
                //    m--;
                //    continue;
                //}

                double wTotal = examples.Sum(c => c.weight);
                for (int j = 0; j < N; j++)
                {
                    examples[j].weight /= wTotal;
                }

                if (log)
                {
                    //Console.WriteLine("Error: " + error);
                    //Console.WriteLine("HError: " + hError);
                    //Console.WriteLine("Weight: " + examples.Sum(c => c.weight));
                }
                h[m].weight = Math.Log((1 - error) / error);
                weightTotal += h[m].weight;
            }

            //for (int m = 0; m < M; m++)
            //    h[m].weight /= weightTotal;

            return h;
        }

        public static Tuple<double, double, double, double> test(List<Hypotheses> hypotheses, List<Case> testSet, bool log)
        {
            double seTotal = 0;
            double wrongCount = 0;
            List<double> errors = new List<double>();
            foreach (Case c in testSet)
            {
                double differance = Math.Abs(classify(hypotheses, c.attributes) - c.classification);
                errors.Add(differance);
                if (differance != 0)
                {
                    seTotal += Math.Pow(differance, 2);
                    wrongCount++;
                }
            }
            double avgError = errors.Average();
            double sd = DataStatistics.standardDeviation(errors);
            double rightPercentage = (1 - wrongCount / testSet.Count) * 100;
            double mse = seTotal / testSet.Count;
            return new Tuple<double, double, double, double>(mse, rightPercentage, avgError, sd);
        }

        private static double classify(List<Hypotheses> H, List<double> attributes)
        {
            double classification = H.GroupBy(h => h.classify(attributes)).OrderByDescending(g => g.Sum(h => h.weight)).First().Key;
            return classification;
        }
    }
}