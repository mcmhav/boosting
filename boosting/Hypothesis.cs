using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    abstract class Hypotheses
    {
        public double weight;
        
        public abstract double classify(List<double> attributes);
        
        public void setWeight(double weight)
        {
            this.weight = weight;
        }
    }
}
