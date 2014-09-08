﻿using PSD.Base;
using PSD.Base.Card;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace PSDDorm
{
    public partial class Dorm : Window
    {
        private LibGroup Tuple { set; get; }

        private IDictionary<int, string> dict;

        class RoundItem
        {
            public string Path { set; get; }
            public string Name { set; get; }
            public override string ToString()
            {
                return Name;
            }
        }

        public Dorm()
        {
            InitializeComponent();
            Tuple = new LibGroup();
            dict = new Dictionary<int, string>();
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Obtain the dragged file
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null)
                return;

            if (files.Length > 0 && 
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
                e.Effects = DragDropEffects.None;

            foreach (string file in files)
            {
                try
                {
                    string destFile = System.IO.Path.GetFullPath(file);
                    if (e.Effects == DragDropEffects.Copy && destFile.EndsWith(".psg"))
                    {
                        string title = GetTitleName(destFile);
                        orgListBox.Items.Add(new RoundItem()
                        {
                            Name = title,
                            Path = destFile
                        });
                    }
                }
                catch
                {
                    MessageBox.Show("录像复盘载入失败。");
                }
            };
            MessageBox.Show("录像载入完毕。");
        }

        private string GetTitleName(string fileName)
        {
            ushort me = 0, uid = 0;
            int version = 0;
            int isWin = 0;
            string meHero = "";
            List<string> friends = new List<string>();
            List<string> enemies = new List<string>();
            IEnumerator<string> iter = File.ReadLines(fileName).GetEnumerator();
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = ushort.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    me = ushort.Parse(firsts[1].Substring("UID=".Length));
            }
            while (iter.MoveNext())
            {
                string line = iter.Current;
                line = PSD.Base.LogES.DESDecrypt(line, "AKB48Show!",
                            (version * version).ToString());
                string[] firsts = line.Split(',');

                if (line.StartsWith("<H0SD"))
                {
                    for (int i = 1; i < firsts.Length; i += 3)
                    {
                        ushort joinid = ushort.Parse(firsts[i + 1]);
                        if (me == joinid)
                            uid = ushort.Parse(firsts[i]);
                    }
                }
                if (line.StartsWith("<H0SL"))
                {
                    for (int i = 1; i < firsts.Length; i += 2)
                    {
                        ushort ut = ushort.Parse(firsts[i]);
                        int heroId = int.Parse(firsts[i + 1]);
                        if (ut == uid)
                        {
                            Hero hero = Tuple.HL.InstanceHero(heroId);
                            if (hero != null)
                                meHero = hero.Name;
                        }
                        else if (ut % 2 == uid % 2)
                        {
                            Hero hero = Tuple.HL.InstanceHero(heroId);
                            if (hero != null)
                                friends.Add(hero.Name);
                        }
                        else
                        {
                            Hero hero = Tuple.HL.InstanceHero(heroId);
                            if (hero != null)
                                enemies.Add(hero.Name);
                        }
                    }
                }
                if (line.StartsWith("<F0WN"))
                {
                    ushort side = ushort.Parse(firsts[1]);
                    if (uid % 2 == side % 2)
                        isWin = 1;
                    else
                        isWin = -1;
                }
            }
            string result = "-VS-";
            if (isWin == 1)
                result = "-胜-";
            else if (isWin == -1)
                result = "-负-";
            string filePureName = fileName.Contains("\\") ?
                fileName.Substring(fileName.LastIndexOf('\\') + 1) : fileName;
            if (filePureName.StartsWith("psd"))
                filePureName = filePureName.Substring("psd".Length);
            if (filePureName.LastIndexOf('(') > 0)
                filePureName = filePureName.Substring(0, filePureName.LastIndexOf('('));
            return string.Format("[{0}]{1}{2}{3}-{4}", meHero, string.Join("", friends),
                result, string.Join("", enemies), filePureName);
        }

        private void Save(string path, string name)
        {
            if (!Directory.Exists("./repo"))
                Directory.CreateDirectory("./repo");
            string lName = "./repo/" + name + ".psg";

            IEnumerator<string> iter = File.ReadLines(path).GetEnumerator();
            int version = 0; ushort playerId = 0;
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = ushort.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    playerId = ushort.Parse(firsts[1].Substring("UID=".Length));
            }
            using (StreamWriter sw = new StreamWriter(lName, true))
            {
                sw.WriteLine("VERSION={0} UID={1}", version, playerId);
                sw.Flush();
                while (iter.MoveNext())
                {
                    string line = iter.Current;
                    line = PSD.Base.LogES.DESDecrypt(line, "AKB48Show!",
                                (version * version).ToString());

                    if (line.StartsWith("<H0SD"))
                    {
                        string[] firsts = line.Split(',');
                        for (int i = 1; i < firsts.Length; i += 3)
                            firsts[i + 2] = "大侠";
                        line = string.Join(",", firsts);
                    }
                    sw.WriteLine(LogES.DESEncrypt(line, "AKB48Show!",
                                   (version * version).ToString()));
                    sw.Flush();
                }
            }
        }

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            if (orgListBox.SelectedItems.Count > 0)
            {
                foreach (var orgItem in orgListBox.SelectedItems)
                {
                    RoundItem item = orgItem as RoundItem;
                    if (item != null)
                        Save(item.Path, item.Name);
                }
                MessageBox.Show("录像转化完毕。");
            }
        }

        private void ButtonResetClick(object sender, RoutedEventArgs e)
        {
            orgListBox.UnselectAll();
        }

        private void ButtonAllClick(object sender, RoutedEventArgs e)
        {
            orgListBox.SelectAll();
        }
    }
}
