using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace PSD.ClientAo
{
    public class AoCEE: INotifyPropertyChanged
    {
        public Base.LibGroup Tuple { set; get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private bool mcz01V, mcz02V, mcz03V, mcz05V;
        public bool CZ01Valid
        {
            set { if (mcz01V != value) { mcz01V = value; NotifyPropertyChanged("CZ01Valid"); } }
            get { return mcz01V; }
        }
        public bool CZ02Valid
        {
            set { if (mcz02V != value) { mcz02V = value; NotifyPropertyChanged("CZ02Valid"); } }
            get { return mcz02V; }
        }
        public bool CZ03Valid
        {
            set { if (mcz03V != value) { mcz03V = value; NotifyPropertyChanged("CZ03Valid"); } }
            get { return mcz03V; }
        }
        public bool CZ05Valid
        {
            set { if (mcz05V != value) { mcz05V = value; NotifyPropertyChanged("CZ05Valid"); } }
            get { return mcz05V; }
        }
        private bool msk1V, msk2V, msk3V, msk4V, msk5V, msk6V, msk7V, mesk1V, mesk2V, mesk3V, mesk4V;
        public bool Skill1Valid
        {
            set { if (msk1V != value) { msk1V = value; NotifyPropertyChanged("Skill1Valid"); } }
            get { return msk1V; }
        }
        public bool Skill2Valid
        {
            set { if (msk2V != value) { msk2V = value; NotifyPropertyChanged("Skill2Valid"); } }
            get { return msk2V; }
        }
        public bool Skill3Valid
        {
            set { if (msk3V != value) { msk3V = value; NotifyPropertyChanged("Skill3Valid"); } }
            get { return msk3V; }
        }
        public bool Skill4Valid
        {
            set { if (msk4V != value) { msk4V = value; NotifyPropertyChanged("Skill4Valid"); } }
            get { return msk4V; }
        }
        public bool Skill5Valid
        {
            set { if (msk5V != value) { msk5V = value; NotifyPropertyChanged("Skill5Valid"); } }
            get { return msk5V; }
        }
        public bool Skill6Valid
        {
            set { if (msk6V != value) { msk6V = value; NotifyPropertyChanged("Skill6Valid"); } }
            get { return msk6V; }
        }
        public bool Skill7Valid
        {
            set { if (msk7V != value) { msk7V = value; NotifyPropertyChanged("Skill7Valid"); } }
            get { return msk7V; }
        }
        public bool ExtSkill1Valid
        {
            set { if (mesk1V != value) { mesk1V = value; NotifyPropertyChanged("ExtSkill1Valid"); } }
            get { return mesk1V; }
        }
        public bool ExtSkill2Valid
        {
            set { if (mesk2V != value) { mesk2V = value; NotifyPropertyChanged("ExtSkill2Valid"); } }
            get { return mesk2V; }
        }
        public bool ExtSkill3Valid
        {
            set { if (mesk3V != value) { mesk3V = value; NotifyPropertyChanged("ExtSkill3Valid"); } }
            get { return mesk3V; }
        }
        public bool ExtSkill4Valid
        {
            set { if (mesk4V != value) { mesk4V = value; NotifyPropertyChanged("ExtSkill4Valid"); } }
            get { return mesk4V; }
        }

        private Base.Skill msk1, msk2, msk3, msk4, msk5, msk6, msk7;
        private Base.Bless mesk1, mesk2, mesk3, mesk4;
        public Base.Skill Skill1
        {
            set { if (msk1 != value) { msk1 = value; NotifyPropertyChanged("Skill1"); } }
            get { return msk1; }
        }
        public Base.Skill Skill2
        {
            set { if (msk2 != value) { msk2 = value; NotifyPropertyChanged("Skill2"); } }
            get { return msk2; }
        }
        public Base.Skill Skill3
        {
            set { if (msk3 != value) { msk3 = value; NotifyPropertyChanged("Skill3"); } }
            get { return msk3; }
        }
        public Base.Skill Skill4
        {
            set { if (msk4 != value) { msk4 = value; NotifyPropertyChanged("Skill4"); } }
            get { return msk4; }
        }
        public Base.Skill Skill5
        {
            set { if (msk5 != value) { msk5 = value; NotifyPropertyChanged("Skill5"); } }
            get { return msk5; }
        }
        public Base.Skill Skill6
        {
            set { if (msk6 != value) { msk6 = value; NotifyPropertyChanged("Skill6"); } }
            get { return msk6; }
        }
        public Base.Skill Skill7
        {
            set { if (msk7 != value) { msk7 = value; NotifyPropertyChanged("Skill7"); } }
            get { return msk7; }
        }
        public Base.Bless ExtSkill1
        {
            set { if (mesk1 != value) { mesk1 = value; NotifyPropertyChanged("ExtSkill1"); } }
            get { return mesk1; }
        }
        public Base.Bless ExtSkill2
        {
            set { if (mesk2 != value) { mesk2 = value; NotifyPropertyChanged("ExtSkill2"); } }
            get { return mesk2; }
        }
        public Base.Bless ExtSkill3
        {
            set { if (mesk3 != value) { mesk3 = value; NotifyPropertyChanged("ExtSkill3"); } }
            get { return mesk3; }
        }
        public Base.Bless ExtSkill4
        {
            set { if (mesk4 != value) { mesk4 = value; NotifyPropertyChanged("ExtSkill4"); } }
            get { return mesk4; }
        }
        public ushort ExtHolder1 { set; get; }
        public ushort ExtHolder2 { set; get; }
        public ushort ExtHolder3 { set; get; }
        public ushort ExtHolder4 { set; get; }

        public JoyStick js { set; get; }

        private int nextSKill, nextBKSkill;

        public AoCEE(JoyStick js, Base.LibGroup tuple)
        {
            this.js = js; this.Tuple = tuple;
            nextSKill = 0; nextBKSkill = 0;
        }

        public void ResetHightlight()
        {
            Skill1Valid = false;
            Skill2Valid = false;
            Skill3Valid = false;
            Skill4Valid = false;
            Skill5Valid = false;
            Skill6Valid = false;
            Skill7Valid = false;
            ExtSkill1Valid = false;
            ExtSkill2Valid = false;
            ExtSkill3Valid = false;
            ExtSkill4Valid = false;
            CZ01Valid = false;
            CZ02Valid = false;
            CZ03Valid = false;
            CZ05Valid = false;

            DecideValid = false;
            CancelValid = false;
            PetValid = false;
        }
        public void SetSkillHighlight(string skName, bool valid)
        {
            if (IsCodeEqual(skName, Skill1))
                Skill1Valid = valid;
            else if (IsCodeEqual(skName, Skill2))
                Skill2Valid = valid;
            else if (IsCodeEqual(skName, Skill3))
                Skill3Valid = valid;
            else if (IsCodeEqual(skName, Skill4))
                Skill4Valid = valid;
            else if (IsCodeEqual(skName, Skill5))
                Skill5Valid = valid;
            else if (IsCodeEqual(skName, Skill6))
                Skill6Valid = valid;
            else if (IsCodeEqual(skName, Skill7))
                Skill7Valid = valid;

            else if (IsCodeEqual(skName, ExtSkill1))
                ExtSkill1Valid = valid;
            else if (IsCodeEqual(skName, ExtSkill2))
                ExtSkill2Valid = valid;
            else if (IsCodeEqual(skName, ExtSkill3))
                ExtSkill3Valid = valid;
            else if (IsCodeEqual(skName, ExtSkill4))
                ExtSkill4Valid = valid;
        }
        public void SetCZHighlight(string czName, bool valid)
        {
            if (czName == "CZ01")
                CZ01Valid = valid;
            else if (czName == "CZ02")
                CZ02Valid = valid;
            else if (czName == "CZ03")
                CZ03Valid = valid;
            else if (czName == "CZ05")
                CZ05Valid = valid;
        }
        //public void SetSkillHighlight(IEnumerable<string> sks)
        //{
        //    Skill1Valid = false;
        //    Skill2Valid = false;
        //    Skill3Valid = false;
        //    ExtSkill1Valid = false;
        //    ExtSkill2Valid = false;
        //    foreach (string sk in sks)
        //    {
        //        if (IsCodeEqual(sk, Skill1))
        //            Skill1Valid = true;
        //        else if (IsCodeEqual(sk, Skill2))
        //            Skill2Valid = true;
        //        else if (IsCodeEqual(sk, Skill3))
        //            Skill3Valid = true;
        //        else if (IsCodeEqual(sk, ExtSkill1))
        //            ExtSkill1Valid = true;
        //        else if (IsCodeEqual(sk, ExtSkill2))
        //            ExtSkill2Valid = true;
        //    }
        //}
        private static bool IsCodeEqual(string skName, Base.Skill sk)
        {
            return sk != null && skName == sk.Code;
        }
        private static bool IsCodeEqual(string bsName, Base.Bless bs)
        {
            int idx = bsName.IndexOf('(');
            if (idx >= 0)
            {
                string skName = bsName.Substring(0, idx);
                return bs != null && skName == bs.Code;
            }
            else
                return false;
        }

        public bool SetNewSkill(Base.Skill skill)
        {
            if (nextSKill == 0) { Skill1 = skill; ++nextSKill; return true; }
            else if (nextSKill == 1) { Skill2 = skill; ++nextSKill; return true; }
            else if (nextSKill == 2) { Skill3 = skill; ++nextSKill; return true; }
            else if (nextSKill == 3) { Skill4 = skill; ++nextSKill; return true; }
            else if (nextSKill == 4) { Skill5 = skill; ++nextSKill; return true; }
            else if (nextSKill == 5) { Skill6 = skill; ++nextSKill; return true; }
            else if (nextSKill == 6) { Skill7 = skill; ++nextSKill; return true; }
            else return false;
        }
        public bool SetNewBKSkill(Base.Skill skill, ushort to)
        {
            Base.Bless bs = skill as Base.Bless;
            if (nextBKSkill == 0) { ExtSkill1 = bs; ExtHolder1 = to; ++nextBKSkill; return true; }
            else if (nextBKSkill == 1) { ExtSkill2 = bs; ExtHolder2 = to; ++nextBKSkill; return true; }
            else if (nextBKSkill == 2) { ExtSkill3 = bs; ExtHolder3 = to; ++nextBKSkill; return true; }
            else if (nextBKSkill == 3) { ExtSkill4 = bs; ExtHolder4 = to; ++nextBKSkill; return true; }
            else return false;
        }

        public void ResetSkill() {
            Skill1 = null; Skill2 = null;
            Skill3 = null; Skill4 = null;
            Skill5 = null; Skill6 = null;
            Skill7 = null;
            nextSKill = 0;
        }
        public void LoseSkill(string code)
        {
            Base.Skill[] skills = new Base.Skill[] { Skill1, Skill2, Skill3,
                Skill4, Skill5, Skill6, Skill7, null };
            Base.Skill[] hsk = new Base.Skill[7];
            int idx = 0; bool found = false;
            while (idx < skills.Length - 1) {
                if (!found) {
                    if (skills[idx] != null && skills[idx].Code == code)
                        found = true;
                    else
                        hsk[idx] = skills[idx];
                }
                if (found) {
                    hsk[idx] = skills[idx + 1];
                    if (hsk[idx] == null)
                        break;
                }
                ++idx;
            }
            Skill1 = hsk[0]; Skill2 = hsk[1]; Skill3 = hsk[2]; Skill4 = hsk[3];
            Skill5 = hsk[4]; Skill6 = hsk[5]; Skill7 = hsk[6];
            idx = 0;
            while (idx < 7) { if (hsk[idx] == null) break; ++idx; }
            nextSKill = idx;
        }
        public void ResetBKSkill()
        {
            ExtSkill1 = null; ExtSkill2 = null; ExtSkill3 = null; ExtSkill4 = null;
            nextBKSkill = 0;
        }
        public void LoseBKSkill(string code)
        {
            Base.Bless[] skills = new Base.Bless[] {
                 ExtSkill1, ExtSkill2, ExtSkill3, ExtSkill4, null };
            Base.Bless[] hsk = new Base.Bless[4];
            int idx = 0; bool found = false;
            while (idx < skills.Length - 1)
            {
                if (!found)
                {
                    if (skills[idx] != null && skills[idx].Code == code)
                        found = true;
                    else
                        hsk[idx] = skills[idx];
                }
                if (found)
                {
                    hsk[idx] = skills[idx + 1];
                    if (hsk[idx] == null)
                        break;
                }
                ++idx;
            }
            ExtSkill1 = hsk[0]; ExtSkill2 = hsk[1];
            ExtSkill3 = hsk[2]; ExtSkill4 = hsk[3];
            idx = 0;
            while (idx < 4) { if (hsk[idx] == null) break; ++idx; }
            nextBKSkill = idx;
        }
        //public void ResetBKSKill(string code)
        //{
        //    if (ExtSkill1 != null && ExtSkill1.Code == code)
        //    {
        //        if (ExtSkill2 == null)
        //        {
        //            ExtSkill1 = null; nextBKSkill = 0;
        //        }
        //        else
        //        {
        //            ExtSkill1 = ExtSkill2;
        //            ExtHolder1 = ExtHolder2;
        //            ExtSkill1Valid = ExtSkill2Valid;
        //            ExtSkill2 = null;
        //            nextBKSkill = 1;
        //        }
        //    }
        //    else if (ExtSkill2 != null && ExtSkill2.Code == code)
        //    {
        //        ExtSkill2 = null; nextBKSkill = 1;
        //    }
        //}

        #region Basic Section

        private bool mCancelValid;
        public bool CancelValid
        {
            set
            {
                if (mCancelValid != value)
                {
                    mCancelValid = value;
                    NotifyPropertyChanged("CancelValid");
                }
            }
            get { return mCancelValid; }
        }
        private bool mDecideValid;
        public bool DecideValid
        {
            set
            {
                if (mDecideValid != value)
                {
                    mDecideValid = value;
                    NotifyPropertyChanged("DecideValid");
                }
            }
            get { return mDecideValid; }
        }

        private bool mPetValid;
        public bool PetValid
        {
            set
            {
                if (mPetValid != value)
                {
                    mPetValid = value;
                    NotifyPropertyChanged("PetValid");
                }
            }
            get { return mPetValid; }
        }

        #endregion Basic Section

        #region Optimazation Section
        private bool mSkOptChecked;
        public bool SkOptChecked
        {
            set
            {
                if (mSkOptChecked != value)
                {
                    mSkOptChecked = value;
                    NotifyPropertyChanged("SkOptChecked");
                }
            }
            get { return mSkOptChecked; }
        }

        private bool mTpOptChecked;
        public bool TpOptChecked
        {
            set
            {
                if (mTpOptChecked != value)
                {
                    mTpOptChecked = value;
                    NotifyPropertyChanged("TpOptChecked");
                }
            }
        }
        #endregion Optimazation Section
    }
}
