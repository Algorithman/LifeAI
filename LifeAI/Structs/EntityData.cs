using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class EntityData
    {
        public Entity Entity;
        public Action StartAction;
        public RecordData StartRecord = new RecordData();
        public List<TraitAmountData> TraitAmountList=new List<TraitAmountData>();
        public List<ValueTraitData> ValueTraitList = new List<ValueTraitData>();
        public List<EntityData> ParentList=new List<EntityData>();
        public List<EntityData> ChildList=new List<EntityData>();
        public List<StepData> StepList=new List<StepData>();
        public List<ActionData> PlanActionList= new List<ActionData>();
        public List<EntityData> ReactEntityList=new List<EntityData>();
        public StepData StepTrack = null;
        public int StepLimit = 0;
        public int StepCount = 0;
    }
}
