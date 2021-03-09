using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QTSuperMarket
{
    class writeLog
    {
        static object locker = new object();
        public static void writeProgramLog(params string[] logs)
        {
            lock (locker)
            {
                string logAddress = Environment.CurrentDirectory + "\\Logs";
                if(!Directory.Exists(logAddress))
                {
                    Directory.CreateDirectory(logAddress);
                }
                logAddress = string.Concat(logAddress, '\\',DateTime.Now.ToLongDateString(),"使用记录.log");
                StreamWriter sw = new StreamWriter(logAddress, true);
                foreach (string log in logs)
                {
                    sw.WriteLine(string.Format("[{0}] {1}", DateTime.Now.ToString(), log));
                }
                sw.Close();
            }
        }
    }
}
