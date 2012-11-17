using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;

namespace boosting
{
    class Program
    {
        private static string fileName = "nursery.txt";
        private static bool testID3 = false;
        private static bool testNB = true;
        private static bool testBoth = false;

        private static int M = 10;
        private static bool log = false;

        static void Main(string[] args)
        {
            trainAndTest();
            
            Console.ReadLine();
        }

        static Tuple<List<Case>, List<Case>> getCaseSetsFromFile()
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

            cases.QuickShuffle();

            List<Case> trainingSet = new List<Case>();
            trainingSet.AddRange(cases.Take((int)(cases.Count * 0.8)));

            List<Case> testSet = new List<Case>();
            testSet.AddRange(cases.Skip((int)(cases.Count * 0.8)));

            return new Tuple<List<Case>, List<Case>>(trainingSet, testSet);
        }

        static void trainAndTest()
        {
            Console.WriteLine(fileName);
            Tuple<List<Case>, List<Case>> caseSets = getCaseSetsFromFile();

            if (testID3)
            {
                trainNtest(ID3.generateHypothesis, caseSets, "ID3");
            }

            if (testNB)
            {
                trainNtest(NaiveBayes.generateHypothesis, caseSets, "Naive Bayes");
            }

            if (testBoth)
            {
                List<Hypotheses> H = new List<Hypotheses>();
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, M/2, log));
                foreach (Case c in caseSets.Item1) c.weight = 1;
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, M/2, log));
                double totalWeight = H.Sum(h => h.weight);
                foreach (Hypotheses h in H) h.weight /= totalWeight;
                Console.WriteLine("Combined: " + ADABoost.test(H, caseSets.Item2, log));
            }
        }

        static void trainNtest(Func<List<Case>, Hypotheses> L, Tuple<List<Case>, List<Case>> caseSets, string name)
        {
            List<Hypotheses> lonleyL = new List<Hypotheses>()
                {
                    L(caseSets.Item1)
                };
            Console.WriteLine(name + ": " + ADABoost.test(lonleyL, caseSets.Item2, log));

            foreach (Case c in caseSets.Item1) c.weight = 1;
            List<Hypotheses> nb = ADABoost.weightedMajorityHypotheses(caseSets.Item1, L, M, log);
            double totalWeightNB = nb.Sum(h => h.weight);
            foreach (Hypotheses h in nb) h.weight /= totalWeightNB;
            Console.WriteLine("M " + name + "': " + ADABoost.test(nb, caseSets.Item2, log)); 
        }
    }

    static class Ext
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                var k = (box[0] % n);
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void QuickShuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}