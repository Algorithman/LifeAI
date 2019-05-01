using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class StepData
    {
        public PlanData TempPlan= new PlanData();
        public PlanData BestPlan= new PlanData();
        public List<AltTraitData> AltTraitList=new List<AltTraitData>();
    }
}
