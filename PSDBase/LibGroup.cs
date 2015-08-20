using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.Base
{
    public class LibGroup
    {
        public HeroLib HL { private set; get; }
        public TuxLib TL { private set; get; }
        public NPCLib NL { private set; get; }
        public MonsterLib ML { private set; get; }
        public EvenementLib EL { private set; get; }

        public SkillLib SL { private set; get; }
        public OperationLib ZL { private set; get; }
        public NCActionLib NJL { private set; get; }
        public RuneLib RL { private set; get; }

        public ExspLib ESL { private set; get; }

        public LibGroup(HeroLib hl, TuxLib tl, NPCLib nl, MonsterLib ml, EvenementLib el,
            SkillLib sl, OperationLib zl, NCActionLib njl, RuneLib rl, ExspLib esl)
        {
            HL = hl; TL = tl; NL = nl; ML = ml; EL = el;
            SL = sl; ZL = zl; NJL = njl; RL = rl; ESL = esl;
        }

        public LibGroup()
        {
            HL = new HeroLib();
            TL = new TuxLib();
            NL = new NPCLib();
            ML = new MonsterLib();
            EL = new EvenementLib();
            SL = new SkillLib();
            ZL = new OperationLib();
            NJL = new NCActionLib();
            RL = new RuneLib();
            ESL = new ExspLib();
        }
    }
}
