using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boosting
{
    class Case
    {
        public List<double> attributes { get; private set; }
        public double classification { get; private set; }

        public Case(List<double> attributes, double classification = -9999)
        {
            this.attributes = attributes;
            this.classification = classification;
        }
    }
}
