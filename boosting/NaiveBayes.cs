using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class NaiveBayes : Hypotheses
    {
        private static readonly int numOfGroupings = 4;

        private List<ClassGrouping> classProbabilities;
        private List<AttrGroupings> attrProbabilities;

        public static Hypotheses generateHypothesis(List<Case> cases)
        {
            return new NaiveBayes(cases);
        }

        public NaiveBayes(List<Case> cases)
        {
            this.classProbabilities = new List<ClassGrouping>();
            this.attrProbabilities = new List<AttrGroupings>();

            for (int i = 0; i < cases.First().attributes.Count; i++)
                attrProbabilities[i] = new AttrGroupings(cases, i);

            var originalGroupings = cases.GroupBy(c => c.classification).ToList();
            if (originalGroupings.Count() <= numOfGroupings)
            {
                for (int i = 0; i < originalGroupings.Count; i++)
                {
                    double probability = originalGroupings[i].Count() / cases.Count;
                    this.classProbabilities.Add(new ClassGrouping(originalGroupings[i].ToList(), this.attrProbabilities, probability));
                }
            }
            else
            {
                List<Case> temp = cases.OrderBy(c => c.classification).ToList();
                int takeCount = temp.Count / numOfGroupings;

                for (int i = 0; i < numOfGroupings; i++)
                {
                    List<Case> temp2;
                    if (i != numOfGroupings - 1) temp2 = temp.Skip(takeCount * i).Take(takeCount).ToList();
                    else temp2 = temp.Skip(takeCount * i).ToList();
                    this.classProbabilities.Add(new ClassGrouping(temp2, attrProbabilities, temp2.Count / cases.Count));
                }
            }
        }

        public override double classify(List<double> attributes)
        {
            // public Dictionary<string, double> Classify(System.IO.StreamReader tr)
            //Dictionary<string, double> score = new Dictionary<string, double>();
            double score = 0;

            foreach (KeyValuePair<string, ICategory> cat in m_Categories)
            {
                score.Add(cat.Value.Name, 0.0);
            }

            foreach (KeyValuePair<string, PhraseCount> kvp1 in words_in_file)
            {
                PhraseCount pc_in_file = kvp1.Value;
                foreach (KeyValuePair<string, ICategory> kvp in m_Categories)
                {
                    ICategory cat = kvp.Value;
                    int count = cat.GetPhraseCount(pc_in_file.RawPhrase);
                    if (0 < count)
                    {
                        score[cat.Name] += System.Math.Log((double)count / (double)cat.TotalWords);
                    }
                    else
                    {
                        score[cat.Name] += System.Math.Log(0.01 / (double)cat.TotalWords);
                    }
                }
            }
  
            foreach (KeyValuePair<string, ICategory> kvp in m_Categories)
            {
                ICategory cat = kvp.Value;
                score[cat.Name] += System.Math.Log((double)cat.TotalWords / (double)this.CountTotalWordsInCategories());
            }

            return score;
        }

        public string Classify(string sample, out double Probability, out List<double> ProbabDistribution)
        {
            //Splits sample words
            string[] words = sample.ToLower().Split(cc, StringSplitOptions.RemoveEmptyEntries);
            words = GetFeatures(words);

            ProbabDistribution = new List<double>();
            double totalProbab = 0;

            for (int i = 0; i < AllCategories.Count; i++)
            {
                double probab = Math.Log(GetCategoryProbability(i));
                for (int j = 0; j < words.Length; j++)
                {
                    probab += Math.Log(GetWordProbability(words[j], i));
                }
                totalProbab += Math.Exp(probab);
                ProbabDistribution.Add(probab);
            }

            int indMax = 0;
            double max = ProbabDistribution[0];
            if (totalProbab != 0) totalProbab = 1.0 / totalProbab;

            for (int i = 0; i < AllCategories.Count; i++)
            {
                if (ProbabDistribution[i] > max)
                {
                    max = ProbabDistribution[i];
                    indMax = i;
                }

                if (totalProbab != 0) ProbabDistribution[i] = Math.Exp(ProbabDistribution[i]) * totalProbab;
            }

            Probability = ProbabDistribution[indMax];
            if (totalProbab == 0)
            {
                //1/pb = exp(lnp1)/exp(lnPmax) + exp(lnp2)/exp(lnPmax) +...
                double pb = 0;
                for (int i = 0; i < ProbabDistribution.Count; i++)
                {
                    if (i != indMax) pb += Math.Exp(ProbabDistribution[i] - ProbabDistribution[indMax]);
                }
                pb += 1;
                pb = 1 / pb;
                Probability = pb;
            }

            if (Probability > 1.0) Probability = 1.0;

            return AllCategories[indMax];
        }

        public override string ToString()
        {
            return "";
        }

        internal class ClassGrouping
        {
            public double min { get; private set; }
            public double max { get; private set; }
            public double probability { get; private set; }
            public List<AttrGroupings> attrProbabilities { get; private set; }

            public ClassGrouping(List<Case> cases, List<AttrGroupings> attrGroupings, double probability)
            {
                this.min = cases.First().classification;
                this.max = cases.Last().classification;
                this.probability = probability;
                this.attrProbabilities = attrGroupings.Select(g => new AttrGroupings(cases, g)).ToList();
            }
        }

        internal class AttrGroupings
        {
            public int attributeIndex { get; private set; }
            public List<AttrGrouping> groupings { get; private set; }

            public AttrGroupings(List<Case> cases, int attributeIndex)
            {
                this.attributeIndex = attributeIndex;
                this.groupings = new List<AttrGrouping>();

                var originalGroupings = cases.GroupBy(c => c.attributes[attributeIndex]).ToList();
                if (originalGroupings.Count() <= numOfGroupings)
                {
                    for(int i = 0; i < originalGroupings.Count; i++)
                    {
                        double probability = originalGroupings[i].Count() / cases.Count;
                        this.groupings.Add(new AttrGrouping(originalGroupings[i].ToList(), i, probability));
                    }
                }
                else
                {
                    List<Case> temp = cases.OrderBy(c => c.attributes[attributeIndex]).ToList();
                    int takeCount = temp.Count / numOfGroupings;

                    for (int i = 0; i < numOfGroupings; i++)
                    {
                        List<Case> temp2;
                        if (i != numOfGroupings - 1) temp2 = temp.Skip(takeCount * i).Take(takeCount).ToList();
                        else temp2 = temp.Skip(takeCount * i).ToList();
                        double probability = temp2.Count / cases.Count;
                        this.groupings.Add(new AttrGrouping(temp2, attributeIndex, probability));
                    }
                }
            }

            public AttrGroupings(List<Case> cases, AttrGroupings original)
            {
                this.attributeIndex = original.attributeIndex;
                this.groupings = new List<AttrGrouping>();

                foreach(AttrGrouping g in original.groupings)
                {
                    double probability = cases.Where(c => 
                        c.attributes[original.attributeIndex] >= original.groupings[attributeIndex].min
                        && c.attributes[original.attributeIndex] <= original.groupings[attributeIndex].max)
                        .Count() / cases.Count;

                    this.groupings.Add(new AttrGrouping(g, probability));
                }
            }
        }

        internal class AttrGrouping
        {
            public double min { get; private set; }
            public double max { get; private set; }
            public double probability { get; private set; }

            public AttrGrouping(List<Case> cases, int attributeIndex, double probability)
            {
                this.min = cases.First().attributes[attributeIndex];
                this.max = cases.Last().attributes[attributeIndex];
                this.probability = probability;
            }

            public AttrGrouping(AttrGrouping original, double probability)
            {
                this.min = original.min;
                this.max = original.max;
                this.probability = probability;
            }
        }
    }
}
