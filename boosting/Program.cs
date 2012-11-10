using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Algorithms;
using System.IO;

namespace boosting
{
    class Program
    {
        static readonly String location;
        
        static void Main(string[] args)
        {
            
            
            List<Hypotheses> id3 = ADABoost.weightedMajorityHypotheses(trainingSet, ID3.generateHypothesis, 10);
            //List<Hypotheses> naiveBayes = ADABoost.weightedMajorityHypotheses(trainingSet, NaiveBayes.generateHypothesis, 10);

            Console.ReadLine();
        }

        Tuple<List<Case>, List<Case>> getCaseSetsFromFile(String fileName)
        {
            var reader = new StreamReader(File.OpenRead(@location + fileName));
            List<string> valueNames = reader.ReadLine().Split(',').ToList();
            List<Case> cases = new List<Case>();
            while (!reader.EndOfStream)
            {
                List<double> attributes = reader.ReadLine().Split(',').Select(s => double.Parse(s)).ToList();
                double classification = attributes.Last();
                attributes.RemoveAt(-1);
                Case c = new Case(attributes, classification);
                cases.Add(c);
            }

            List<Case> trainingSet = new List<Case>();
            trainingSet.AddRange(cases.Take((int)(cases.Count * 0.8)));

            List<Case> testSet = new List<Case>();
            testSet.AddRange(cases.Skip((int)(cases.Count * 0.8)));

            return new Tuple<List<Case>, List<Case>>(trainingSet, testSet);
        }
    }
}