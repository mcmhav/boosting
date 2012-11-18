using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;

namespace boosting
{
    class Program
    {
        //private static string fileName = "nursery.txt";     // 0 - 4
        private static string fileName = "pen-digits.txt";  // 0 - 9
        private static bool testID3 = false;
        private static bool testNB = false;
        private static bool testBoth = false;
        private static int M = 5;
        private static bool log = false;
        private static readonly int testCount = 1;

        private static Tuple<double, double> id3 = new Tuple<double,double>(0,0);
        private static Tuple<double, double> naive = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> id3M = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> naiveM = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> both = new Tuple<double, double>(0, 0);

        static void Main(string[] args)
        {
            for (int i = 0; i < testCount; i++)
            {
                trainAndTest();
                Console.WriteLine();
                Console.WriteLine();
            }
            if (testID3)
            {
                ID3.maxDepth = 5;
                id3 = new Tuple<double, double>(id3.Item1 / testCount, id3.Item2 / testCount);
                id3M = new Tuple<double, double>(id3M.Item1 / testCount, id3M.Item2 / testCount);
                Console.WriteLine("Average ID3: " + id3);
                Console.WriteLine("Average " + M + " ID3: " + id3M);
            }
            if (testNB)
            {
                naive = new Tuple<double, double>(naive.Item1 / testCount, naive.Item2 / testCount);
                naiveM = new Tuple<double, double>(naiveM.Item1 / testCount, naiveM.Item2 / testCount);
                Console.WriteLine("Average NB: " + naive);
                Console.WriteLine("Average " + M + " NB: " + naiveM);
            }
            if (testBoth)
            {
                both = new Tuple<double, double>(both.Item1 / testCount, both.Item2 / testCount);
                Console.WriteLine("Average " + M + "Both: " + both);
            }

            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        static Tuple<List<Case>, List<Case>, double> getCaseSetsFromFile()
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

            double binaryRatio = 0.5 / (1 - (double)(1 / cases.GroupBy(c => c.classification).Count()));

            return new Tuple<List<Case>, List<Case>, double>(trainingSet, testSet, binaryRatio);
        }

        static void trainAndTest()
        {
            Console.WriteLine(fileName);
            Tuple<List<Case>, List<Case>, double> caseSets = getCaseSetsFromFile();

            List<Hypotheses> lonleyL = new List<Hypotheses>()
                {
                    KNearest.generateHypothesis(caseSets.Item1)
                };
            Tuple<double, double> resh = ADABoost.test(lonleyL, caseSets.Item2, log);
            Console.WriteLine("KNearest: " + resh);

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
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, M / 2, caseSets.Item3, log));
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, M / 2, caseSets.Item3, log));
                double totalWeight = H.Sum(h => h.weight);
                foreach (Hypotheses h in H) h.weight /= totalWeight;
                Tuple<double, double> res = ADABoost.test(H, caseSets.Item2, log);
                both = new Tuple<double, double>(both.Item1 + res.Item1, both.Item2 + res.Item2);
                Console.WriteLine("Combined: " + res);
            }
        }

        static void trainNtest(Func<List<Case>, Hypotheses> L, Tuple<List<Case>, List<Case>, double> caseSets, string name)
        {
            List<Hypotheses> lonleyL = new List<Hypotheses>()
                {
                    L(caseSets.Item1)
                };
            Tuple<double, double> res = ADABoost.test(lonleyL, caseSets.Item2, log);
            if(name == "ID3") id3 = new Tuple<double, double>(id3.Item1 + res.Item1, id3.Item2 + res.Item2);
            else naive = new Tuple<double, double>(naive.Item1 + res.Item1, naive.Item2 + res.Item2);
            Console.WriteLine(name + ": " + res);

            List<Hypotheses> H = ADABoost.weightedMajorityHypotheses(caseSets.Item1, L, M, caseSets.Item3, log);
            double totalWeightNB = H.Sum(h => h.weight);
            foreach (Hypotheses h in H)
            {
                h.weight /= totalWeightNB;
                if (log) Console.WriteLine("weight: " + h.weight);
            }

            Tuple<double, double> resM = ADABoost.test(H, caseSets.Item2, log);
            if (name == "ID3") id3M = new Tuple<double, double>(id3M.Item1 + resM.Item1, id3M.Item2 + resM.Item2);
            else naive = new Tuple<double, double>(naiveM.Item1 + resM.Item1, naiveM.Item2 + resM.Item2);
            Console.WriteLine(M + " " + name + "': " + resM);
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