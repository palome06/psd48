using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.ClientAo.Card
{
    public class ShipRule
    {
        public enum AlignStyle { ALIGN, STAY };

        public class Zone
        {
            public int x1, x2, y1, y2;
            public AlignStyle style;
            public Zone(int x1, int x2, int y1, int y2, AlignStyle style)
            {
                this.x1 = x1; this.x2 = x2; this.y1 = y1; this.y2 = y2;
                this.style = style;
            }
        }

        public List<Zone> ZoneList { private set; get; }

        public ShipRule() { ZoneList = new List<Zone>(); }

        static ShipRule()
        {
            mDefSet = new ShipRule();
            mDefSet.ZoneList.Add(new Zone(0, 20, 0, 10, AlignStyle.ALIGN));
        }

        private static ShipRule mDefSet;
        public static ShipRule DEF_SETTING { get { return mDefSet; } }
    }
}
