using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg
{
    internal class SkTriple
    {
        internal string Name { set; get; }
        internal int Priorty { set; get; }
        // Owner = 0, public:Tux; otherwise, private:Skill
        internal ushort Owner { set; get; }
        // Type, which occur trigger the action, starts with 0
        internal int InType { set; get; }
        // whether an effect needs equipment or not.
        internal bool NeedEquip { set; get; }
        // whether card is consume or not, false if skill
        internal bool IsConsume { set; get; }
        // lock skill / card-effect, use card then false
        internal bool Lock { set; get; }
        // whether is once or not
        internal bool IsOnce { get; set; }

        internal static int Cmp(SkTriple skt, SkTriple sku)
        {
            return skt.Priorty - sku.Priorty;
        }
    }

    internal class SKT
    {
        internal string Name { private set; get; }
        internal int Priorty { private set; get; }
        internal ushort Owner { private set; get; }
        internal int InType { private set; get; }
        internal bool NeedEqiup { private set; get; }
        internal bool IsConsume { set; get; }
        internal bool Lock { set; get; }
        internal bool IsOnce { get; set; }

        // card code to distinguish which card, 0 if skill
        //internal ushort CardCode { set; get; }
        // Fuse, which R/G trigger the action
        internal string Fuse { set; get; }        
        // Use Count
        internal int Tick { set; get; }

        internal SKT(SkTriple skt)
        {
            Name = skt.Name;
            Priorty = skt.Priorty;
            Owner = skt.Owner;
            InType = skt.InType;
            NeedEqiup = skt.NeedEquip;
            IsConsume = skt.IsConsume;
            Lock = skt.Lock;
            IsOnce = skt.IsOnce;

            Fuse = ""; Tick = 0;
        }

        internal static List<SKT> Generate(List<SkTriple> list)
        {
            List<SKT> result = new List<SKT>();
            foreach (SkTriple skt in list)
                result.Add(new SKT(skt));
            return result;
        }

        internal static SKT Find(string name, List<SKT> list)
        {
            foreach (SKT skt in list)
                if (skt.Name.Equals(name))
                    return skt;
            return null;
        }
    }
}
