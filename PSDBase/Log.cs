using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

namespace PSD.Base
{
    public class Log
    {
        private string rName, lName;
        private Queue<string> rq, lq;
        //private List<string> recentList;

        // record literature results 
        private bool record;
        // record log in code
        private bool msglog;

        public void Start(int playerId, bool record, bool msglog, int nouse)
        {
            rq = new Queue<string>();
            lq = new Queue<string>();
            this.record = record; this.msglog = msglog;

            DateTime dt = System.DateTime.Now;
            if (record)
            {
                if (!Directory.Exists("./rec"))
                    Directory.CreateDirectory("./rec");
                rName = string.Format("./rec/逍遥游游戏记录{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).txt",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                new Thread(delegate()
                {
                    while (true)
                    {
                        string line = null;
                        lock (rq)
                        {
                            if (rq.Count > 0)
                                line = rq.Dequeue();
                        }
                        if (!string.IsNullOrEmpty(line))
                        {
                            using (StreamWriter sw = new StreamWriter(rName, true))
                            {
                                sw.WriteLine(line);
                                sw.Flush();
                            }
                        }
                        else
                            Thread.Sleep(300);
                    }
                }).Start();
            }
            if (msglog)
            {
                if (!Directory.Exists("./log"))
                    Directory.CreateDirectory("./log");
                lName = string.Format("./log/psd{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}({6}).psg",
                    dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, playerId);
                var ass = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                int version = ass.Version.Revision;
                using (StreamWriter sw = new StreamWriter(lName, true))
                {
                    sw.WriteLine("VERSION={0} UID={1}", version, playerId);
                    sw.Flush();
                }
                new Thread(delegate()
                {
                    while (true)
                    {
                        string line = null;
                        lock (lq)
                        {
                            if (lq.Count > 0)
                                line = lq.Dequeue();
                        }
                        if (!string.IsNullOrEmpty(line))
                        {
                            using (StreamWriter sw = new StreamWriter(lName, true))
                            {
                                sw.WriteLine(LogES.DESEncrypt(line, "AKB48Show!", 
                                    (version * version).ToString()));
                                //char[] chs = line.ToCharArray();
                                //sw.Write(chs.Length);
                                //sw.Write(chs);
                                sw.Flush();
                            }
                        }
                        else
                            Thread.Sleep(300);
                    }
                }).Start();
            }
        }

        public void Logg(string line)
        {
            if (msglog)
            {
                new Thread(delegate()
                {
                    lock (lq)
                        lq.Enqueue(line);
                }).Start();
            }
        }

        public void Record(string line)
        {
            if (record)
            {
                new Thread(delegate()
                {
                    lock (rq)
                        rq.Enqueue(line);
                }).Start();
            }
        }
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
            key += "12345678";
            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ict;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            sa = new DESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(IV);
            ict = sa.CreateEncryptor();

            byt = Encoding.UTF8.GetBytes(encryptStr);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ict, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            //加上一些干扰字符
            string retVal = Convert.ToBase64String(ms.ToArray());
            System.Random ra = new Random();

            for (int i = 0; i < 8; i++)
            {
                int radNum = ra.Next(36);
                char radChr = Convert.ToChar(radNum + 65);//生成一个随机字符

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
            key += "12345678";
            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ict;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            try
            {
                sa = new DESCryptoServiceProvider();
                sa.Key = Encoding.UTF8.GetBytes(key);
                sa.IV = Encoding.UTF8.GetBytes(IV);
                ict = sa.CreateDecryptor();

                byt = Convert.FromBase64String(encryptedValue);

                ms = new MemoryStream();
                cs = new CryptoStream(ms, ict, CryptoStreamMode.Write);
                cs.Write(byt, 0, byt.Length);
                cs.FlushFinalBlock();

                cs.Close();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (System.Exception)
            {
                return "";
            }

        }
    }
}
