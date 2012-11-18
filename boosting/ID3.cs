using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class ID3 : Hypotheses
    {
        public static int maxDepth = -1;
        public static int numOfGroupings = 4;
        public static int minCasesPerRegNode = 50;
        public static double minInfoGain = 0;

        public static Hypotheses generateHypothesis(List<Case> cases)
        {
            ID3 h = new ID3(cases);
            List<int> attributeIndexes = new List<int>();
            for (int i = 0; i < cases.First().attributes.Count; i++) attributeIndexes.Add(i);
            h.rootNode = h.createNode(attributeIndexes, cases, 0);

            return h;
        }

        private Node rootNode;
        private int numOfClassifications;

        public ID3(List<Case> cases)
        {
            this.numOfClassifications = cases.GroupBy(c => c.classification).Count();
        }

        public override double classify(List<double> attributes)
        {
            return rootNode.classify(attributes);
        }
        
        private Node createNode(List<int> attributeIndexes, List<Case> cases, int depth)
        {
            double entropy = DataStatistics.entropy(cases, numOfClassifications);
            //Console.WriteLine("entropy: " + entropy);
            if (entropy == 0) 
                return new LeafNode(cases[0].classification);
            else if (attributeIndexes.Count == 0 || (maxDepth != -1 && depth == maxDepth) || cases.Count < minCasesPerRegNode)
            {
                return new LeafNode(cases.GroupBy(c => c.classification).OrderByDescending(g => g.Count()).First().Key);
            }
            double bestInfoGain = 0;
            int bestIndex = 0;
            RegNode bestNode = null;
            if (attributeIndexes.Count > 0)
            {
                foreach (int attributeIndex in attributeIndexes)
                {
                    RegNode node = new RegNode(attributeIndex, cases, numOfGroupings, numOfClassifications);
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
                    //Console.WriteLine(remainingCases.Count);
                    if (remainingCases.Count == 0) branchesToRemove.Insert(0, i);
                    else bestNode.branches[i].setChildNode(createNode(remainingAttributeIndexes, remainingCases, depth + 1));
                }
                foreach (int i in branchesToRemove) bestNode.branches.RemoveAt(i);
                bestNode.coverDomain();
                return bestNode;
            }
            else
            {
                return new LeafNode(cases.GroupBy(c => c.classification).OrderByDescending(g => g.Count()).First().Key);
            }
        }

        public override void print()
        {
            Console.WriteLine(rootNode.ToString());
        }

        internal interface Node
        {
            double classify(List<double> attributes);
            string ToString();
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

            public override string ToString()
            {
                return classification.ToString();
            }
        }

        internal class RegNode : Node
        {
            public int attributeIndex { get; private set; }
            public string attributeName { get; private set; }
            public List<Branch> branches { get; private set; }
            public double entropy { get; private set; }

            public RegNode(int attributeIndex, List<Case> cases, int numOfGroupings, int numOfClassifications)
            {
                this.attributeIndex = attributeIndex;
                branches = new List<Branch>();
                entropy = 0;
                var originalGroupings = cases.GroupBy(c => c.attributes[attributeIndex]).OrderBy(g => g.Key);
                if (originalGroupings.Count() <= numOfGroupings)
                {
                    foreach (var group in originalGroupings)
                    {
                        Branch branch = new Branch(group.Key, group.Key);
                        branches.Add(branch);
                        entropy += (double)group.Count() / cases.Count * DataStatistics.entropy(group.ToList(), numOfClassifications);
                    }
                }
                else
                {
                    var casesOrderedBytAttribute = cases.OrderBy(c => c.attributes[attributeIndex]).ToList();
                    int groupSize = Math.Max(casesOrderedBytAttribute.Count / numOfGroupings, 1);
                    for (int i = 0; i < numOfGroupings && i < casesOrderedBytAttribute.Count; i++)
                    {
                        List<Case> branchCases = casesOrderedBytAttribute.Skip(i*groupSize).Take(groupSize).ToList();
                        Branch branch = new Branch(branchCases.First().attributes[attributeIndex], branchCases.Last().attributes[attributeIndex]);
                        branches.Add(branch);
                        entropy += ((double)branchCases.Count / cases.Count) * DataStatistics.entropy(branchCases, numOfClassifications);
                        //Console.WriteLine("branchentropy: " + DataStatistics.entropy(branchCases) + " - " + branchCases.Count);
                    }
                }
                //Console.WriteLine("entropy: " + entropy + " - caseCount: " + cases.Count);
                coverDomain();
            }

            public double classify(List<double> attributes)
            {
                Branch branch = branches.First(b => b.min <= attributes[attributeIndex] && b.max >= attributes[attributeIndex]);
                return branch.child.classify(attributes);
            }

            public void coverDomain()
            {
                branches.First().min = Math.Min(branches.First().min, -100000);
                branches.Last().max = Math.Max(branches.Last().max, +100000);

                for (int i = 1; i < branches.Count; i++)
                {
                    double crackSize = branches[i].min - branches[i - 1].max;
                    branches[i - 1].max += crackSize / 2;
                    branches[i].min = branches[i - 1].max;
                }
            }

            public override string ToString()
            {
                string s = attributeName + "(";
                foreach (Branch b in branches)
                {
                    s += "<" + b.max + "(" + b.child.ToString() + ")";
                }
                return s;
            }
        }

        internal class Branch
        {
            public double min;
            public double max;
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

            public void setChildNode(Node childNode)
            {
                child = childNode;
            }
        }

    }
}