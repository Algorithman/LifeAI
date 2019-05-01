using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI.Structs
{
    public class ActionData
    {
        public Action Action;
        public int RecordCount;
        public List<ActionData> ParentList = new List<ActionData>();
        public List<ActionData> ChildList = new List<ActionData>();
        public List<MemberData> MemberList = new List<MemberData>();
        public List<RecordData> RecordList = new List<RecordData>();
    }
}
