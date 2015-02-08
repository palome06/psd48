using PSD.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.ClientAo
{
    public class TupleAdjuster
    {
        public void ConvertTuple(LibGroup tuple, int version)
        {
            if (version <= 131)
            {
                Base.Card.Evenement eve = tuple.EL.GetEveFromName("SJ104");
                if (eve != null)
                    eve.ForceChange("Count", 1);
                tuple.EL.Refresh();
                Base.Skill skill = tuple.SL.EncodeSkill("JNH1102");
                if (skill != null)
                {
                    skill.ForceChange("Occurs", RemoveOnArray(skill.Occurs, 2));
                    skill.ForceChange("Priorities", RemoveOnArray(skill.Priorities, 2));
                    skill.ForceChange("IsOnce", RemoveOnArray(skill.IsOnce, 2));
                    skill.ForceChange("IsTermini", RemoveOnArray(skill.IsTermini, 2));
                    skill.ForceChange("Lock", RemoveOnArray(skill.Lock, 2));
                    skill.ForceChange("IsHind", RemoveOnArray(skill.IsHind, 2));
                }
            }
            if (version <= 114)
            {
                Base.Skill skill = tuple.SL.EncodeSkill("JN50402");
                if (skill != null)
                {
                    skill.ForceChange("Occurs", AppendOnArray(skill.Occurs, "G0IY", 2));
                    skill.ForceChange("Priorities", AppendOnArray(skill.Priorities, 110, 2));
                    skill.ForceChange("IsOnce", AppendOnArray(skill.IsOnce, true, 2));
                    skill.ForceChange("IsTermini", AppendOnArray(skill.IsTermini, false, 2));
                    skill.ForceChange("Lock", AppendOnArray(skill.Lock, true, 2));
                    skill.ForceChange("IsHind", AppendOnArray(skill.IsHind, true, 2));
                }
            }
            if (version <= 110)
            {
                Base.Skill skill = tuple.SL.EncodeSkill("JN50502");
                if (skill != null)
                {
                    skill.ForceChange("Occurs", AppendOnArray(skill.Occurs, "G0IY", 3));
                    skill.ForceChange("Priorities", AppendOnArray(skill.Priorities, 110, 3));
                    skill.ForceChange("IsOnce", AppendOnArray(skill.IsOnce, true, 3));
                    skill.ForceChange("IsTermini", AppendOnArray(skill.IsTermini, false, 3));
                    skill.ForceChange("Lock", AppendOnArray(skill.Lock, true, 3));
                    skill.ForceChange("IsHind", AppendOnArray(skill.IsHind, false, 3));
                }
            }
            if (version <= 107)
            {
                Base.Card.Hero hero = tuple.HL.InstanceHero(17002);
                if (hero != null)
                    hero.ForceChange("HP", (ushort)7);
            }
        }

        private Type[] AppendOnArray<Type>(Type[] array, Type item, int index)
        {
            List<Type> list = array.ToList();
            list.Insert(index, item);
            return list.ToArray();
        }

        private Type[] RemoveOnArray<Type>(Type[] array, int index)
        {
            List<Type> list = array.ToList();
            list.RemoveAt(index);
            return list.ToArray();
        }
    }
}
