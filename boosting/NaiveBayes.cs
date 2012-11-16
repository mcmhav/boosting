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


  //           max_prob = 0.0
  //best = nil
  
  //scores = cat_scores(text)
  //scores.each do |score|
  //  cat, prob = score
  //  if prob > max_prob
  //    max_prob = prob
  //    best = cat
  //  end
  //end

  //# Return the default category in case the threshold condition was
  //# not met. For example, if the threshold for :spam is 1.2
  //#
  //#    :spam => 0.73, :ham => 0.40  (OK)
  //#    :spam => 0.80, :ham => 0.70  (Fail, :ham is too close)

  //return default unless best
  //threshold = @thresholds[best] || 1.0

  //scores.each do |score|
  //  cat, prob = score
  //  next if cat == best
  //  return default if prob * threshold > max_prob
  //end

            return score;
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
