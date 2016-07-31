using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.Base.Card
{
    public interface NMB
    {
        bool IsMonster();

        bool IsNPC();

        string Name { get; }

        string Code { get; }

        int STR { get; }

        int AGL { get; }
    }

    public abstract class NMBLib
    {
        //public virtual bool IsDenger;

        public static bool IsMonster(ushort id)
        {
            return id > 0 && id < 1000;
        }

        public static bool IsNPC(ushort id)
        {
            return id > 1000 && id < 2000;
        }

        //public static bool IsDenger(ushort id)
        //{
        //    return id > 2000;
        //}

        public static NMB Decode(ushort id, MonsterLib ml, NPCLib nl)
        {
            if (id < 1000)
                return ml.Decode(id);
            else if (id > 1000 && id < 2000)
                return nl.Decode((ushort)(id - 1000));
            else
                return null;
        }
        public static ushort Encode(string code, MonsterLib ml, NPCLib nl)
        {
            if (code.StartsWith("G"))
                return CodeOfMonster(ml.Encode(code));
            else if (code.StartsWith("N"))
                return CodeOfNPC(nl.Encode(code));
            else
                return 0;
        }

        public static ushort CodeOfMonster(ushort id) { return id; }
        public static ushort OriginalMonster(ushort id) { return id; }
        public static ushort CodeOfNPC(ushort id) { return (ushort)(id + 1000); }
        public static ushort OriginalNPC(ushort id) { return (ushort)(id - 1000); }
        //public static ushort CodeOfDenger(ushort id)
        //{
        //    return (ushort)(id + 2000);
        //}
    }
}
