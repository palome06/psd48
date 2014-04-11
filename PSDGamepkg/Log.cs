using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace PSD.PSDGamepkg
{
    public class Log
    {
        private string fileName;

        private Queue<string> queues;

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
            using (StreamWriter sw = new StreamWriter(fileName, true))
            {
                sw.WriteLine("VERSION={0} ISSV=1", version);
                sw.Flush();
            }
            queues = new Queue<string>();
            new Thread(delegate()
            {
                while (true)
                {
                    if (queues.Count > 0)
                    {
                        string line = queues.Dequeue();
                        using (StreamWriter sw = new StreamWriter(fileName, true))
                        {
                            sw.WriteLine(Base.LogES.DESEncrypt(line, "AKB48Show!",
                                (version * version).ToString()));
                            sw.Flush();
                        }
                    }
                    else
                        Thread.Sleep(300);
                }
            }).Start();
        }

        public void Logger(String line)
        {
            new Thread(delegate()
            {
                lock (queues)
                    queues.Enqueue(line);
            }).Start();
        }
    }
}
