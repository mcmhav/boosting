using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class KNearest : Hypotheses
    {
        private static bool trainK = false;
        public static int K = 25;

        public static Hypotheses generateHypothesis(List<Case> cases)
        {
            KNearest h = new KNearest();
            h.train(cases);
            return h;
        }

        private List<Case> trainingSet;
        private List<double> distanceunits;
        private bool weighted = true;

        public void train(List<Case> trainingSet)
        {
            this.trainingSet = trainingSet;
            this.distanceunits = new List<double>();
            for (int i = 0; i < trainingSet.First().attributes.Count; i++)
            {
                List<double> attributeValues = new List<double>();
                foreach (Case c in trainingSet) attributeValues.Add(c.attributes[i]);
                distanceunits.Add(DataStatistics.standardDeviation(attributeValues));
            }

            Console.WriteLine("Training... Finding best distanceunits");
            trainDistanceunits();
            if (trainK)
            {
                Console.WriteLine("Training... Finding best K");
                findBestK();
            }
        }

        public override void print()
        {
            Console.WriteLine(ToString());
        }

        public override string ToString()
        {
            return "K nearest nabours";
        }

        private void findBestK()
        {
            List<int> kValues = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 25, 30, 40, 50 };
            int bestK = 0;
            double bestMSE = 1000;
            double currentMSE;
            for (int i = 0; i < kValues.Count; i++)
            {
                K = kValues[i];
                currentMSE = 0;
                foreach (Case c in trainingSet)
                {
                    currentMSE += Math.Pow(classify(c.attributes, null, true) - c.classification, 2) * c.weight;
                }
                currentMSE /= trainingSet.Count;
                if (currentMSE < bestMSE)
                {
                    bestK = K;
                    bestMSE = currentMSE;
                }
                
            }
            K = bestK;
            Console.WriteLine("TrainK: bestMSE = " + bestMSE);
        }

        public void trainDistanceunits()
        {
            List<double> tempDistanceUnits = new List<double>();
            for (int i = 0; i < distanceunits.Count; i++) tempDistanceUnits.Add(0);
            distanceunits = trainDistanceunit(0, tempDistanceUnits).Item2;

            //for (int j = 0; j < trainingSet.First().attributes.Count; j++)
            //{
            //    List<double> dValues = new List<double>() { 0.1, 0.2, 0.5, 1, 2, 4, 8};
            //    double bestD = 0;
            //    double bestMSE = 1000;
            //    double currentMSE;
            //    for (int i = 0; i < dValues.Count; i++)
            //    {
            //        distanceunits[j] *= dValues[i];
            //        currentMSE = 0;
            //        foreach (Case c in trainingSet)
            //        {
            //            currentMSE += Math.Pow(classify(c.attributes) - c.classification, 2) * c.weight;
            //        }
            //        currentMSE /= trainingSet.Count;
            //        if (currentMSE < bestMSE)
            //        {
            //            bestD = distanceunits[j];
            //            bestMSE = currentMSE;
            //        }
            //        distanceunits[j] /= dValues[i];
            //    }
            //    distanceunits[j] = bestD;
            //}
        }

        public Tuple<double, List<double>> trainDistanceunit(int index, List<double> tempDistanceUnits)
        {
            List<double> dValues = new List<double>() { 1, 2 };
            double bestD = 1;
            double bestMSE = 1000;
            double currentMSE;
            for (int i = 0; i < dValues.Count; i++)
            {
                String output = "";
                for (int j = 0; j < index + 1; j++) output += "\t";
                Console.WriteLine(output + i + " of " + (dValues.Count - 1));
                tempDistanceUnits[index] = distanceunits[index] * dValues[i];
                if (index == trainingSet.First().attributes.Count - 1)
                {
                    currentMSE = 0;
                    foreach (Case c in trainingSet)
                    {
                        currentMSE += Math.Pow(classify(c.attributes, tempDistanceUnits) - c.classification, 2) * c.weight;
                    }
                    currentMSE /= trainingSet.Count;

                    if (currentMSE < bestMSE)
                    {
                        bestD = tempDistanceUnits[index];
                        bestMSE = currentMSE;
                    }
                }
                else
                {
                    Tuple<double, List<double>> res = trainDistanceunit(index + 1, tempDistanceUnits);
                    currentMSE = res.Item1;

                    if (currentMSE < bestMSE)
                    {
                        bestD = tempDistanceUnits[index];
                        bestMSE = currentMSE;
                    }
                }
            }
            tempDistanceUnits[index] = bestD;
            return new Tuple<double,List<double>>(bestMSE, tempDistanceUnits);
        }

        public double classify(List<double> attributes, List<double> tempDistanceUnits = null, bool cInTrainingSet = false)
        {
            List<Tuple<double, Case>> kNearest = findKNearest(attributes, distanceunits, cInTrainingSet);
            double classification = 0;
            if (weighted)
            {
                double totalSquaredDistance = kNearest.Sum(t => t.Item1);
                foreach (Tuple<double, Case> t in kNearest)
                {
                    classification += t.Item2.classification * (t.Item1 / totalSquaredDistance);
                }
                classification = Math.Round(classification);
            }
            else
            {
                foreach (Tuple<double, Case> t in kNearest)
                {
                    classification += t.Item2.classification;
                }
                classification = Math.Round(classification / K);
            }
            return (int)classification;
        }

        public override double classify(List<double> attributes)
        {
            List<Tuple<double, Case>> kNearest = findKNearest(attributes);
            double classification = 0;
            if (weighted)
            {
                double totalSquaredDistance = kNearest.Sum(t => t.Item1);
                foreach (Tuple<double, Case> t in kNearest)
                {
                    classification += t.Item2.classification * (t.Item1 / totalSquaredDistance);
                }
                classification = Math.Round(classification);
            }
            else
            {
                foreach (Tuple<double, Case> t in kNearest)
                {
                    classification += t.Item2.classification;
                }
                classification = Math.Round(classification / K);
            }
            return (int)classification;
        }

        private List<Tuple<double, Case>> findKNearest(List<double> attributes, List<double> tempDistanceUnits = null, bool cInTrainingSet = false)
        {
            List<Tuple<double, Case>> kNearest = new List<Tuple<double, Case>>();
            foreach (Case c2 in trainingSet)
            {
                double distance = 0;
                for (int i = 0; i < trainingSet.First().attributes.Count; i++)
                {
                    if(tempDistanceUnits == null) distance += Math.Pow(Math.Abs(attributes[i] - c2.attributes[i]) / distanceunits[i], 2);
                    else distance += Math.Pow(Math.Abs(attributes[i] - c2.attributes[i]) / tempDistanceUnits[i], 2);
                }
                kNearest.Add(new Tuple<double, Case>(distance, c2));
            }
            if (cInTrainingSet)
            {
                kNearest = kNearest.OrderBy(t => t.Item1).Take(K + 1).ToList();
                kNearest.Remove(new Tuple<double, Case>(0, trainingSet.First(c => c.attributes.Equals(attributes))));
                if (kNearest.Count != K) throw new Exception();
                //kNearest.RemoveAll(t => t.Item2 == c);
            }
            else kNearest = kNearest.OrderBy(t => t.Item1).Take(K).ToList();
            return kNearest;
        }
    }
}