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
            
            List<Hypotheses> id3 = ADABoost.weightedMajorityHypotheses(caseSets.Item1, ID3.generateHypothesis, 10);
            Console.WriteLine("ID3: " + ADABoost.test(id3, caseSets.Item2));
            
            //List<Hypotheses> naiveBayes = ADABoost.weightedMajorityHypotheses(caseSets.Item1, NaiveBayes.generateHypothesis, 10);

        }
    }
}