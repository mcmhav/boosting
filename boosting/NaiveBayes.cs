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
                attrProbabilities.Add(new AttrGroupings(cases, i));

            var originalGroupings = cases.GroupBy(c => c.classification).ToList();
            for (int i = 0; i < originalGroupings.Count; i++)
            {
                double probability = ((double) originalGroupings[i].Count() / cases.Count);
                this.classProbabilities.Add(new ClassGrouping(originalGroupings[i].ToList(), this.attrProbabilities, probability));
            }
        }

        public override double classify(List<double> attributes)
        {
            double score = 0;
            double classification = 0;

            foreach (ClassGrouping c in classProbabilities)
	        {
                double tempScore = 1;
                for (int i = 0; i < attributes.Count; i++)
                {
                    tempScore *= c.probability * c.attrProbabilities[i].groupings.Where(g => g.min < attributes[i] && g.max >= attributes[i]).First().probability;
                }
                if (tempScore > score)
                {
                    score = tempScore;
                    classification = c.classification;
                }
	        }

            return classification;
        }

        public override string ToString()
        {
            return "";
        }

        internal class ClassGrouping
        {
            public double classification { get; private set; }
            public double probability { get; private set; }
            public List<AttrGroupings> attrProbabilities { get; private set; }

            public ClassGrouping(List<Case> cases, List<AttrGroupings> attrGroupings, double probability)
            {
                this.classification = cases.First().classification;
                this.probability = probability;
                this.attrProbabilities = attrGroupings.Select(g => new AttrGroupings(cases, g)).ToList();
            }
        }

        internal class AttrGroupings
        {
            public int attributeIndex { get; private set; }
            public List<ValueGrouping> groupings { get; private set; }

            public AttrGroupings(List<Case> cases, int attributeIndex)
            {
                this.attributeIndex = attributeIndex;
                this.groupings = new List<ValueGrouping>();

                var originalGroupings = cases.GroupBy(c => c.attributes[attributeIndex]).ToList();
                if (originalGroupings.Count() <= numOfGroupings)
                {
                    for(int i = 0; i < originalGroupings.Count; i++)
                    {
                        double probability = ((double) originalGroupings[i].Count() / cases.Count);
                        this.groupings.Add(new ValueGrouping(originalGroupings[i].ToList(), i, probability));
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
                        double probability = ((double) temp2.Count / cases.Count);
                        this.groupings.Add(new ValueGrouping(temp2, attributeIndex, probability));
                    }
                }

                fillCracks();
            }

            public AttrGroupings(List<Case> cases, AttrGroupings original)
            {
                this.attributeIndex = original.attributeIndex;
                this.groupings = new List<ValueGrouping>();

                foreach(ValueGrouping g in original.groupings)
                {
                    double probability = ((double) 
                        cases.Where(c => 
                        c.attributes[attributeIndex] >= g.min
                        && c.attributes[attributeIndex] <= g.max)
                        .Count() / cases.Count);

                    this.groupings.Add(new ValueGrouping(g, probability));
                }

                fillCracks();
            }

            private void fillCracks()
            {
                groupings.First().min = -999999;
                groupings.Last().max = 999999;

                for (int i = 1; i < groupings.Count; i++)
                {
                    double crackSize = groupings[i].min - groupings[i - 1].max;
                    groupings[i - 1].max += crackSize;
                    groupings[i].min = groupings[i - 1].max;
                }
            }
        }

        internal class ValueGrouping
        {
            public double min;
            public double max;
            public double probability { get; private set; }

            public ValueGrouping(List<Case> cases, int attributeIndex, double probability)
            {
                this.min = cases.First().attributes[attributeIndex];
                this.max = cases.Last().attributes[attributeIndex];
                this.probability = probability;
            }

            public ValueGrouping(ValueGrouping original, double probability)
            {
                this.min = original.min;
                this.max = original.max;
                this.probability = probability;
            }
        }
    }
}
