using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeAI.Structs;

namespace LifeAI
{
    public class Entity
    {
        const int LAI_DEF_VALUE = 0;
        const int LAI_DEF_STANDARD = 1;
        const int LAI_DEF_RECORD = 0;
        const int LAI_DEF_AMOUNT = 0;
        const int LAI_ACTOR_MEMBER = 0;
        const int LAI_MAX_RECORD = 32;
        const int LAI_DEF_REACT_STEPS = 1;
        const int LAI_DEF_STEPS = 1;
        const int LAI_MAX_MEMBER = 4;
        const int LAI_MIN_ACTION_VALUE = -99999;
        const int LAI_MIN_REACTION_VALUE = -99999;
        const int LAI_MIN_RECORD_AVERAGE = 4;
        const int LAI_MIN_RECORD_CORRELATION = 4;
        const float LAI_MIN_COEF = 0.8f;
        const int LAI_DEF_SHIFT = 1;

        private static List<Entity> MainEntityList = new List<Entity>();
        private static int MainEntityCount = 0;

        public string Name { get; private set; }
        public int Number { get; private set; }


        private List<EntityData> entityList = new List<EntityData>();
        private List<ActionData> actionList = new List<ActionData>();
        private List<PlanData> planList = new List<PlanData>();
        private List<AltTraitData> reactTraitList = new List<AltTraitData>();
        private EntityData mainEntityItem = new EntityData();
        private EntityData self = null;
        private int reactSteps = 0;


