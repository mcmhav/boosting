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
        private static bool testID3 = false;
        private static bool testNB = false;
        private static bool testBoth = false;
        private static int M = 5;
        private static bool log = false;
        private static readonly int testCount = 10;


        private static Tuple<double, double> id3 = new Tuple<double,double>(0,0);
        private static Tuple<double, double> naive = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> id3M = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> naiveM = new Tuple<double, double>(0, 0);
        private static Tuple<double, double> both = new Tuple<double, double>(0, 0);

        static void Main(string[] args)
        {
            //bool cont = false;
            //while (!cont)
            //{
            //    String input = Console.ReadLine();
            //    // [file,   id3/NB/both,    M,      log]
            //    // [0-4,    0-2,            0-..,   0-1]

            //    Console.WriteLine("input: " + input);

            //    cont = initRun(input, true);
            //}
            
            //run();

            makeDatasetTables();
            Console.WriteLine(data);

            System.IO.StreamWriter file2 = new System.IO.StreamWriter(@"..\..\..\datasets\data.txt");
            file2.WriteLine(data);

            Console.WriteLine("DONE");
            Console.ReadLine();
        }

        static private bool initRun(string input, bool reader)
        {
            String[] vals = input.Split(',');

            if (inputOK(vals))
            {
                fileName = filenames[int.Parse(vals[0])];
                switch (int.Parse(vals[1]))
                {
                    case 0:
                        if (reader)
                        {
                            Console.WriteLine("Tree depth: ");
                            string temp = Console.ReadLine();
                            int num;
                            if (int.TryParse(temp, out num))
                            {
                                ID3.maxDepth = num;
                            }
                            else return false;
                        }
                        testID3 = true;
                        testNB = false;
                        testBoth = false;
                        break;
                    case 1:
                        testID3 = false;
                        testNB = true;
                        testBoth = false;
                        break;
                    case 2:
                        testID3 = false;
                        testNB = false;
                        testBoth = true;
                        break;
                    default:
                        break;
                }


                M = int.Parse(vals[2]);

                switch (int.Parse(vals[3]))
                {
                    case 0:
                        log = false;
                        break;
                    case 1:
                        log = true;
                        break;
                    default:
                        break;
                }

                return true;
            }
            return false;
        }

        static private List<Tuple<double, double>> data = new List<Tuple<double, double>>();


        static private void makeDatasetTables()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    initRun(i + "," + j + ",1,0", false);
                    int a = -1;
                    if (j == 0)
                    {
                        Tuple<List<Case>, List<Case>, double> temp = getCaseSetsFromFile();
                        int t1 = temp.Item1.GroupBy(c => c.classification).Count();
                        int t2 = temp.Item2.GroupBy(c => c.classification).Count();
                        a = (int)Math.Max(t1, t2);
                    }
                    ID3.maxDepth = a;
                    run();

                    initRun(i + "," + j + ",5,0", false);
                    ID3.maxDepth = -1;
                    run();


                    initRun(i + "," + j + ",10,0", false);
                    ID3.maxDepth = 1;
                    run();

                    if (j == 0)
                    {
                        initRun(i + "," + j + ",10,0", false);
                        ID3.maxDepth = 2;
                        run();

                        initRun(i + "," + j + ",10,0", false);
                        ID3.maxDepth = a;
                        run();
                    }

                    initRun(i + "," + j + ",20,0", false);
                    ID3.maxDepth = a;
                    run();
                }
            }
        }

        //A single DTC (max depth = A)
        //5 DTCs (max possible depth)
        //10 DTCs (max depth = 1)
        //10 DTCs (max depth = 2)
        //10 DTCs (max depth = A)
        //20 DTCs (max depth = A)

        //A single NBC.
        //5 NBCs
        //10 NBCs
        //20 NBCs

        //5 NBCs and 5 DTCs (max depth = 2)
        //10 NBCs and 10 DTCs (max depth = 2)
        //20 NBCs and 20 DTCs (max depth = 2)


        static private bool inputOK(string[] vals)
        {
            bool temp = true;
            int num;
            if (vals.Length != 4)
                return false;
            for (int i = 0; i < 4; i++)
            {
                if (!int.TryParse(vals[i], out num))
                    return false;
                switch (i)
                {
                    case 0:
                        if (num < 0 || num > 4) 
                            return false;
                        break;
                    case 1:
                        if (num < 0 || num > 2)
                            return false;
                        break;
                    case 2:
                        if (false)
                            return false;
                        break;
                    case 3:
                        if (num < 0 || num > 1)
                            return false;
                        break;
                    default:
                        break;
                }
            }
            return temp;
        }

        static void run()
        {
            for (int i = 0; i < testCount; i++)
            {
                trainAndTest();
                Console.WriteLine();
                Console.WriteLine();
            }
            if (testID3)
            {
                id3 = new Tuple<double, double>(id3.Item1 / testCount, id3.Item2 / testCount);
                id3M = new Tuple<double, double>(id3M.Item1 / testCount, id3M.Item2 / testCount);
                Console.WriteLine("Average ID3: " + id3);
                Console.WriteLine("Average " + M + " ID3: " + id3M);

                data.Add(id3);
                data.Add(id3M);
            }
            if (testNB)
            {
                naive = new Tuple<double, double>(naive.Item1 / testCount, naive.Item2 / testCount);
                naiveM = new Tuple<double, double>(naiveM.Item1 / testCount, naiveM.Item2 / testCount);
                Console.WriteLine("Average NB: " + naive);
                Console.WriteLine("Average " + M + " NB: " + naiveM);

                data.Add(naive);
                data.Add(naiveM);
            }
            if (testBoth)
            {
                both = new Tuple<double, double>(both.Item1 / testCount, both.Item2 / testCount);
                Console.WriteLine("Average " + M + "Both: " + both);

                data.Add(both);
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

            double binaryRatio = 0.5 / (1 - (double)(1 / cases.GroupBy(c => c.classification).Count()));

            return new Tuple<List<Case>, List<Case>, double>(trainingSet, testSet, binaryRatio);
        }

        static void trainAndTest()
        {
            if (log)
            {
                Console.WriteLine(fileName);
            }
            Tuple<List<Case>, List<Case>, double> caseSets = getCaseSetsFromFile();

            //List<Hypotheses> lonleyL = new List<Hypotheses>()
            //    {
            //        KNearest.generateHypothesis(caseSets.Item1)
            //    };
            //Tuple<double, double> resh = ADABoost.test(lonleyL, caseSets.Item2, log);
            //Console.WriteLine("KNearest: " + resh);

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
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, M, caseSets.Item3, log));
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, M, caseSets.Item3, log));
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
            if (log)
            {
                Console.WriteLine(name + ": " + res);
            }

            List<Hypotheses> H = ADABoost.weightedMajorityHypotheses(caseSets.Item1, L, M, caseSets.Item3, log);
            double totalWeightNB = H.Sum(h => h.weight);
            foreach (Hypotheses h in H)
            {
                h.weight /= totalWeightNB;
                if (log) Console.WriteLine("weight: " + h.weight);
            }

            Tuple<double, double> resM = ADABoost.test(H, caseSets.Item2, log);
            if (name == "ID3") id3M = new Tuple<double, double>(id3M.Item1 + resM.Item1, id3M.Item2 + resM.Item2);
            else naiveM = new Tuple<double, double>(naiveM.Item1 + resM.Item1, naiveM.Item2 + resM.Item2);
            if (log)
            {
                Console.WriteLine(M + " " + name + "': " + resM);
            }
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