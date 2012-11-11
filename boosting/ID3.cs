using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class ID3 : Hypotheses
    {
        private static readonly int numOfGroupings = 4;
        private static readonly int minCasesPerRegNode = 55;
        private static readonly double minInfoGain = 0.0;
        private static readonly bool averageVote = false;
        
        public static Hypotheses generateHypothesis(List<Case> cases)
        {
            ID3 h = new ID3();
            List<int> remainingAttributeIndexes2 = new List<int>();
            for (int i = 0; i < cases.First().attributes.Count; i++) remainingAttributeIndexes2.Add(i);
            h.rootNode = h.createNode(remainingAttributeIndexes2, cases);

            return h;
        }

        private Node rootNode;

        public override double classify(List<double> attributes)
        {
            return rootNode.classify(attributes);
        }
        
        private Node createNode(List<int> attributeIndexes, List<Case> cases)
        {
            double entropy = DataStatistics.entropy(cases);
            if (entropy == 0) return new LeafNode(cases[0].classification);
            else if (attributeIndexes.Count == 0 || cases.Count < minCasesPerRegNode)
            {
                if (averageVote) return new LeafNode(cases.Average(c => c.classification));
                else return new LeafNode(cases.GroupBy(c => c.classification).OrderByDescending(g => g.Count()).First().Key);
            }
            double bestInfoGain = 0;
            int bestIndex = 0;
            RegNode bestNode = null;
            if (attributeIndexes.Count > 0)
            {
                foreach (int attributeIndex in attributeIndexes)
                {
                    RegNode node = new RegNode(attributeIndex, cases, numOfGroupings);
                    double nodeInfoGain = entropy - node.entropy;
                    if (nodeInfoGain > bestInfoGain)
                    {
                        bestInfoGain = nodeInfoGain;
                        bestIndex = attributeIndex;
                        bestNode = node;
                    }
                }
            }

            if (bestInfoGain > minInfoGain)
            {
                List<int> branchesToRemove = new List<int>();
                for (int i = 0; i < bestNode.branches.Count; i++)
                {
                    List<int> remainingAttributeIndexes = attributeIndexes.Where(index => index != bestIndex).ToList();
                    List<Case> remainingCases = cases.Where(c => c.attributes[bestIndex] >= bestNode.branches[i].min && c.attributes[bestIndex] < bestNode.branches[i].max).ToList();
                    if (remainingCases.Count == 0) branchesToRemove.Insert(0, i);
                    else bestNode.branches[i].setChildNode(createNode(remainingAttributeIndexes, remainingCases));
                }
                foreach (int i in branchesToRemove) bestNode.branches.RemoveAt(i);
                bestNode.coverDomain();
                return bestNode;
            }
            else
            {
                if (averageVote) return new LeafNode(cases.Average(c => c.classification));
                else return new LeafNode(cases.GroupBy(c => c.classification).OrderByDescending(g => g.Count()).First().Key);
            }
        }

        internal interface Node
        {
            double classify(List<double> attributes);
        }

        internal class LeafNode : Node
        {
            private double classification;

            public LeafNode(double classification)
            {
                this.classification = classification;
            }

            public double classify(List<double> attributes)
            {
                return classification;
            }

            public double entropy(List<Case> cases)
            {
                return 0;
            }
        }

        internal class RegNode : Node
        {
            public int attributeIndex { get; private set; }
            public string attributeName { get; private set; }
            public List<Branch> branches { get; private set; }
            public double entropy { get; private set; }

            public RegNode(int attributeIndex, List<Case> cases, int numOfGroupings)
            {
                this.attributeIndex = attributeIndex;
                branches = new List<Branch>();
                entropy = 0;
                var casesOrderedBytAttribute = cases.OrderBy(c => c.attributes[attributeIndex]).ToList();
                int groupSize = Math.Max(casesOrderedBytAttribute.Count / numOfGroupings, 1);
                double lastMax = -10000;
                for (int i = 0; i < numOfGroupings && i < casesOrderedBytAttribute.Count; i++)
                {
                    Branch branch = new Branch(
                        lastMax,
                        lastMax = casesOrderedBytAttribute[i * groupSize + groupSize - 1].attributes[attributeIndex]);
                    branches.Add(branch);

                    List<Case> branchCases = casesOrderedBytAttribute
                        .Where(c => c.attributes[attributeIndex] > branch.min && c.attributes[attributeIndex] < branch.max)
                        .ToList();
                    entropy += (double)branchCases.Count / cases.Count * DataStatistics.entropy(branchCases);
                }
                coverDomain();
            }

            public double classify(List<double> attributes)
            {
                Branch branch = branches.First(b => b.min <= attributes[attributeIndex] && b.max >= attributes[attributeIndex]);
                return branch.child.classify(attributes);
            }

            public void coverDomain()
            {
                branches.First().setMinValue(Math.Min(branches.First().min, -100000));
                branches.Last().setMaxValue(Math.Max(branches.Last().max, +100000));
            }
        }

        internal class Branch
        {
            public double min { get; private set; }
            public double max { get; private set; }
            public Node child { get; private set; }

            public Branch(double minValue, double maxValue)
            {
                min = minValue;
                max = maxValue;
            }

            public void setMinValue(double minValue)
            {
                min = minValue;
            }

            public void setMaxValue(double maxValue)
            {
                max = maxValue;
            }

            public void setChildNode(Node childDode)
            {
                child = childDode;
            }
        }

    }
}