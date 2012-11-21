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
        //private static string fileName = ;     // 0 - 4
        private static string fileName = "pen-digits.txt";  // 0 - 9
        private static List<string> filenames = new List<string>(){"Yeast.txt",
                                                                    "page-blocks.txt",
                                                                    "glass.txt",
                                                                    "nursery.txt",
                                                                    "pen-digits.txt"};
        private static bool log = false;
        private static readonly int testCount = 10;

        private static Tuple<double, double, double, double> results = new Tuple<double, double, double, double>(0, 0, 0, 0);
        static private List<string> data = new List<string>();

        static void Main(string[] args)
        {
            int id3 = 0;
            int nb = 0;
            int maxDepth = -1;
            
            bool cont = false;
            while (!cont)
            {
                Console.WriteLine("Please type in three numbers divided by ',' and press enter.");
                Console.WriteLine("The first is for which dataset you want to use, and the second and third number tells how many DTC's you want to use, and how many NBC's you want to use in your ADA-Boost.");
                Console.WriteLine();
                Console.WriteLine("Yeast = 0");
                Console.WriteLine("page-blocks = 1");
                Console.WriteLine("glass = 2");
                Console.WriteLine("nursery = 3");
                Console.WriteLine("pen-digits = 4");
                Console.WriteLine();
                Console.WriteLine();

                String input = Console.ReadLine();
                // [file,   id3/NB/both,    M,      log]
                // [0-4,    0-2,            0-..,   0-1]

                Console.WriteLine("input: " + input);

                String[] vals = input.Split(',');
                int num;
                bool inputOK = true;
                List<int> values = new List<int>();
                foreach (string val in vals)
                {
                    if (!int.TryParse(val, out num))
                        inputOK = false;
                    else
                        values.Add(num);
                }
                if (values.Count < 3)
                {
                    inputOK = false;
                }
                if (inputOK)
                {
                    fileName = filenames[values[0]];

                    id3 = values[1];
                    nb = values[2];
                    if (id3 > 0)
                    {
                        Console.WriteLine("Tree depth: ");
                        string temp = Console.ReadLine();
                        int num2;
                        if (int.TryParse(temp, out num2))
                        {
                            maxDepth = num2;
                        }
                    }
                    Console.WriteLine();
                    cont = true;
                }
            }

            run(id3, nb, maxDepth);

            //makeDatasetTables();
            StringBuilder sb = new StringBuilder();
            foreach (var line in data)
            {
                sb.AppendLine(line);
            }

            string filePath = @"..\..\..\datasets\data2.txt";
            //File.WriteAllText(filePath, sb.ToString());

            Console.WriteLine();
            Console.WriteLine("DONE");
            Console.ReadLine();
        }




        static void run(int id3, int nb, int maxDepth = -1)
        {          
            results = new Tuple<double, double, double, double>(0, 0, 0, 0);
            for (int i = 0; i < testCount; i++)
            {
                trainAndTest(id3, nb, maxDepth);
            }
            results = new Tuple<double, double, double, double>(results.Item1 / testCount, results.Item2 / testCount, results.Item3 / testCount, results.Item4 / testCount);
            Console.WriteLine("Average " + id3 + " id3's and " + nb +" nb's from " + testCount + " runs.");
            Console.WriteLine();
            Console.WriteLine("MSE:\t\t" + results.Item1);
            Console.WriteLine("%correct:\t" + results.Item2);
            Console.WriteLine("Avg error:\t" + results.Item3);
            Console.WriteLine("SD:\t\t" + results.Item4);
        }

        static void trainAndTest(int id3, int nb, int maxDepth = -1)
        {
            if (log)
            {
                Console.WriteLine(fileName);
            }
            Tuple<List<Case>, List<Case>, double> caseSets = getCaseSetsFromFile();

            List<Hypotheses> H = new List<Hypotheses>();
            H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, id3, caseSets.Item3, log));
            H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, nb, caseSets.Item3, log));
            double totalWeight = H.Sum(h => h.weight);
            foreach (Hypotheses h in H) h.weight /= totalWeight;
            Tuple<double, double, double, double> res = ADABoost.test(H, caseSets.Item2, log);
            res = new Tuple<double, double, double, double>(Math.Round(res.Item1, 3), Math.Round(res.Item2, 3), Math.Round(res.Item3, 3), Math.Round(res.Item4, 3));
            results = new Tuple<double, double, double, double>(results.Item1 + res.Item1, results.Item2 + res.Item2, results.Item3 + res.Item3, results.Item4 + res.Item4);
            if (log)
            {
                Console.WriteLine("Result: " + res);
            }
        }

        static private void makeDatasetTables()
        {
            foreach (string file in filenames)
            {
                data.Add(file);
                int a = getCaseSetsFromFile().Item1.First().attributes.Count;
                fileName = file;

                data.Add("\tMSE\tCorrect %\tAvg error\tSD");

                string singleNB = "A single NBC";
                run(0, 1);
                singleNB += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                data.Add(singleNB);

                string singleDT = "A single DTC (with max depth = A)";
                run(1, 0, a);
                singleDT += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                data.Add(singleDT);

                for (int i = 5; i <= 20; i += 5)
                {
                    if (i == 15) continue;
                    string nbi = i + " NBCs";
                    run(0, i);
                    nbi += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                    data.Add(nbi);
                }

                for (int i = 5; i <= 20; i += 5)
                {
                    if (i == 15) continue;
                    else if (i == 10)
                    {
                        for (int j = 1; j <= 2; j++)
                        {
                            string dti = i + " DTCs (maximum depth = " + j + ")";
                            run(i, 0, j);
                            dti += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                            data.Add(dti);
                        }
                    }
                    string dti2 = i + " DTCs (maximum depth = " + a + ")";
                    run(i, 0, a);
                    dti2 += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                    data.Add(dti2);
                }

                for (int i = 5; i <= 20; i += 5)
                {
                    if (i == 15) continue;
                    string both = i + "NBCs and " + i + " DTCs (maximum depth = 2)";
                    run(i, i, 2);
                    both += "\t" + results.Item1 + "\t" + results.Item2 + "\t" + results.Item3 + "\t" + results.Item4;
                    data.Add(both);
                }

                data.Add("");
            }
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

            double binaryRatio = 0.5 / (1 - (double)1 / cases.GroupBy(c => c.classification).Count());
            return new Tuple<List<Case>, List<Case>, double>(trainingSet, testSet, binaryRatio);
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