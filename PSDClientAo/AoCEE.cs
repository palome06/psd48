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
        private bool msk1V, msk2V, msk3V, msk4V, mesk1V, mesk2V;
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

        private Base.Skill msk1, msk2, msk3, msk4;
        private Base.Bless mesk1, mesk2;
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
        public ushort ExtHolder1 { set; get; }
        public ushort ExtHolder2 { set; get; }

        //private string msk1, msk2, msk3, mesk1, mesk2;
        //public string Skill1
        //{
        //    set { if (msk1 != value) { msk1 = value; NotifyPropertyChanged("Skill1"); } }
        //    get { return msk1; }
        //}
        //public string Skill2
        //{
        //    set { if (msk2 != value) { msk2 = value; NotifyPropertyChanged("Skill2"); } }
        //    get { return msk2; }
        //}
        //public string Skill3
        //{
        //    set { if (msk3 != value) { msk3 = value; NotifyPropertyChanged("Skill3"); } }
        //    get { return msk3; }
        //}
        //public string ExtSkill1
        //{
        //    set { if (mesk1 != value) { mesk1 = value; NotifyPropertyChanged("ExtSkill1"); } }
        //    get { return mesk1; }
        //}
        //public string ExtSkill2
        //{
        //    set { if (mesk2 != value) { mesk2 = value; NotifyPropertyChanged("ExtSkill2"); } }
        //    get { return mesk2; }
        //}

        //private string msk1T, msk2T, msk3T, mesk1T, mesk2T;
        //public string Skill1Text
        //{
        //    set { if (msk1T != value) { msk1T = value; NotifyPropertyChanged("Skill1Text"); } }
        //    get { return msk1T; }
        //}
        //public string Skill2Text
        //{
        //    set { if (msk2T != value) { msk2T = value; NotifyPropertyChanged("Skill2Text"); } }
        //    get { return msk2T; }
        //}
        //public string Skill3Text
        //{
        //    set { if (msk3T != value) { msk3T = value; NotifyPropertyChanged("Skill3Text"); } }
        //    get { return msk3T; }
        //}
        //public string ExtSkill1Text
        //{
        //    set { if (mesk1T != value) { mesk1T = value; NotifyPropertyChanged("ExtSkill1Text"); } }
        //    get { return mesk1T; }
        //}
        //public string ExtSkill2Text
        //{
        //    set { if (mesk2T != value) { mesk2T = value; NotifyPropertyChanged("ExtSkill2Text"); } }
        //    get { return mesk2T; }
        //}

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
            ExtSkill1Valid = false;
            ExtSkill2Valid = false;
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

            else if (IsCodeEqual(skName, ExtSkill1))
                ExtSkill1Valid = valid;
            else if (IsCodeEqual(skName, ExtSkill2))
                ExtSkill2Valid = valid;
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
            string skName = bsName.Substring(0, bsName.IndexOf('('));
            return bs != null && skName == bs.Code;
        }

        //public void SetSkillText(int index, string code, string text)
        //{
        //    if (index == 0) { Skill1 = code; Skill1Text = text; }
        //    else if (index == 1) { Skill2 = code; Skill2Text = text; }
        //    else if (index == 2) { Skill3 = code; Skill3Text = text; }
        //    else if (index == 10) { ExtSkill1 = code; ExtSkill1Text = text; }
        //    else if (index == 11) { ExtSkill2 = code; ExtSkill2Text = text; }
        //}

        //public bool SetNewSkill(string code, string text)
        //{
        //    if (nextSKill == 0) { Skill1 = code; Skill1Text = text; ++nextSKill; return true; }
        //    else if (nextSKill == 1) { Skill2 = code; Skill2Text = text; ++nextSKill; return true; }
        //    else if (nextSKill == 2) { Skill3 = code; Skill3Text = text; ++nextSKill; return true; }
        //    else return false;
        //}
        //public bool SetNewBKSkill(string code, string text)
        //{
        //    if (nextBKSkill == 0) { ExtSkill1 = code; ExtSkill1Text = text; ++nextBKSkill; return true; }
        //    else if (nextBKSkill == 1) { ExtSkill2 = code; ExtSkill2Text = text; ++nextBKSkill; return true; }
        //    else return false;
        //}

        //public void ResetSkill() { Skill1 = ""; Skill2 = ""; Skill3 = ""; nextSKill = 0; }
        //public void ResetBKSKill(string code)
        //{
        //    if (ExtSkill1 == code)
        //    {
        //        if (string.IsNullOrEmpty(ExtSkill2))
        //        {
        //            ExtSkill1 = "";
        //            nextBKSkill = 0;
        //        }
        //        else
        //        {
        //            ExtSkill1 = ExtSkill2;
        //            ExtSkill1Text = ExtSkill2Text;
        //            ExtSkill1Valid = ExtSkill2Valid;
        //            ExtSkill2 = "";
        //            nextBKSkill = 1;
        //        }
        //    }
        //    else if (ExtSkill2 == code)
        //    {
        //        ExtSkill2 = "";
        //        nextBKSkill = 1;
        //    }
        //}
        public bool SetNewSkill(Base.Skill skill)
        {
            if (nextSKill == 0) { Skill1 = skill; ++nextSKill; return true; }
            else if (nextSKill == 1) { Skill2 = skill; ++nextSKill; return true; }
            else if (nextSKill == 2) { Skill3 = skill; ++nextSKill; return true; }
            else if (nextSKill == 3) { Skill4 = skill; ++nextSKill; return true; }
            else return false;
        }
        public bool SetNewBKSkill(Base.Skill skill, ushort to)
        {
            Base.Bless bs = skill as Base.Bless;
            if (nextBKSkill == 0) { ExtSkill1 = bs; ExtHolder1 = to; ++nextBKSkill; return true; }
            else if (nextBKSkill == 1) { ExtSkill2 = bs; ExtHolder2 = to; ++nextBKSkill; return true; }
            else return false;
        }

        public void ResetSkill() {
            Skill1 = null; Skill2 = null;
            Skill3 = null; Skill4 = null;
            nextSKill = 0;
        }

        public void ResetBKSKill(string code)
        {
            if (ExtSkill1.Code == code)
            {
                if (ExtSkill2 == null)
                {
                    ExtSkill1 = null; nextBKSkill = 0;
                }
                else
                {
                    ExtSkill1 = ExtSkill2;
                    ExtHolder1 = ExtHolder2;
                    ExtSkill1Valid = ExtSkill2Valid;
                    ExtSkill2 = null;
                    nextBKSkill = 1;
                }
            }
            else if (ExtSkill2.Code == code)
            {
                ExtSkill2 = null; nextBKSkill = 1;
            }
        }

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
