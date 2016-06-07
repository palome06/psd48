using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace PSD.PSDGamepkg
{
    public class Log
    {
        private string fileName;

        private BlockingCollection<string> queue;
        // whether the writing to Log stops or not
        public bool Stop { set; get; }

        public void Start()
        {
            DateTime dt = System.DateTime.Now;
            bool exists = Directory.Exists("./log");
            if (!exists)
                Directory.CreateDirectory("./log");
            fileName = string.Format("./log/psd{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}.log",
                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            int version = ass.Version.Revision;

            queue = new BlockingCollection<string>(new ConcurrentQueue<string>());
            Task.Factory.StartNew(() =>
            {
                using (StreamWriter sw = new StreamWriter(fileName, true))
                {
                    sw.WriteLine("VERSION={0} ISSV=1", version);
                    sw.Flush();
                    Stop = false;
                    while (!Stop)
                    {
                        string line = queue.Take();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string eline = Base.LogES.DESEncrypt(line, "AKB48Show!",
                                (version * version).ToString());
                            sw.WriteLine(eline);
                            sw.Flush();
                        }
                    }
                }
            });
        }

        public void Logger(string line) { queue.Add(line); }
    }
}
