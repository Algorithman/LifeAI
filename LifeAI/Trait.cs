using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeAI
{
    public class Trait
    {
        public string Name { get; private set; }

        public static List<Trait> mainTraitList = new List<Trait>();
        private static int mainTraitCount { get { return mainTraitList.Count; } }

        public Trait(string name)
        {
            this.Name = name;
            mainTraitList.Add(this);
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public int TraitCount()
        {
            return mainTraitCount;
        }

        public static Trait GetTrait(int number)
        {
            int ct = 0;
            foreach (Trait trait in mainTraitList)
            {
                if (ct == number)
                {
                    return trait;
                }
                ct++;
            }
            return null;
        }

        public static Trait GetTrait(string name)
        {
            foreach (Trait trait in mainTraitList)
            {
                if (trait.Name == name)
                {
                    return trait;
                }
            }
            return null;
        }
    }
}
