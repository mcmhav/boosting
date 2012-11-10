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
			    w.Add(1/N);
			}
            List<double> z = new List<double>();
            for (int m = 1; m < M; m++)
            {
                h[m] = L(examples, w);
                double error = 0;
                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) != examples[j].classification) error += + w[j];

                for (int j = 0; j < N; j++)
                    if (h[m].classify(examples[j].attributes) == examples[j].classification) w[j] *= error / (1 - error);

                double wTotal = w.Sum();
                for (int j = 0; j < N; j++) w[m] /= wTotal;
                
                h[m].setWeight(Math.Log(error / (1- error)));
            }

            return h;
        }
    }
}