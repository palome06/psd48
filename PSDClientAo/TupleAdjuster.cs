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
            if (version <= 107)
            {
                Base.Card.Hero hero = tuple.HL.InstanceHero(17002);
                if (hero != null)
                    hero.ForceChange("HP", (ushort)7);
            }
            if (version <= 110)
            {
                Base.Skill skill = tuple.SL.EncodeSkill("JN50502");
                if (skill != null)
                    skill.ForceChange("Branches", AppendOnArray(skill.Branches, new Base.SKBranch()
                    {
                        Occur = "GOIY",
                        Priority = 110,
                        Lock = true,
                        Once = true,
                        Hind = false,
                        Serial = false,
                    }, 3));
            }
            if (version <= 114)
            {
                Base.Skill skill = tuple.SL.EncodeSkill("JN50402");
                if (skill != null)
                    skill.ForceChange("Branches", AppendOnArray(skill.Branches, new Base.SKBranch()
                    {
                        Occur = "GOIY",
                        Priority = 110,
                        Lock = true,
                        Once = true,
                        Hind = true,
                        Serial = false,
                    }, 2));
            }
            if (version <= 131)
            {
                Base.Card.Evenement eve = tuple.EL.GetEveFromName("SJ104");
                if (eve != null)
                    eve.ForceChange("Count", (ushort)1);
                tuple.EL.Refresh();
                Base.Skill skill = tuple.SL.EncodeSkill("JNH1102");
                if (skill != null)
                    skill.ForceChange("Branches", RemoveOnArray(skill.Branches, 2));
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
