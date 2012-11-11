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
            NaiveBayes h = new NaiveBayes(cases);

            return h;
        }

        public NaiveBayes(List<Case> cases)
        {
            this.classProbabilities = new List<ClassGrouping>();
            this.attrProbabilities = new List<AttrGroupings>();

            for (int i = 0; i < cases.First().attributes.Count; i++)
                attrProbabilities[i] = new AttrGroupings(cases, i);

            var originalGroupings = cases.GroupBy(c => c.classification);

            if (originalGroupings.Count() <= numOfGroupings)
            {
                //this.classProbabilities = originalGroupings.Select(g => new ClassGrouping(g.All(), this.attrProbabilities));
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
                    this.classProbabilities.Add(new ClassGrouping(temp2, attrProbabilities));
                }
            }
        }

        public override double classify(List<double> attributes)
        {
            return 0;
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

            public ClassGrouping(List<Case> cases, List<AttrGroupings> attrGroupings)
            {
                this.min = cases.First().classification;
                this.max = cases.Last().classification;

                this.attrProbabilities = attrGroupings.Select(g => new AttrGroupings(g)).ToList();
            }

            public void setProbability(double probability)
            {
                this.probability = probability;
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

                List<Case> temp = cases.OrderBy(c => c.attributes[attributeIndex]).ToList();
                int takeCount = temp.Count / numOfGroupings;

                for (int i = 0; i < numOfGroupings; i++)
                {
                    List<Case> temp2;
                    if (i != numOfGroupings - 1) temp2 = temp.Skip(takeCount * i).Take(takeCount).ToList();
                    else temp2 = temp.Skip(takeCount * i).ToList();
                    this.groupings.Add(new AttrGrouping(temp2, attributeIndex));
                }
            }

            public AttrGroupings(AttrGroupings original)
            {
                this.attributeIndex = original.attributeIndex;
                this.groupings = new List<AttrGrouping>();
                this.groupings = original.groupings.Select(g => new AttrGrouping(g)).ToList();
            }
        }

        internal class AttrGrouping
        {
            public double min { get; private set; }
            public double max { get; private set; }
            public double probability { get; private set; }

            public AttrGrouping(List<Case> cases, int attributeIndex)
            {
                this.min = cases.First().attributes[attributeIndex];
                this.max = cases.Last().attributes[attributeIndex];
            }

            public AttrGrouping(AttrGrouping original)
            {
                this.min = original.min;
                this.max = original.max;
            }

            public void setProbability(double probability)
            {
                this.probability = probability;
            }
        }
    }
}
