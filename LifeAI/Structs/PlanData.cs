using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class PlanData
    {
        public int Step;
        public Action Action;
        public List<Entity> Member = new List<Entity>();
        public float Value;
        public float TotalValue;
        public float SelfValue;
    }
}
