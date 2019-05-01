using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI
{
    public class Action
    {
        private static List<Action> mainActionList = new List<Action>();
        private static int mainActionCount = 0;


        private int memberCount = 0;
        public int MemberCount
        {
            get
            {
                return this.memberCount;
            }
            private set
            {
                int temp = Math.Max(Math.Min(value, 4), 1);
                memberCount = temp;
            }
        }
        public string Name { get; private set; }
        public int Number { get; private set; }

        public Action(int memberCount, string actionName)
        {
            mainActionCount++;
            this.Number = mainActionCount;
            this.MemberCount = memberCount;

            this.Name = actionName;
            mainActionList.Add(this);
        }

        public Action GetAction(int number)
        {
            foreach (Action action in mainActionList)
            {
                if (action.Number == number)
                {
                    return action;
                }
            }
            return null;
        }

        public Action GetAction(string name)
        {
            foreach (Action action in mainActionList)
            {
                if (action.Name == name)
                {
                    return action;
                }
            }
            return null;
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public int ActionCount()
        {
            return mainActionCount;
        }

    }
}
