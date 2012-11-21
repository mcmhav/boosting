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
        private static string fileName = "nursery.txt";  // 0 - 9
        private static List<string> filenames = new List<string>(){"Yeast.txt",
                                                                    "page-blocks.txt",
                                                                    "glass.txt",
                                                                    "nursery.txt",
                                                                    "pen-digits.txt"};
        private static bool testID3 = true;
        private static bool testNB = true;
        private static bool testBoth = true;
        private static int M = 10;
        //private static int Mi3 = 10;
        //private static int Mnb = 10;
        private static bool log = false;
        private static readonly int testCount = 5;

        static private List<string> data = new List<string>();

        //private static Tuple<double, double> id3 = new Tuple<double,double>(0,0);
        //private static Tuple<double, double> naive = new Tuple<double, double>(0, 0);
        private static Tuple<double, double, double, double> id3M = new Tuple<double, double, double, double>(0, 0, 0, 0);
        private static Tuple<double, double, double, double> naiveM = new Tuple<double, double, double, double>(0, 0, 0, 0);
        private static Tuple<double, double, double, double> both = new Tuple<double, double, double, double>(0, 0, 0, 0);

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

            run();

            //makeDatasetTables();
            StringBuilder sb = new StringBuilder();
            foreach (var line in data)
            {
                sb.AppendLine(line);
            }

            //string filePath = @"..\..\..\datasets\data2.txt";
            //File.WriteAllText(filePath, sb.ToString());

            Console.WriteLine("DONE");
            Console.ReadLine();
        }


        

        static void run()
        {          
            //id3 = new Tuple<double, double>(0, 0);
            id3M = new Tuple<double, double, double, double>(0, 0, 0, 0);
            //naive = new Tuple<double, double>(0, 0);
            naiveM = new Tuple<double, double, double, double>(0, 0, 0, 0);
            both = new Tuple<double, double, double, double>(0, 0, 0, 0);
            for (int i = 0; i < testCount; i++)
            {
                //Console.WriteLine();
                //Console.WriteLine();
                trainAndTest();
            }
            if (testID3)
            {
                //id3 = new Tuple<double, double>(id3.Item1 / testCount, id3.Item2 / testCount);
                id3M = new Tuple<double, double, double, double>(id3M.Item1 / testCount, id3M.Item2 / testCount, id3M.Item3 / testCount, id3M.Item4 / testCount);
                //if (id3.Item2 > 1)
                //    Console.WriteLine();
                if (id3M.Item2 > 1)
                    Console.WriteLine();
                //string id3line = "Average ID3: " + id3 + " (max depth = " + ID3.maxDepth + ")";
                string id3Mline = "Average " + M + " ID3: " + id3M + " (max depth = " + ID3.maxDepth + ")";
                //Console.WriteLine(id3line);
                Console.WriteLine(id3Mline);
                //data.Add(id3line);
                data.Add(id3Mline);
            }
            if (testNB)
            {
                //naive = new Tuple<double, double>(naive.Item1 / testCount, naive.Item2 / testCount);
                naiveM = new Tuple<double, double, double, double>(naiveM.Item1 / testCount, naiveM.Item2 / testCount, id3M.Item3 / testCount, id3M.Item4 / testCount);
                //if (naive.Item2 > 1)
                //    Console.WriteLine();
                if (naiveM.Item2 > 1)
                    Console.WriteLine();
                //string naiveLine = "Average NB: " + naive;
                string naiveMLine = "Average " + M + " NB: " + naiveM;
                //Console.WriteLine(naiveLine);
                Console.WriteLine(naiveMLine);

                //data.Add(naiveLine);
                data.Add(naiveMLine);
            }
            if (testBoth)
            {
                both = new Tuple<double, double, double, double>(both.Item1 / testCount, both.Item2 / testCount, id3M.Item3 / testCount, id3M.Item4 / testCount);
                if (both.Item2 > 1)
                    Console.WriteLine();
                string bothLine = "Average " + M + "Both: " + both;
                Console.WriteLine(bothLine);

                data.Add(bothLine);
            }
            Console.WriteLine();
        }

        

        static void trainAndTest()
        {
            if (log)
            {
                Console.WriteLine(fileName);
            }
            Tuple<List<Case>, List<Case>, double> caseSets = getCaseSetsFromFile();

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
                int tempDepth = ID3.maxDepth;
                ID3.maxDepth = 2;


                List<Hypotheses> H = new List<Hypotheses>();
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, M, caseSets.Item3, log));
                H.AddRange(ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, M, caseSets.Item3, log));
                double totalWeight = H.Sum(h => h.weight);
                foreach (Hypotheses h in H) h.weight /= totalWeight;
                Tuple<double, double, double, double> res = ADABoost.test(H, caseSets.Item2, log);
                res = new Tuple<double, double, double, double>(Math.Round(res.Item1, 3), Math.Round(res.Item2, 3), Math.Round(res.Item3, 3), Math.Round(res.Item4, 3));
                both = new Tuple<double, double, double, double>(both.Item1 + res.Item1, both.Item2 + res.Item2, both.Item3 + res.Item3, both.Item4 + res.Item4);
                if (log)
                {
                    Console.WriteLine("Combined: " + res);
                }

                ID3.maxDepth = tempDepth;
            }
        }

        static void trainNtest(Func<List<Case>, Hypotheses> L, Tuple<List<Case>, List<Case>, double> caseSets, string name)
        {
            List<Hypotheses> H = ADABoost.weightedMajorityHypotheses(caseSets.Item1, L, M, caseSets.Item3, log);
            double totalWeightNB = H.Sum(h => h.weight);
            foreach (Hypotheses h in H)
            {
                h.weight /= totalWeightNB;
                //if (log) Console.WriteLine("weight: " + h.weight);
            }

            Tuple<double, double, double, double> resM = ADABoost.test(H, caseSets.Item2, log);
            resM = new Tuple<double, double, double, double>(Math.Round(resM.Item1, 3), Math.Round(resM.Item2, 3), Math.Round(resM.Item3, 3), Math.Round(resM.Item4, 3));
            if (name == "ID3") id3M = new Tuple<double, double, double, double>(id3M.Item1 + resM.Item1, id3M.Item2 + resM.Item2, id3M.Item3 + resM.Item3, id3M.Item4 + resM.Item4);
            else naiveM = new Tuple<double, double, double, double>(naiveM.Item1 + resM.Item1, naiveM.Item2 + resM.Item2, naiveM.Item3 + resM.Item3, naiveM.Item4 + resM.Item4);
            if (log)
            {
                Console.WriteLine(M + " " + name + "': " + resM);
            }
        }

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

                    data.Add("");
                }
                data.Add("");
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