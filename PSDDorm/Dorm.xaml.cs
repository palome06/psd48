using PSD.Base;
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
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            Title = ass.Name + " v" + ass.Version + " 录像控制台";
            CurrentMode = RunMode.NORMAL;

            if (File.Exists("PSDDorm.AKB48Show!"))
            {
                TransTab.Visibility = Visibility.Visible;
                mainAngleCheckBox.Checked += (s, e) => mainAngleNumber.IsEnabled = true;
                mainAngleCheckBox.Unchecked += (s, e) => mainAngleNumber.IsEnabled = false;
            }
            else
                TransTab.Visibility = Visibility.Collapsed;
        }

        private void FileDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            // Obtain the dragged file
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ListBox box = sender as ListBox;
            if (files == null || box == null)
                return;

            if (files.Length > 0 &&
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
                e.Effects = DragDropEffects.None;

            bool anySuccess = false, anyFailure = false;
            foreach (string file in files)
            {
                try
                {
                    string destFile = System.IO.Path.GetFullPath(file);
                    if (e.Effects == DragDropEffects.Copy && destFile.EndsWith(".psg"))
                    {
                        string title = GetTitleName(destFile);
                        box.Items.Add(new RoundItem()
                        {
                            Name = title,
                            Path = destFile
                        });
                        anySuccess = true;
                    }
                    else if (e.Effects == DragDropEffects.Copy && CurrentMode == RunMode.TRANSLATE && destFile.EndsWith(".log"))
                    {
                        box.Items.Add(new RoundItem()
                        {
                            Name = "log+" + System.IO.Path.GetFileName(file),
                            Path = destFile
                        });
                        anySuccess = true;
                    }
                    else
                        anyFailure = true;
                }
                catch
                {
                    anyFailure = true;
                }
            };
            if (anyFailure && anySuccess)
                MessageBox.Show("录像载入完毕，部分失败。");
            else if (anyFailure)
                MessageBox.Show("录像复盘载入失败。");
            else if (anySuccess)
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
            bool encrypted = true;
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = ushort.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    me = ushort.Parse(firsts[1].Substring("UID=".Length));
                if (firsts.Length > 2 && firsts[2] == "ENCRY=0")
                    encrypted = false;
            }
            while (iter.MoveNext())
            {
                string line = iter.Current;
                if (encrypted)
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

        private int GetTrueAUid(string fileName, int angel)
        {
            if (angel == 0)
                return 0;
            int version = 0;
            IEnumerator<string> iter = File.ReadLines(fileName).GetEnumerator();
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = ushort.Parse(firsts[0].Substring("VERSION=".Length));
            }
            while (iter.MoveNext())
            {
                string line = iter.Current;
                line = PSD.Base.LogES.DESDecrypt(line, "AKB48Show!",
                            (version * version).ToString());
                string[] firsts = line.Split(',');

                if (line.StartsWith("0>" + angel + ":H0SD") || line.StartsWith("0>" + angel + ";H0SD"))
                {
                    for (int i = 1; i < firsts.Length; i += 3)
                    {
                        ushort seat = ushort.Parse(firsts[i]);
                        ushort joinid = ushort.Parse(firsts[i + 1]);
                        if (seat == angel)
                            return joinid != 0 ? joinid : angel;
                    }
                }
            }
            return 0;
        }

        private void Encrypt(string path, string name, int angel, int angelUid)
        {
            if (!Directory.Exists("./mosh"))
                Directory.CreateDirectory("./mosh");
            string lName = "./mosh/" + name + (angelUid != 0 ? ("(" + angelUid + ").psg") : ".txt");

            var iter = System.IO.File.ReadLines(path).GetEnumerator();
            int version = 0, uid = 0; bool issv = false;
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = int.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    uid = ushort.Parse(firsts[1].Substring("UID=".Length));
                else if (firsts[1].StartsWith("ISSV="))
                    issv = true;
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(lName, true))
            {
                if (issv && angelUid != 0)
                    sw.WriteLine("VERSION={0} UID={1} ENCRY=0", version, angelUid);
                else if (issv)
                    sw.WriteLine("VERSION={0} ISSV=1", version);
                else
                    sw.WriteLine("VERSION={0} UID={1}", version, uid);
                sw.Flush();
                while (iter.MoveNext())
                {
                    string line = iter.Current;
                    if (version >= 99)
                    {
                        line = PSD.Base.LogES.DESDecrypt(line, "AKB48Show!",
                            (version * version).ToString());
                    }
                    if (issv && angelUid != 0)
                    {
                        if (line.StartsWith("0>" + angel + ":") || line.StartsWith("0>" + angel + ";"))
                        {
                            string content = line.Substring("0>?:".Length);
                            int idx = content.IndexOf(',');
                            string head = idx < 0 ? content : content.Substring(0, idx);
                            if (!h0Namelist.Contains(head) || h0SetOccur.Add(head))
                                sw.WriteLine("<" + line.Substring("0>?:".Length));
                        }
                    }
                    else
                        sw.WriteLine(line);
                }
            };
        }

        private ISet<string> h0SetOccur = new HashSet<string>();
        private static readonly ISet<string> h0Namelist =
            new HashSet<string>() { "H0SM", "H09N", "H09G", "H09P", "H09F" };

        private void Save(string path, string name)
        {
            if (!Directory.Exists("./repo"))
                Directory.CreateDirectory("./repo");
            string lName = "./repo/" + name + ".psg";

            IEnumerator<string> iter = File.ReadLines(path).GetEnumerator();
            int version = 0; ushort playerId = 0;
            bool encrypted = true;
            if (iter.MoveNext())
            {
                string firstLine = iter.Current;
                string[] firsts = firstLine.Split(' ');
                if (firsts[0].StartsWith("VERSION="))
                    version = ushort.Parse(firsts[0].Substring("VERSION=".Length));
                if (firsts[1].StartsWith("UID="))
                    playerId = ushort.Parse(firsts[1].Substring("UID=".Length));
                if (firsts.Length > 2 && firsts[2] == "ENCRY=0")
                    encrypted = false;
            }
            using (StreamWriter sw = new StreamWriter(lName, true))
            {
                sw.WriteLine("VERSION={0} UID={1}", version, playerId);
                sw.Flush();
                while (iter.MoveNext())
                {
                    string line = iter.Current;
                    if (encrypted)
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
            if (CurrentMode == RunMode.NORMAL)
            {
                foreach (var orgItem in orgListBox.SelectedItems)
                {
                    RoundItem item = orgItem as RoundItem;
                    if (item != null)
                        Save(item.Path, item.Name);
                }
                MessageBox.Show("录像转化完毕。");
            }
            else if (CurrentMode == RunMode.TRANSLATE)
            {
                int angel = 0;
                if (mainAngleCheckBox.IsChecked == true)
                    int.TryParse(mainAngleNumber.Text, out angel);
                foreach (var orgItem in transListBox.SelectedItems)
                {
                    RoundItem item = orgItem as RoundItem;
                    if (item != null)
                        Encrypt(item.Path, item.Name, angel, GetTrueAUid(item.Path, angel));
                }
                MessageBox.Show("录像翻译完毕。");
            }
        }

        private void ButtonResetClick(object sender, RoutedEventArgs e)
        {
            if (CurrentMode == RunMode.NORMAL)
                orgListBox.UnselectAll();
            if (CurrentMode == RunMode.TRANSLATE)
                transListBox.UnselectAll();
        }

        private void ButtonAllClick(object sender, RoutedEventArgs e)
        {
            if (CurrentMode == RunMode.NORMAL)
                orgListBox.SelectAll();
            if (CurrentMode == RunMode.TRANSLATE)
                transListBox.SelectAll();
        }

        public enum RunMode { NORMAL, TRANSLATE };

        private RunMode CurrentMode { set; get; }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = sender as TabControl;
            var selected = item.SelectedItem as TabItem;

            if (selected == NormalTab)
                CurrentMode = RunMode.NORMAL;
            else if (selected == TransTab)
                CurrentMode = RunMode.TRANSLATE;
        }
    }
}
