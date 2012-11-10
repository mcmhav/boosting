using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class ADABoost
    {
        public static List<Hypotheses> weightedMajorityHypotheses(List<Case> examples, Func<List<Case>, List<double>, Hypotheses> L, int M)
        {
            int N = examples.Count;
            List<Hypotheses> h = new List<Hypotheses>();
            List<double> weights = new List<double>();
            List<double> w = new List<double>();
            for (int i = 0; i < N; i++)
			{
			    w.Add((double)1/N);
			}
            List<double> z = new List<double>();
            for (int m = 0; m < M; m++)
            {
                h.Add(L(examples, w));
                double error = 0;
                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) != examples[j].classification) error += w[j];

                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) == examples[j].classification) w[j] *= error / (1 - error);

                double wTotal = w.Sum();
                for (int j = 0; j < N; j++) w[m] /= wTotal;
                

                h[m].setWeight(Math.Log(error / (1- error)));
            }

            return h;
        }

        public static double test(List<Hypotheses> hypotheses, List<Case> testSet)
        {
            double tse = 0;
            foreach (Case c in testSet)
            {
                tse += Math.Pow((classify(hypotheses, c.attributes) - c.classification), 2);
            }
            Console.WriteLine(testSet.Count);
            Console.WriteLine(tse);
            return tse / testSet.Count;
        }

        private static double classify(List<Hypotheses> hypotheses, List<double> attributes)
        {
            double classification = 0;
            foreach (Hypotheses h in hypotheses)
            {
                classification += h.classify(attributes) * h.weight;
                Console.WriteLine(h.weight);
                Console.WriteLine(h.classify(attributes));
            }
            return classification;
        }
    }
}