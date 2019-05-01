using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class ModifyData
    {
        public Trait Trait;
        public float Basic;
        public float Average;
        public int CorrelationCount
        {
            get { return CorrelationList.Count(); }
        }
        public List<CorrelationData> CorrelationList = new List<CorrelationData>();
    }
}
