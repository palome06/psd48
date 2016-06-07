using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PSD.Base
{
    // Client log
    public class ClLog
    {
        private string rName, lName;
        //private Queue<string> rq, lq;
        private BlockingCollection<string> rq, lq;
        //private List<string> recentList;

        // record literature results 
        private bool record;
        // record log in code
        private bool msglog;
        // whether the writing to Log stops or not
        public bool Stop { set; get; }

        public void Start(int playerId, bool record, bool msglog, int nouse)
        {
            rq = new BlockingCollection<string>(new ConcurrentQueue<string>());
            lq = new BlockingCollection<string>(new ConcurrentQueue<string>());
            //rq = new Queue<string>();
            //lq = new Queue<string>();
            this.record = record; this.msglog = msglog;
            Stop = false;

            DateTime dt = System.DateTime.Now;
            if (record)
            {
                if (!Directory.Exists("./rec"))
                    Directory.CreateDirectory("./rec");
                rName = string.Format("./rec/逍遥游游戏记录{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).txt",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                Task.Factory.StartNew(() =>
                {
                    using (StreamWriter sw = new StreamWriter(rName, true))
                    {
                        while (!Stop)
                        {
                            string line = rq.Take();
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(line);
                                sw.Flush();
                            }
                        }
                    }
                });
            }
            if (msglog)
            {
                if (!Directory.Exists("./log"))
                    Directory.CreateDirectory("./log");
                lName = string.Format("./log/psd{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).psg",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                int version = ass.Version.Revision;

                Task.Factory.StartNew(() =>
                {
                    using (StreamWriter sw = new StreamWriter(lName, true))
                    {
                        sw.WriteLine("VERSION={0} UID={1}", version, playerId);
                        sw.Flush();
                        while (!Stop)
                        {
                            string line = lq.Take();
                            if (!string.IsNullOrEmpty(line))
                            {
                                sw.WriteLine(LogES.DESEncrypt(line, "AKB48Show!",
                                    (version * version).ToString()));
                                //char[] chs = line.ToCharArray();
                                //sw.Write(chs.Length);
                                //sw.Write(chs);
                                sw.Flush();
                            }
                        }
                    }
                });
            }
        }

        public void Logg(string line) { if (msglog) lq.Add(line); }

        public void Record(string line) { if (record) rq.Add(line); }
    }

    public class LogES
    {
        /// <summary>
        /// Encrypt string with DES
        /// </summary>
        /// <param name="encryptedStr">plaintext</param>
        /// <param name="key">key(length 8 at most)</param>
        /// <param name="IV">initial vector(length 8 at most)</param>
        /// <returns>ciphertext</returns>
        public static string DESEncrypt(string encryptStr, string key, string IV)
        {
            // Set it to length of 8
            key = (key + "12345678").Substring(0, 8);
            IV = (IV + "12345678").Substring(0, 8);

            SymmetricAlgorithm sa = new DESCryptoServiceProvider()
            {
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes(IV)
            };
            ICryptoTransform ict = sa.CreateEncryptor();
            byte[] byt = Encoding.UTF8.GetBytes(encryptStr);

            string retVal = "";
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();        
                }
                retVal = Convert.ToBase64String(ms.ToArray());
            }

            // do some confusion
            System.Random ra = new Random();
            for (int i = 0; i < 8; i++)
            {
                int radNum = ra.Next(36);
                char radChr = Convert.ToChar(radNum + 65);// get a random character

                retVal = retVal.Substring(0, 2 * i + 1) + radChr.ToString() + retVal.Substring(2 * i + 1);
            }

            return retVal;
        }

        /// <summary>
        /// Decrypt string with DES
        /// </summary>
        /// <param name="encryptedValue">ciphertext</param>
        /// <param name="key">key(length 8 at most)</param>
        /// <param name="IV">initial vector(length 8 at most)</param>
        /// <returns>plaintext</returns>
        public static string DESDecrypt(string encryptedValue, string key, string IV)
        {
            // remove disturbs
            string tmp = encryptedValue;
            if (tmp.Length < 16)
            {
                return "";
            }

            for (int i = 0; i < 8; i++)
            {
                tmp = tmp.Substring(0, i + 1) + tmp.Substring(i + 2);
            }
            encryptedValue = tmp;

            // Set it to length of 8
            key = (key + "12345678").Substring(0, 8);
            IV = (IV + "12345678").Substring(0, 8);

            try
            {
                SymmetricAlgorithm sa = new DESCryptoServiceProvider()
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = Encoding.UTF8.GetBytes(IV)
                };
                ICryptoTransform ict = sa.CreateDecryptor();

                byte[] byt = Convert.FromBase64String(encryptedValue);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                    {
                        cs.Write(byt, 0, byt.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception) { return ""; }
        }
    }
}
