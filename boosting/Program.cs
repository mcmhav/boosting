using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms;
using System.IO;
using System.Globalization;

namespace boosting
{
    class Program
    {
        static readonly String location;
        
        static void Main(string[] args)
        {
            trainAndTest("yeast.txt");
            
            Console.ReadLine();
        }

        static Tuple<List<Case>, List<Case>> getCaseSetsFromFile(String fileName)
        {
            var reader = new StreamReader(File.OpenRead(@"..\..\..\datasets\" + fileName));
            List<string> valueNames = reader.ReadLine().Split(',').ToList();
            List<Case> cases = new List<Case>();
            while (!reader.EndOfStream)
            {
                List<double> attributes = reader.ReadLine().Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();
                double classification = attributes.Last();
                attributes.RemoveAt(attributes.Count - 1);
                Case c = new Case(attributes, classification);
                cases.Add(c);
            }

            List<Case> trainingSet = new List<Case>();
            trainingSet.AddRange(cases.Take((int)(cases.Count * 0.8)));

            List<Case> testSet = new List<Case>();
            testSet.AddRange(cases.Skip((int)(cases.Count * 0.8)));

            return new Tuple<List<Case>, List<Case>>(trainingSet, testSet);
        }

        static void trainAndTest(string fileName)
        {
            Console.WriteLine(fileName);
            Tuple<List<Case>, List<Case>> caseSets = getCaseSetsFromFile(fileName);

            List<Hypotheses> H = new List<Hypotheses>();
            H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, 10));
            H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, 10));

            List<Hypotheses> id3 = ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, 10);
            List<Hypotheses> nb = ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, 1);

            double totalWeight = H.Sum(h => h.weight);
            foreach (Hypotheses h in H) h.weight /= totalWeight;

            double totalWeightID3 = id3.Sum(h => h.weight);
            foreach (Hypotheses h in id3)
            {
                h.weight /= totalWeightID3;
                Console.WriteLine(h.weight);
            }

            double totalWeightNB = nb.Sum(h => h.weight);
            foreach (Hypotheses h in nb) h.weight /= totalWeightNB;

            //foreach (Hypotheses h in id3) Console.WriteLine(h.ToString());

            //Console.WriteLine("ID3: " + ADABoost.test(id3, caseSets.Item2));
            Console.WriteLine("Naive Bayes: " + ADABoost.test(nb, caseSets.Item2));
            //Console.WriteLine("Combined: " + ADABoost.test(H, caseSets.Item2));
            
            //List<Hypotheses> naiveBayes = ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, 10);

        }
    }
}