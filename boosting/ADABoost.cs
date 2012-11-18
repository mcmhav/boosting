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

                double wTotal = examples.Sum(c => c.weight);
                for (int j = 0; j < N; j++)
                {
                    examples[j].weight /= wTotal;
                }

                if (log)
                {
                    Console.WriteLine("Error: " + error);
                    Console.WriteLine("HError: " + hError);
                    Console.WriteLine("Weight: " + examples.Sum(c => c.weight));
                }
                h[m].weight = Math.Log((1 - error) / error);
                weightTotal += h[m].weight;
            }

            //for (int m = 0; m < M; m++)
            //    h[m].weight /= weightTotal;

            return h;
        }

        public static Tuple<double, double> test(List<Hypotheses> hypotheses, List<Case> testSet, bool log)
        {
            double seTotal = 0;
            double wrongCount = 0;
            foreach (Case c in testSet)
            {
                if(log) Console.WriteLine("our: " + classify(hypotheses, c.attributes) + "   real: " + c.classification);
                double differance = classify(hypotheses, c.attributes) - c.classification;
                if (differance != 0)
                {
                    seTotal += Math.Pow(differance, 2);
                    wrongCount++;
                }
            }
            double rightPercentage = 1 - wrongCount / testSet.Count;
            double mse = seTotal / testSet.Count;
            return new Tuple<double, double>(mse, rightPercentage);
        }

        private static double classify(List<Hypotheses> H, List<double> attributes)
        {
            double classification = H.GroupBy(h => h.classify(attributes)).OrderByDescending(g => g.Sum(h => h.weight)).First().Key;
            return classification;
        }
    }
}