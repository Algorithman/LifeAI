using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class MemberData
    {
        public int Number;
        public Entity ParentEntity;
        public List<ModifyData> ModifyList=new List<ModifyData>();
    }
}