        public Entity(string name)
        {
            this.Name = name;
            MainEntityCount++;
            this.Number = MainEntityCount;
            MainEntityList.Add(this);
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public void SetTrait(Entity entity, Trait trait, float amount, bool offset = false)
        {
            EntityData ed = FindEntity(entity, true);
            TraitAmountData traitAmountData = FindTraitAmount(ed, trait, true);
            if (traitAmountData != null)
            {
                if (!offset)
                {
                    traitAmountData.Amount = amount;

                }
                else
                {
                    traitAmountData.Amount += amount;
                }
            }

        }
        public TraitAmountData FindTraitAmount(EntityData entity, Trait trait, bool add = false)
        {
            TraitAmountData tmd = entity.TraitAmountList.FirstOrDefault(x => x.Trait == trait);
            if (tmd != null)
            {
                return tmd;
            }
            if (add)
            {
                return AddTraitAmount(entity, trait);
            }
            return null;
        }

        public EntityData FindEntity(Entity entity, bool add = false)
        {
            foreach (EntityData entityData in this.entityList)
            {
                if (entityData.Entity == entity)
                {
                    return entityData;
                }
            }
            if (add)
            {
                return AddEntity(entity);
            }
            return null;
        }

        private EntityData AddEntity(Entity entity)
        {
            EntityData entityData = new EntityData()
            {
                Entity = entity,
            };
            entityList.Add(entityData);
            return entityData;
        }

        private TraitAmountData AddTraitAmount(EntityData entityData, Trait trait)
        {
            TraitAmountData traitAmountData = new TraitAmountData();
            traitAmountData.Trait = trait;
            entityData.TraitAmountList.Add(traitAmountData);
            return traitAmountData;
        }

        private ValueTraitData AddValueTrait(EntityData entityData, Entity traitOwner, Trait trait)
        {
            ValueTraitData valueTraitData = new ValueTraitData();
            entityData.ValueTraitList.Add(valueTraitData);
            valueTraitData.TraitOwner = traitOwner;
            valueTraitData.Trait = trait;
            valueTraitData.Value = LAI_DEF_VALUE;
            valueTraitData.Standard = LAI_DEF_STANDARD;

            // Add if not already in list
            FindEntity(traitOwner, true);
            return valueTraitData;
        }

        private EntityData AddEntityParent(EntityData entityData, EntityData parentData)
        {
            entityData.ParentList.Add(parentData);
            return parentData;
        }

        private EntityData AddEntityChild(EntityData entityData, EntityData child)
        {
            entityData.ChildList.Add(child);
            return child;
        }


        private RecordData AddStartRecord(EntityData ed)
        {
            ed.StartRecord = new RecordData();
            return ed.StartRecord;
        }

        private ActionData AddAction(Action action)
        {
            ActionData newAction = new ActionData();
            newAction.Action = action;
            newAction.RecordCount = LAI_DEF_RECORD;
            actionList.Add(newAction);

            for (int i = 0; i < action.MemberCount; i++)
            {
                AddMember(newAction, i);
            }
            return newAction;
        }

        private MemberData AddMember(ActionData a, int number)
        {
            MemberData mb = new MemberData();
            a.MemberList.Add(mb);
            mb.Number = number;
            mb.ParentEntity = null;
            return mb;
        }
        private ActionData AddActionParent(ActionData ad, ActionData parent)
        {
            ad.ParentList.Add(parent);
            return parent;
        }

        private ActionData AddActionChild(ActionData ad, ActionData child)
        {
            ad.ChildList.Add(child);
            return child;
        }

        private ModifyData AddModify(MemberData m, Trait trait)
        {
            ModifyData md = new ModifyData();
            m.ModifyList.Add(md);
            md.Trait = trait;
            md.Basic = LAI_DEF_AMOUNT;
            md.Average = LAI_DEF_AMOUNT;
            return md;
        }

        private CorrelationData AddCorrelation(ModifyData md)
        {
            CorrelationData cd = new CorrelationData();
            md.CorrelationList.Add(cd);
            cd.Member = LAI_ACTOR_MEMBER;
            cd.Trait = null;
            cd.Coefficient = LAI_DEF_AMOUNT;
            cd.Slope = LAI_DEF_AMOUNT;
            cd.Intercept = LAI_DEF_AMOUNT;
            return cd;
        }

        private RecordData AddRecord(ActionData ad)
        {
            RecordData rd = new RecordData();
            ad.RecordList.Add(rd);
            ad.RecordCount++;

            if (ad.RecordCount > LAI_MAX_RECORD)
            {
                RemoveRecord(ad, ad.RecordList[0]);
                ad.RecordCount = LAI_MAX_RECORD;
            }
            return rd;
        }

        private void RemoveRecord(ActionData ad, RecordData recordData)
        {
            foreach (RecordMemberData rmd in recordData.RecordMemberList.ToArray())
            {
                RemoveRecordMember(recordData, rmd);
            }
            ad.RecordList.Remove(recordData);
            ad.RecordCount--;
        }

        private void RemoveRecordMember(RecordData recordData, RecordMemberData rmd)
        {
            rmd.RecordTraitList.Clear();
            recordData.RecordMemberList.Remove(rmd);
        }

        private RecordMemberData AddRecordMember(RecordData rd, int number)
        {
            RecordMemberData rmd = new RecordMemberData();
            rd.RecordMemberList.Add(rmd);

            rmd.Entity = null;
            rmd.Number = number;
            return rmd;
        }

        private RecordTraitData AddRecordTrait(RecordMemberData rmd)
        {
            RecordTraitData rt = new RecordTraitData();
            rmd.RecordTraitList.Add(rt);
            rt.Trait = null;
            rt.StartAmount = LAI_DEF_AMOUNT;
            rt.EndAmount = LAI_DEF_AMOUNT;
            rt.Change = LAI_DEF_AMOUNT;
            return rt;
        }

        private StepData AddStep(EntityData ed, int step)
        {
            StepData sd = new StepData();
            ed.StepList.Add(sd);

            sd.TempPlan.Action = null;
            sd.TempPlan.Step = step;
            sd.TempPlan.Value = LAI_DEF_VALUE;
            sd.TempPlan.TotalValue = LAI_DEF_VALUE;
            sd.BestPlan.Action = null;
            sd.BestPlan.Value = LAI_DEF_VALUE;
            sd.BestPlan.TotalValue = LAI_DEF_VALUE;

            return sd;
        }


        private AltTraitData AddAltTraitAmount(StepData sd)
        {
            AltTraitData ad = new AltTraitData();
            sd.AltTraitList.Add(ad);
            return ad;
        }

        private PlanData AddPlan(int step)
        {
            PlanData pd = new PlanData();
            planList.Add(pd);
            pd.Step = step;
            pd.Action = null;
            pd.Value = LAI_DEF_VALUE;
            pd.TotalValue = LAI_DEF_VALUE;
            return pd;
        }

        private void RemoveEntity(EntityData e)
        {
            e.TraitAmountList.Clear();
            e.ValueTraitList.Clear();

            e.ParentList.Clear();
            e.ChildList.Clear();

            planList.Clear();

            foreach (EntityData ed in entityList)
            {
                if (ed != e)
                {
                    foreach (ValueTraitData vtd in ed.ValueTraitList.Where(x => x.TraitOwner == e.Entity).ToArray())
                    {
                        ed.ValueTraitList.Remove(vtd);
                    }

                    EntityData parent = FindEntityParent(ed, e);
                    if (parent != null)
                    {
                        RemoveEntityParent(ed, parent);
                    }

                    EntityData child = FindEntityChild(ed, e);
                    if (child != null)
                    {
                        RemoveEntityChild(ed, child);
                    }

                    foreach (RecordMemberData rmd in ed.StartRecord.RecordMemberList.Where(x => x.Entity == e.Entity).ToArray())
                    {
                        ed.StartRecord.RecordMemberList.Remove(rmd);
                    }
                }
            }

            foreach (ActionData ad in actionList)
            {
                foreach (RecordData rd in ad.RecordList.ToArray())
                {
                    foreach (RecordMemberData rmd in rd.RecordMemberList.Where(x => x.Entity == e.Entity).ToArray())
                    {
                        ad.RecordList.Remove(rd);
                    }
                }
            }

            entityList.Remove(e);
        }

        private EntityData FindEntityChild(EntityData ed, EntityData c, bool add = false)
        {
            EntityData result = c.ChildList.FirstOrDefault(x => x == c);
            if (result != null)
            {
                return result;
            }
            if (add)
            {
                AddEntityParent(c, ed);
                return AddEntityChild(ed, c);
            }
            return null;
        }

        private void RemoveEntityParent(EntityData ed, EntityData parent)
        {
            ed.ParentList.Remove(parent);
        }


        private void RemoveEntityChild(EntityData ed, EntityData child)
        {
            ed.ChildList.Remove(child);
        }



        private EntityData FindEntityParent(EntityData e, EntityData p, bool add = false)
        {
            EntityData result = e.ParentList.FirstOrDefault(x => x == p);
            if (result != null)
            {
                return result;
            }
            if (add)
            {
                AddEntityChild(p, e);
                return AddEntityParent(e, p);
            }
            return null;
        }


        public void SetValue(Entity entity, Trait trait, float value, float standard = LAI_DEF_STANDARD)
        {
            SetValue(entity, entity, trait, value, standard);
        }
        public void SetValue(Entity entity, Entity traitOwner, Trait trait, float value, float standard = LAI_DEF_STANDARD)
        {
            EntityData ed = FindEntity(entity, true);
            if (ed != null)
            {
                ValueTraitData vd = FindValueTrait(ed, traitOwner, trait, true);
                if (vd != null)
                {
                    vd.Value = value;
                    vd.Standard = standard;
                }
            }
        }

        private ValueTraitData FindValueTrait(EntityData ed, Entity traitOwner, Trait trait, bool add = false)
        {
            ValueTraitData result = ed.ValueTraitList.FirstOrDefault(x => x.TraitOwner == traitOwner);
            if (result != null)
            {
                return result;
            }
            if (add)
            {
                return AddValueTrait(ed, traitOwner, trait);
            }
            return null;
        }

        public void SetParent(Entity entity, Entity parent, bool isParent = true)
        {
            EntityData ed = FindEntity(entity, true);
            if (ed != null)
            {
                EntityData ep = FindEntity(parent, true);
                if (ep != null)
                {
                    EntityData par = FindEntityParent(ed, ep, isParent);
                    if (!isParent && (par != null))
                    {
                        RemoveEntityParent(ed, par);
                        EntityData c = FindEntityChild(ep, ed);
                        if (c != null)
                        {
                            RemoveEntityChild(ep, c);
                        }
                    }
                }
            }
        }

        private void SetChild(Entity entity, Entity child, bool isChild)
        {
            EntityData e = FindEntity(entity, true);
            if (e != null)
            {
                EntityData e2 = FindEntity(child, true);
                if (e2 != null)
                {
                    EntityData c = FindEntityChild(e, e2, isChild);
                    if (!isChild && (c != null))
                    {
                        RemoveEntityChild(e, c);
                        EntityData p = FindEntityParent(e2, e);
                        if (p != null)
                        {
                            RemoveEntityParent(e2, p);
                        }
                    }
                }
            }
        }

        private void SetParent(Action action, Action parent, bool isParent = true)
        {
            ActionData a = FindAction(action, true);
            if (a != null)
            {
                ActionData a2 = FindAction(parent, true);
                if (a2 != null)
                {
                    ActionData p = FindActionParent(a, a2, isParent);
                    if (!isParent && (p != null))
                    {
                        RemoveActionParent(a, p);
                        ActionData c = FindActionChild(a2, a);
                        if (c != null)
                        {
                            RemoveActionChild(a2, c);
                        }

                    }
                }
            }
        }

        private void SetChild(Action action, Action child, bool isChild)
        {
            ActionData a = FindAction(action, true);
            if (a != null)
            {
                ActionData a2 = FindAction(child, true);
                if (a2 != null)
                {
                    ActionData c = FindActionChild(a, a2, isChild);
                    if (!isChild && (c != null))
                    {
                        RemoveActionChild(a, c);
                        ActionData p = FindActionParent(a2, a);
                        if (p != null)
                        {
                            RemoveActionParent(a2, p);
                        }
                    }
                }
            }
        }


        public void SetMemberParent(Action action, int member, Entity parent)
        {
            ActionData a = FindAction(action, true);
            if (a != null)
            {
                MemberData m = FindMember(a, member, true);
                if (m != null)
                {
                    m.ParentEntity = parent;
                }
            }
        }


        public void SetBasic(Action action, int member, Trait trait, float modify = LAI_DEF_SHIFT)
        {
            ActionData a = FindAction(action, true);
            MemberData m = FindMember(a, member, true);
            ModifyData mod = FindModify(m, trait, true);
            if (mod != null)
            {
                mod.Basic = modify;
            }
        }

        private ModifyData FindModify(MemberData m, Trait trait, bool add = false)
        {
            ModifyData mod = m.ModifyList.FirstOrDefault(x => x.Trait == trait);
            if (mod != null)
            {
                return mod;
            }
            if (add)
            {
                return AddModify(m, trait);
            }
            return null;
        }

        private MemberData FindMember(ActionData a, int number, bool add = false)
        {
            MemberData m = a.MemberList.FirstOrDefault(x => x.Number == number);
            if (m != null)
            {
                return m;
            }
            if (add)
            {
                return AddMember(a, number);
            }
            return null;
        }

        private void RemoveActionChild(ActionData a, ActionData c)
        {
            a.ChildList.Remove(c);
        }

        private ActionData FindActionChild(ActionData a, ActionData c, bool add = false)
        {
            ActionData a2 = a.ChildList.FirstOrDefault(x => x == c);
            if (a2 != null)
            {
                return a2;
            }
            if (add)
            {
                AddActionParent(c, a);
                return AddActionChild(a, c);
            }
            return null;
        }

        private void RemoveActionParent(ActionData a, ActionData p)
        {
            a.ParentList.Remove(p);
        }

        private ActionData FindActionParent(ActionData a, ActionData p, bool add = false)
        {
            ActionData a2 = a.ParentList.FirstOrDefault(x => x == p);
            if (a2 != null)
            {
                return a2;
            }
            if (add)
            {
                AddActionChild(p, a);
                return AddActionParent(a, p);
            }
            return null;
        }

        private ActionData FindAction(Action action, bool add = false)
        {
            ActionData a = actionList.FirstOrDefault(x => x.Action == action);
            if (a != null)
            {
                return a;
            }
            if (add)
            {
                return AddAction(action);
            }
            return null;
        }

        public float GetTrait(Entity entity, Trait trait)
        {
            EntityData e = FindEntity(entity);
            if (e != null)
            {
                TraitAmountData a = FindTraitAmount(e, trait);
                if (a != null)
                {
                    return a.Amount;
                }
            }
            return LAI_DEF_AMOUNT;
        }

        public float GetValue(Entity entity, Trait trait, bool standard = false)
        {
            return GetValue(entity, entity, trait, standard);
        }

        private float GetValue(Entity entity, Entity traitOwner, Trait trait, bool standard = false)
        {
            EntityData e = FindEntity(entity);
            if (e != null)
            {

                ValueTraitData v = FindValueTrait(e, traitOwner, trait);
                if (v != null)
                {
                    if (!standard)
                    {
                        return v.Value;
                    }
                    else
                    {
                        return v.Standard;
                    }
                }
            }
            return LAI_DEF_VALUE;
        }

        private bool GetParent(Entity entity, Entity parent)
        {
            EntityData e = FindEntity(entity);
            if (e != null)
            {
                EntityData e2 = FindEntity(parent);
                if (e2 != null)
                {
                    if (FindEntityParent(e, e2) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool GetChild(Entity entity, Entity child)
        {
            EntityData e = FindEntity(entity);
            if (e != null)
            {
                EntityData e2 = FindEntity(child);
                if (e2 != null)
                {
                    if (FindEntityChild(e, e2) != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private Action GetStartAction(Entity entity = null)
        {
            if (entity == null)
            {
                entity = this;
            }
            EntityData e = FindEntity(entity);
            if (e != null)
            {
                return e.StartAction;
            }
            return null;
        }

        private Entity GetStartEntity(Entity entity, int member = 0)
        {
            EntityData e = FindEntity(entity);
            if (e != null)
            {
                RecordMemberData m = e.StartRecord.RecordMemberList.FirstOrDefault(x => x.Number == member);
                if (m != null)
                {
                    return m.Entity;
                }
            }
            return null;
        }

        public Action GetPlan(int step = 0)
        {
            PlanData p = FindPlan(step);
            if (p != null)
            {
                return p.Action;
            }
            return null;
        }

        private PlanData FindPlan(int step, bool add = false)
        {
            PlanData d = planList.FirstOrDefault(x => x.Step == step);
            if (d != null)
            {
                return d;
            }
            if (add)
            {
                return AddPlan(step);
            }
            return null;
        }

        public Entity GetPlanEntity(int step = 0, int member = 0)
        {
            PlanData p = FindPlan(step);
            if (p != null)
            {
                return p.Member[member];
            }
            return null;
        }

        public float GetPlanValue(int step = 0, bool total = false)
        {
            PlanData p = FindPlan(step);
            if (p != null)
            {
                if (total)
                {
                    return p.TotalValue;
                }
                return p.Value;
            }
            return LAI_DEF_VALUE;
        }


        public void Plan(int steps = LAI_DEF_STEPS, int reactSteps = LAI_DEF_REACT_STEPS, Action parentAction = null, Entity reactParentEntity = null, float minimumValue = LAI_MIN_ACTION_VALUE)
        {
            PlanData p;

            if (steps < 1)
            {
                return;
            }

            planList.Clear();

            self = FindEntity(this);
            if ((self != null) && (self.ValueTraitList.Any()))
            {
                this.reactSteps = reactSteps;
                PlanStart(self, steps, reactSteps, parentAction, reactParentEntity, minimumValue);

                foreach (StepData s in self.StepList)
                {
                    PlanData b = s.BestPlan;
                    if (b.Action != null)
                    {
                        p = AddPlan(b.Step);

                        if (p != null)
                        {
                            p.Action = b.Action;
                            p.Member.Clear();
                            foreach (Entity m in b.Member)
                            {
                                p.Member.Add(m);
                            }
                            p.Value = b.Value;
                            p.TotalValue = b.TotalValue;
                        }
                    }
                    s.AltTraitList.Clear();
                }
                self.StepList.Clear();
            }
        }

        private void PlanStart(EntityData e, int steps, int reactSteps, Action parentAction, Entity reactParentEntity, float minimumValue)
        {
            e.StepLimit = steps - 1;
            e.StepCount = 0;
            e.StepList.Clear();
            for (int i = 0; i < steps; i++)
            {
                StepData s = AddStep(e, i);
                if (s != null)
                {
                    s.BestPlan.TotalValue = minimumValue;
                }
            }

            e.StepTrack = e.StepList.First();

            if (parentAction != null)
            {
                ActionData a = FindAction(parentAction);
                if (a != null)
                {
                    e.PlanActionList = a.ChildList;
                    if (!e.PlanActionList.Any())
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                e.PlanActionList = actionList;
            }

            if (reactSteps > 0)
            {
                if (reactParentEntity != null)
                {
                    EntityData re = FindEntity(reactParentEntity);
                    if (re != null)
                    {
                        e.ReactEntityList = re.ChildList;
                        if (!e.ReactEntityList.Any())
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    e.ReactEntityList = entityList;
                }
            }
            ActionStep(e);
        }

        private void ActionStep(EntityData actor)
        {
            List<EntityData> e = new List<EntityData>();

            // Reaction or preaction?
            bool preaction = false;
            bool reaction = false;
            float modifyAmount = 0.0f;
            AltTraitData alt = null;
            if (reactSteps > 0)
            {
                if (actor == self)
                {
                    preaction = true;
                }
                else
                {
                    reaction = true;
                }
            }

            e.Add(actor);


            List<List<EntityData>> planEntityList = new List<List<EntityData>>();
            planEntityList.Add(new List<EntityData>() { actor });

            foreach (ActionData a in actor.PlanActionList)
            {
                bool hasEntityList = true;
                MemberData m = a.MemberList.First();
                if (m.ParentEntity != null)
                {
                    hasEntityList = false;
                    EntityData parent = FindEntity(m.ParentEntity);
                    if (parent != null)
                    {
                        if (parent.ChildList.Any(x => x == actor))
                        {
                            hasEntityList = true;
                        }
                    }
                }

                if (hasEntityList)
                {
                    foreach (MemberData m2 in a.MemberList.Where(x => x != m))
                    {
                        if (m2.ParentEntity != null)
                        {

                            EntityData parent = FindEntity(m2.ParentEntity);
                            if (parent != null)
                            {
                                planEntityList[m.Number] = parent.ChildList;
                                if (!parent.ChildList.Any())
                                {
                                    hasEntityList = false;
                                    break;
                                }
                                else
                                {
                                    planEntityList.Add(parent.ChildList);
                                }
                            }
                            else
                            {
                                hasEntityList = false;
                                break;
                            }
                        }
                        else
                        {
                            planEntityList.Add(entityList);
                        }
                    }
                }

                if (hasEntityList)
                {
                    List<EntityData> usedEntities = new List<EntityData>();
                    usedEntities.Add(actor);
                    Stack<IEnumerator<EntityData>> eStack = new Stack<IEnumerator<EntityData>>();
                    foreach (List<EntityData> el in planEntityList)
                    {
                        IEnumerator<EntityData> tempx = el.GetEnumerator();
                        tempx.Reset();
                        tempx.MoveNext();
                        eStack.Push(tempx);
                    }
                    bool finished = false;
                    while (!finished)
                    {
                        // Skip iterations with double entities 
                        if (!eStack.GroupBy(x => x.Current).Where(g => g.Count() > 1).Any())
                        {
                            float valueSum = LAI_DEF_VALUE;
                            float selfValueSum = LAI_DEF_VALUE;

                            // Optionally display action information when
                            // LAI_DEBUG is enabled
                            Trace.WriteLine("");
                            Trace.WriteLine("");
                            if (preaction)
                            {
                                Trace.WriteLine("PREACTION");
                            }
                            else
                            {
                                if (reaction)
                                {
                                    Trace.WriteLine("REACTION");
                                }
                                else
                                {
                                    Trace.WriteLine("ACTION");
                                }
                            }
                            Trace.WriteLine(string.Format("          Step: {0}      Action: {1}", actor.StepCount, a.Action.Name));
                            foreach (string n in eStack.Select(x => x.Current.Entity.Name))
                            {
                                Trace.WriteLine("          Member: " + n);
                            }
                            // Debug END

                            int cm2 = 0;
                            foreach (MemberData m2 in a.MemberList)
                            {
                                foreach (ModifyData mod in m.ModifyList)
                                {
                                    float traitAmount = LAI_DEF_AMOUNT;
                                    float traitAmount2 = LAI_DEF_AMOUNT;
                                    float valueTrait = LAI_DEF_VALUE;
                                    float valueStandard = LAI_DEF_STANDARD;
                                    float selfValueTrait = LAI_DEF_VALUE;
                                    float selfValueStandard = LAI_DEF_STANDARD;
                                    bool skipCor = false;

                                    TraitAmountData t = FindTraitAmount(eStack.ElementAt(cm2).Current, mod.Trait);
                                    if (t != null)
                                    {
                                        traitAmount = t.Amount;
                                    }
                                    else
                                    {
                                        skipCor = true;
                                    }

                                    // Check to see if member entity's trait value is known
                                    // If unknown, defaults to 0 (indifference)  May not
                                    // always be accurate, but will prevent need for
                                    // excessive data entry.
                                    ValueTraitData v = FindValueTrait(eStack.ElementAt(LAI_ACTOR_MEMBER).Current, eStack.ElementAt(cm2).Current.Entity, mod.Trait);
                                    if (v != null)
                                    {
                                        valueTrait = v.Value;
                                        valueStandard = v.Standard;
                                    }

                                    if (reaction)
                                    {
                                        ValueTraitData sv = FindValueTrait(self, eStack.ElementAt(cm2).Current.Entity, mod.Trait);
                                        if (sv != null)
                                        {
                                            selfValueTrait = sv.Value;
                                            selfValueStandard = sv.Standard;
                                        }
                                    }

                                    // Set correlation count and sum to default
                                    // Do here because use amountCount as a flag for
                                    // whether to calculate basic/average further below
                                    int amountCount = 0;
                                    float amountSum = 0.0f;

                                    // Base the calculations on the number of records
                                    // of the action acquired
                                    if (!skipCor && (mod.CorrelationCount > 0))
                                    {
                                        foreach (CorrelationData c in mod.CorrelationList)
                                        {
                                            TraitAmountData t2 = FindTraitAmount(eStack.ElementAt(cm2).Current, c.Trait);
                                            if (t2 != null)
                                            {
                                                traitAmount2 = t2.Amount;
                                            }
                                            else
                                            {
                                                // If relevant traits unknown for a correlation,
                                                // safest option is to default to calculating
                                                // with modify average.

                                                skipCor = true;
                                                break;
                                            }


                                            amountSum += c.Slope * traitAmount2 + c.Intercept;
                                            amountCount++;
                                        }

                                        // Average the correlation results to get an
                                        // approximate trait amount change
                                        if (!skipCor && (amountSum > 0))
                                        {
                                            modifyAmount = (float)(amountSum / amountCount);
                                        }
                                    }

                                    // No valid correlation data found, so default
                                    // to basic/average modify amounts
                                    if (amountCount == 0)
                                    {
                                        if (a.RecordCount < LAI_MIN_RECORD_AVERAGE)
                                        {
                                            modifyAmount = mod.Basic;
                                        }
                                        else
                                        {
                                            modifyAmount = mod.Average;
                                        }
                                    }

                                    if (modifyAmount > 0)
                                    {
                                        // Lock in the alt trait amount before continuing
                                        // to next step.  Adds to altTraitList for step.
                                        // Only do this if there is a future step

                                        // If don't know the original trait amount, don't
                                        // store an alt trait.  Wouldn't be reliable as
                                        // an influencing trait in future steps.  If
                                        // nothing modified, no need to generate an alt
                                        // trait record either

                                        if (t != null)
                                        {
                                            if (preaction || reaction || (actor.StepCount < actor.StepLimit))
                                            {
                                                alt = AddAltTraitAmount(actor.StepTrack);
                                                // Keep a copy of entity's original amount and
                                                // the alt modified amount.  Don't want to change
                                                // their trait amount yet since would affect the
                                                // outcome of action
                                                if (alt != null)
                                                {
                                                    // Can use traitItem instead of trait because more
                                                    // efficient.  Address location won't change in planning.
                                                    // points to this specific member entity's traitAmount
                                                    alt.Trait = t;
                                                    alt.Amount = traitAmount;
                                                    alt.AltAmount = traitAmount + modifyAmount;
                                                    alt.ModAmount = modifyAmount;
                                                }
                                            }
                                        }


                                        // Calculate the value of the trait change
                                        if ((valueStandard > 0) && (valueTrait > 0))
                                        {
                                            valueSum += (modifyAmount / valueStandard) * valueTrait;
                                        }
                                        if (reaction && (selfValueStandard > 0) && (selfValueTrait > 0))
                                        {
                                            selfValueSum += (modifyAmount / selfValueStandard) * selfValueTrait;
                                        }
                                    }
                                } // Modifier iteration END
                            } // Member iteration END


                            // DEBUG Start
                            if (reaction)
                            {
                                Trace.WriteLine("      valueSum: " + valueSum.ToString("n3"));
                                Trace.WriteLine("  selfvalueSum: " + selfValueSum.ToString("n3"));
                            }
                            // DEBUG END

                            if (preaction)
                            {
                                // Need to put in alt amounts before reaction,
                                // and after reaction (if there is another step)
                                // since the amounts can change.
                                foreach (AltTraitData altx in actor.StepTrack.AltTraitList)
                                {
                                    alt.Trait.Amount = alt.AltAmount;
                                }

                                // Calculate reactions of entities involved in action
                                // Start with member 1, since don't involve actor
                                // entity in reaction
                                foreach (EntityData entity in actor.ReactEntityList)
                                {
                                    // Only involve entities that have a value
                                    // toward something, implying the volition to act
                                    if ((entity != actor) && (entity.ValueTraitList.Any()))
                                    {
                                        // Setting "actionParent" to null implies the
                                        // reacting entity will make use of full action list
                                        PlanStart(entity, reactSteps, 0, null, null, LAI_MIN_REACTION_VALUE);

                                        // Only factor in the first step's value to actor
                                        StepData s = entity.StepList.FirstOrDefault();
                                        if (s!=null)
                                        {
                                            // Get the first step self value
                                            // and add to the original entity's valueSum
                                            // Use first step because want to interleave
                                            // preaction steps with reaction steps.
                                            valueSum += s.BestPlan.SelfValue;

                                            // Add alt trait info to preaction entities alt trait-lists.
                                            // reactTraitList will only be generated if
                                            // there is best action data, so don't need
                                            // additional conditional about if there is best action

                                            foreach (AltTraitData alt2 in this.reactTraitList)
                                            {
                                                bool altFound = false;
                                                foreach (AltTraitData alt3 in actor.StepTrack.AltTraitList)
                                                {
                                                    if (alt.Trait == alt3.Trait)
                                                    {
                                                        alt3.AltAmount += alt2.ModAmount;
                                                        alt3.ModAmount += alt2.ModAmount;
                                                        altFound = true;
                                                        break;
                                                    }
                                                }
                                                if (!altFound)
                                                {
                                                    alt = AddAltTraitAmount(actor.StepTrack);
                                                    alt.Trait = alt2.Trait;
                                                    // No alt trait occurred for preaction entity for
                                                    // this trait, and at this point the amount has
                                                    // been returned to the start of the preaction,
                                                    // so the trait amount referenced here is valid.
                                                    alt.Amount = alt2.Trait.Amount;
                                                    alt.AltAmount = alt.Amount + alt2.ModAmount;
                                                    alt.ModAmount = alt2.ModAmount;
                                                }
                                            }
                                            // Clean out reactTraitList
                                            this.reactTraitList.Clear();

                                            // Need to delete alt trait list before deleting
                                            // the steps further down
                                            entity.StepList.Clear();
                                        }
                                        // Need to delete alt trait list before deleting
                                        // the steps further down
                                        entity.StepList.Clear();

                                    }
                                } // END Iterator actor.ReactEntityList
                            }

                            // DEBUG Start
                            if (preaction)
                            {
                                Trace.WriteLine("valueSum after reaction: " + valueSum.ToString("n3"));
                            }
                            // DEBUG END

                            actor.StepTrack.TempPlan.Step = actor.StepCount;
                            actor.StepTrack.TempPlan.Action = a.Action;
                            foreach (Entity ed in eStack.Select(x => x.Current.Entity))
                            {
                                actor.StepTrack.TempPlan.Member.Add(ed);
                            }
                            actor.StepTrack.TempPlan.Value = valueSum;

                            if (actor.StepCount > 0)
                            {
                                StepData s = actor.StepList[actor.StepList.IndexOf(actor.StepTrack) - 1];
                                actor.StepTrack.TempPlan.TotalValue = valueSum + s.TempPlan.TotalValue;
                                actor.StepTrack.TempPlan.SelfValue = selfValueSum + s.TempPlan.SelfValue;
                            }
                            else
                            {
                                actor.StepTrack.TempPlan.TotalValue = valueSum;
                                actor.StepTrack.TempPlan.SelfValue = selfValueSum;
                            }


                            if (actor.StepCount < actor.StepLimit)
                            {
                                // Lock in alt traits if has multiple steps
                                foreach (AltTraitData alt2 in actor.StepTrack.AltTraitList)
                                {
                                    alt.Trait.Amount = alt.Amount;
                                }

                                // Increment StepData count/list
                                actor.StepCount++;
                                actor.StepTrack = null;
                                if (actor.StepTrack != actor.StepList.Last())
                                {
                                    actor.StepTrack = actor.StepList[actor.StepList.IndexOf(actor.StepTrack) + 1];
                                }

                                // Perform next step
                                ActionStep(actor);

                                // Decrement step again
                                actor.StepCount--;
                                actor.StepTrack = actor.StepList[actor.StepList.IndexOf(actor.StepTrack) - 1];
                            }
                            else
                            {
                                // At the last step, compare temporary plan list
                                // to best plan list to see if better

                                // Compare last step aggregates
                                if (actor.StepTrack.TempPlan.TotalValue > actor.StepTrack.BestPlan.TotalValue)
                                {
                                    // Iterate through all the steps to copy over
                                    // temp data to best plan data

                                    // DEBUG Start
                                    if (reaction)
                                    {
                                        Trace.WriteLine("Best action stored");
                                    }
                                    // DEBUG END

                                    foreach (StepData s in actor.StepList)
                                    {
                                        s.BestPlan.Step = s.TempPlan.Step;
                                        s.BestPlan.Action = s.TempPlan.Action;
                                        s.BestPlan.Value = s.TempPlan.Value;
                                        s.BestPlan.TotalValue = s.TempPlan.TotalValue;
                                        s.BestPlan.SelfValue = s.TempPlan.SelfValue;
                                        s.BestPlan.Member.Clear();
                                        foreach (var en in s.TempPlan.Member)
                                        {
                                            s.BestPlan.Member.Add(en);
                                        }
                                    }

                                    // Copy altTraitList info to reactTraitList
                                    // Will allow preaction entity to know what
                                    // alt traits to adjust.  Need to get from
                                    // the first step only.
                                    if (reaction)
                                    {
                                        this.reactTraitList.Clear();

                                        // Store best alt trait data in reactTraitList
                                        // Get from first reaction step
                                        StepData s = actor.StepList.First();
                                        foreach (AltTraitData alt2 in s.AltTraitList)
                                        {
                                            // Make new reactTraitList item
                                            AltTraitData anew = new AltTraitData();
                                            anew.Trait = alt.Trait;
                                            anew.Amount = alt.Amount;
                                            anew.AltAmount = alt.AltAmount;
                                            anew.ModAmount = alt.ModAmount;
                                            this.reactTraitList.Add(anew);
                                        }
                                    }
                                }
                            }

                            // Reverse alt trait amounts back to their
                            // original amounts if coming back from a
                            // step or reactions
                            foreach (AltTraitData alt2 in actor.StepTrack.AltTraitList)
                            {
                                alt2.Trait.Amount = alt.Amount;
                            }
                            actor.StepTrack.AltTraitList.Clear();
                        }


                        Stack<IEnumerator<EntityData>> revStack = new Stack<IEnumerator<EntityData>>();
                        while (eStack.Count>0)
                        {
                            IEnumerator<EntityData> tempEN = eStack.Last();
                            if (!tempEN.MoveNext())
                            {
                                revStack.Push(eStack.Pop());
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (eStack.Count == 0)
                        {
                            break;
                        }
                        // get enumerators reset and back onto stack
                        while (revStack.Count > 0)
                        {
                            eStack.Push(revStack.Pop());
                            eStack.Last().Reset();
                            eStack.Last().MoveNext();
                        }
                    }
                }
            }
        }

        public void StartActíon(Entity entity, Action action, params Entity[] mems)
        {
            List<Entity> members = new List<Entity>();

            // Insert entity as first member
            members.Insert(0, entity);

            members.AddRange(mems);

            List<EntityData> e = new List<EntityData>();

            // if no action specified, defaults to starting
            // the plan action
            if ((action == null) && (entity == null))
            {
                // If no action entered, default to the first
                // item on the plan list if available

                PlanData p = planList.FirstOrDefault();
                if (p != null)
                {
                    action = p.Action;
                    foreach (Entity m in members)
                    {
                        e.Add(FindEntity(m));
                    }
                }
                else
                {
                    // no plan to reference, so action needs
                    // to be specified
                    return;
                }
            }
            else
            {
                foreach (Entity m in members)
                {
                    EntityData ed = FindEntity(m, true);
                    if (ed != null)
                    {
                        e.Add(ed);
                    }
                    else
                    {
                        // exit function if can not locate member entity
                        return;
                    }
                }
            }


            ActionData a = FindAction(action, true);
            if (a != null)
            {
                // Store member entity trait information at the
                // start of the action.  Does not get stored as
                // an action record until the action has ended.
                RecordData r = e[LAI_ACTOR_MEMBER].StartRecord;

                // remove if there is a prev start record
                if (r != null)
                {
                    RemoveStartRecord(r);
                }
                r = AddStartRecord(e[LAI_ACTOR_MEMBER]);

                // need action stored here because not stored in record
                e[LAI_ACTOR_MEMBER].StartAction = action;

                for (int i = 0; i < action.MemberCount; i++)
                {
                    RecordMemberData m = AddRecordMember(r, i);
                    m.Entity = e[i].Entity;
                    m.Number = i;
                    foreach (TraitAmountData t in e[i].TraitAmountList)
                    {
                        // need to generate a recordTrait item for each
                        // entity trait that exists
                        RecordTraitData rt = AddRecordTrait(m);
                        rt.Trait = t.Trait;
                        rt.StartAmount = t.Amount;
                    }
                }
            }


        }

        private void RemoveStartRecord(RecordData r)
        {
            foreach (RecordMemberData m in r.RecordMemberList.ToArray())
            {
                RemoveRecordMember(r, m);
            }
        }

        public void EndAction(Entity entity)
        {
            if (entity == null)
            {
                entity = this;
            }
            ActionData a = null;



            EntityData e = FindEntity(entity);
            // Return of one of the items doesn't exist
            if (e != null)
            {
                a = FindAction(e.StartAction);
                if ((a == null) || (e.StartRecord == null))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            // Copy start record data to a permanent action record
            RecordData r = e.StartRecord;
            RecordData r2 = AddRecord(a);

            foreach (RecordMemberData m in r.RecordMemberList)
            {
                // Copy record member data from start record to action record
                RecordMemberData m2 = AddRecordMember(r2, m.Number);
                m2.Number = m.Number;
                m2.Entity = m.Entity;
                foreach (RecordTraitData rt in m.RecordTraitList)
                {
                    RecordTraitData rt2 = AddRecordTrait(m2);
                    rt2.Trait = rt.Trait;
                    rt2.StartAmount = rt.StartAmount;

                    // Record the trait end amounts and change
                    EntityData e2 = FindEntity(m2.Entity);
                    if (e2 != null)
                    {
                        TraitAmountData t = FindTraitAmount(e2, rt2.Trait);
                        if (t != null)
                        {
                            rt2.EndAmount = t.Amount;
                            rt2.Change = rt2.EndAmount - rt2.StartAmount;
                        }
                    }
                }
            }


            e.StartAction = null;
            RemoveStartRecord(r);
            e.StartRecord = null;

            UpdateActionLearn(a);
        }

        private void UpdateActionLearn(ActionData a)
        {
            int ct = 0;
            float sum = 0;
            float average = 0;
            float xsum = 0;
            float ysum = 0;
            float yysum = 0;
            float xysum = 0;
            float xxsum = 0;

            float[] y = new float[LAI_MAX_RECORD];
            float[] yy = new float[LAI_MAX_RECORD];
            float[] x = new float[LAI_MAX_RECORD];
            float[] xx = new float[LAI_MAX_RECORD];
            float[] xy = new float[LAI_MAX_RECORD];
            bool[] has_rec = new bool[2 * LAI_MAX_RECORD];
            foreach (MemberData m in a.MemberList)
            {
                foreach (Trait t in Trait.mainTraitList)
                {
                    sum = 0;
                    average = 0;
                    ct = 0;
                    foreach (RecordData r in a.RecordList)
                    {
                        // Match member with a record member
                        foreach (RecordMemberData rm in r.RecordMemberList)
                        {
                            if (rm.Number == m.Number)
                            {
                                RecordTraitData rt = FindRecordTrait(rm, t);
                                if (rt != null)
                                {
                                    sum += rt.Change;
                                }
                                // ct increments regardless of whether record member
                                // trait contains info or not because defaults to 0
                                ct++;
                                break;
                            }
                        }
                    }
                    // calculate average
                    if ((sum > 0) && (ct > 0))
                    {
                        average = sum / ct;
                    }

                    // See if a member trait exists for average
                    // If not, create one if the avg != 0
                    ModifyData mod = FindModify(m, t);
                    if (mod != null)
                    {
                        mod.Average = average;
                    }
                    else if (average > 0)
                    {
                        mod = AddModify(m, t);
                        mod.Average = average;
                    }
                }
            }

            if (a.RecordCount >= LAI_MIN_RECORD_CORRELATION)
            {
                // Calculate correlation data
                // Correlation-based learning of mod amounts in actions
                // X is the 2nd trait, Y is the mod amt of the 1st trait
                // Tries to establish correlation between the 2nd trait's
                // amt and how that impacts how much the first trait is changed.

                // 1. Figure out the full list of traits that need to check on
                // (every trait than an entity has a traitAmount for)
                // 2. Cycle through the members of the action, looking for all
                // traits and comparing to all traits.

                // Loop through member records
                foreach (MemberData m in a.MemberList)
                {
                    // Loop through main trait members
                    foreach (Trait t in Trait.mainTraitList)
                    {
                        for (int i = 0; i < LAI_MAX_RECORD; i++)
                        {
                            y[i] = 0;
                            yy[i] = 0;
                            has_rec[i] = false;
                        }

                        // Reset correlation data for the trait
                        // (delete the correlation list)
                        // rest hasCorrelation flag back to 0
                        ModifyData mod = FindModify(m, t);
                        if (mod != null)
                        {
                            mod.CorrelationList.Clear();
                        }

                        int recordCount = 0;
                        ct = 0;
                        // Cycle through records for first member/trait combination
                        foreach (RecordData r in a.RecordList)
                        {
                            // Collect Y data from record for the specified
                            // member and trait.  Match member with a record member
                            // Need here because can differ in each record
                            RecordMemberData rm = r.RecordMemberList.FirstOrDefault(z => z.Number == m.Number);
                            if (rm != null)
                            {
                                RecordTraitData rt = rm.RecordTraitList.FirstOrDefault(z => z.Trait == t);
                                if (rt != null)
                                {
                                    has_rec[recordCount] = true;
                                    ct++;

                                    // Get the Y values from recordlist
                                    // "n" is the record number
                                    // Don't want to calculate ysum and yysum yet
                                    // because not confirmed if there is a matching
                                    // 2nd trait yet
                                    y[recordCount] = rt.Change;
                                    yy[recordCount] = y[recordCount] * y[recordCount];
                                }
                            }
                            recordCount++;
                        }

                        // Go through records again to gather X data
                        // from other members/traits.  Cycle through
                        // records for first member/trait combination.
                        // Collect X data from other members and their
                        // various traits.  Only process if there was
                        // a record found for the original matching trait
                        if (ct > 0)
                        {
                            foreach (MemberData m2 in a.MemberList)
                            {
                                foreach (Trait t2 in Trait.mainTraitList)
                                {
                                    ysum = 0;
                                    yysum = 0;
                                    xsum = 0;
                                    xxsum = 0;
                                    xysum = 0;
                                    for (int i = 0; i < LAI_MAX_RECORD; i++)
                                    {
                                        x[i] = 0;
                                        xx[i] = 0;
                                        xy[i] = 0;
                                        // simulating 2dim array
                                        has_rec[LAI_MAX_RECORD + i] = false;
                                    }

                                    recordCount = 0;
                                    ct = 0;
                                    foreach (RecordData r in a.RecordList)
                                    {
                                        if (has_rec[recordCount])
                                        {
                                            // record member list
                                            RecordMemberData rm = r.RecordMemberList.FirstOrDefault(z => z.Number == m2.Number);
                                            if (rm != null)
                                            {
                                                RecordTraitData rt = rm.RecordTraitList.FirstOrDefault(z => z.Trait == t2);
                                                if (rt != null)
                                                {
                                                    has_rec[LAI_MAX_RECORD + recordCount] = true;

                                                    // paired trait combo in same record, so increase count
                                                    ct++;

                                                    // Can finally calculate ysum and yysum now
                                                    // that has a paired 2nd trait
                                                    ysum += y[recordCount];
                                                    yysum += yy[recordCount];

                                                    // The "X" data is gathered from the 2nd
                                                    // trait's start amount
                                                    x[recordCount] = rt.StartAmount;
                                                    xx[recordCount] = x[recordCount] * x[recordCount];
                                                    xy[recordCount] = x[recordCount] * y[recordCount];

                                                    xsum += x[recordCount];
                                                    xxsum += xx[recordCount];
                                                    xysum += xy[recordCount];

                                                }
                                            }

                                        }
                                        recordCount++;
                                    }

                                    if ((ct > 0) && (xsum != 0) && (ysum != 0))
                                    {
                                        float coef = 0;

                                        // Solve correlation coefficient
                                        float num = (ct * xysum) - (xsum * ysum);
                                        float deno1 = (ct * xxsum) - (xsum * xsum);
                                        float deno2 = (float)Math.Sqrt(deno1 * ((ct * yysum) - (ysum * ysum)));
                                        if (deno2 != 0)
                                        {
                                            coef = num / deno2;
                                        }

                                        // Generate a correlation record
                                        // if coefficient is strong enough
                                        if ((coef >= LAI_MIN_COEF) || (coef <= -LAI_MIN_COEF))
                                        {
                                            // see if modify exists for the trait, add one if there isn't
                                            ModifyData mod2 = FindModify(m, t, true);
                                            if (mod2 != null)
                                            {
                                                CorrelationData c = AddCorrelation(mod2);
                                                c.Coefficient = coef;
                                                c.Slope = num / deno1;
                                                c.Intercept = ((ysum * xxsum) - (xsum * xysum)) / deno1;
                                                c.Trait = t2;
                                                c.Member = m2.Number;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private RecordTraitData FindRecordTrait(RecordMemberData m, Trait trait, bool add = false)
        {
            foreach (RecordTraitData t in m.RecordTraitList)
            {
                if (t.Trait == trait)
                {
                    return t;
                }
            }
            if (add)
            {
                return AddRecordTrait(m);
            }
            return null;
        }
        public int GetCorrelationCount(Action action, int member, Trait trait)
        {
            ActionData a = FindAction(action);
            if (a!=null)
            {
                MemberData m = FindMember(a, member);
                if (m!=null)
                {
                    ModifyData mod = FindModify(m, trait);
                    if (mod!=null)
                    {
                        return mod.CorrelationCount;
                    }
                }
            }
            return 0;
        }
 
        public float GetCorrelation(Action action, int member, Trait trait, int correlationNumber=0, bool slope=false, bool intercept=false)
        {
            ActionData a = FindAction(action);
            if (a!=null)
            {
                MemberData m = FindMember(a, member);
                if (m!=null)
                {
                    ModifyData mod = FindModify(m, trait);
                    if (mod!=null)
                    {
                        CorrelationData c = FindCorrelation(mod, correlationNumber);
                        if (c!=null)
                        {
                            if (slope)
                            {
                                return c.Slope;
                            }
                            if (intercept)
                            {
                                return c.Intercept;
                            }
                            return c.Coefficient;
                        }
                    }
                }
            }
            return LAI_DEF_AMOUNT;
        }

        private CorrelationData FindCorrelation(ModifyData mod, int correlationNumber, bool add=false)
        {
            CorrelationData c = mod.CorrelationList.FirstOrDefault(x => correlationNumber == mod.CorrelationList.IndexOf(x));
            if (c!=null)
            {
                return c;
            }
            if (add)
            {
                return AddCorrelation(mod);
            }
            return null;
        }

        public int GetPlanCount()
        {
            PlanData p = planList.Last();
            if (p!=null)
            {
                return p.Step + 1;
            }
            return 0;
        }

        public int GetRecordCount(Action action)
        {
            ActionData a = FindAction(action);
            if (a!=null)
            {
                return a.RecordCount;
            }
            return 0;
        }
    }
}