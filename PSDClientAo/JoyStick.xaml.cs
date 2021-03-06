﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSD.ClientAo
{
    /// <summary>
    /// Interaction logic for JoyStick.xaml
    /// </summary>
    public partial class JoyStick : UserControl
    {
        public Base.LibGroup Tuple
        {
            set
            {
                CEE = new AoCEE(this, value)
                {
                    //Skill1 = "技能甲",
                    //Skill1Valid = true,
                    //Skill2 = "技能乙",
                    //Skill2Valid = true,
                    //Skill3 = "技能丙",
                    //Skill3Valid = false,
                    //ExtSkill1 = "辅助技能甲",
                    //ExtSkill1Valid = true,
                    //ExtSkill2 = ""
                    Skill1 = null,
                    Skill2 = null,
                    Skill3 = null,
                    Skill4 = null,
                    Skill5 = null,
                    Skill6 = null,
                    Skill7 = null,
                    ExtSkill1 = null,
                    ExtSkill2 = null,
                    ExtSkill3 = null,
                    ExtSkill4 = null
                };
                this.DataContext = CEE;
            }
        }

        public AoCEE CEE { get; set; }

        public JoyStick()
        {
            InitializeComponent();
        }

        public string DecideMessage { get; set; }

        private void DecideButtonClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("input?" + DecideMessage);
            if (input != null)
                input(DecideMessage ?? "");
        }
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("0");
        }
        private void PetButtonClick(object sender, RoutedEventArgs e)
        {
        }

        public event Util.InputMessageHandler input;

        private void CZ01ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ01");
        }
        private void CZ02ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ02");
        }
        private void CZ04ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ04");
        }
        private void CZ05ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("CZ05");
        }

        private void Skill1ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill1.Code);
        }
        private void Skill2ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill2.Code);
        }
        private void Skill3ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill3.Code);
        }
        private void Skill4ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill4.Code);
        }
        private void Skill5ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill5.Code);
        }
        private void Skill6ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill6.Code);
        }
        private void Skill7ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.Skill7.Code);
        }
        private void ExtSkill1ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill1.Code + "(" + CEE.ExtHolder1 + ")");
        }
        private void ExtSkill2ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill2.Code + "(" + CEE.ExtHolder2 + ")");
        }
        private void ExtSkill3ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill3.Code + "(" + CEE.ExtHolder3 + ")");
        }
        private void ExtSkill4ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill4.Code + "(" + CEE.ExtHolder4 + ")");
        }
        private void ExtSkill5ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill5.Code + "(" + CEE.ExtHolder5 + ")");
        }
        private void ExtSkill6ButtonClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input(CEE.ExtSkill6.Code + "(" + CEE.ExtHolder6 + ")");
        }

        private void SkOptChecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#1");
        }

        private void SkOptUnchecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#2");
        }

        private void TpOptChecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#3");
        }

        private void TpOptUnchecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#4");
        }

        private void MyOptChecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#5");
        }

        private void MyOptUnchecked(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("@#6");
        }
        // Force An input of Cancel(0) in case of fatal struck.
        private void SmoothForceClick(object sender, RoutedEventArgs e)
        {
            if (input != null)
                input("0");
        }
    }
}
