using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MQTTServer
{
    public enum LOGLEVEL
    {
        INFO = 0,
        ERROR,
        DEBUG,
        OFF,//不打日志
    }
    public class LogManager
    {
        private static object WriterLocker = new object();

        private static string logPath = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".nlog";

        public static string LogPath
        {
            get
            {
                string fullPath = Path.GetFullPath(logPath);
                string dirpath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dirpath))
                {
                    Directory.CreateDirectory(dirpath);
                }


                if (!File.Exists(logPath))
                {
                    var sw = File.Create(logPath);
                    sw.Close();
                }
                return logPath;
            }
            set
            {
                logPath = value + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                string fullPath = Path.GetFullPath(logPath);
                string dirpath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dirpath))
                {
                    Directory.CreateDirectory(dirpath);
                }

                if (!File.Exists(logPath))
                {
                    var sw = File.Create(logPath);
                    sw.Close();
                }
            }
        }

        public static void WriteLogEx(LOGLEVEL logLevel, string msg)
        {

            if (logLevel == LOGLEVEL.OFF)
                return;

            try
            {
                lock (WriterLocker)
                {
                    StreamWriter sw;
                    string path = LogPath;

                    if (File.Exists(path))
                    {
                        sw = File.AppendText(path);
                    }
                    else
                    {
                        sw = File.CreateText(path);
                    }

                    string stackinfo = GetStackInfo();
                    sw.WriteLine("[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "][" + logLevel.ToString() + "][" + stackinfo /*+ DateTime.Now.Millisecond.ToString()*/   + msg);
                    sw.Close();
                }

            }
            catch (Exception ex)
            {
                string aa = ex.Message;
            }
        }
        
        //获得正在执行的 类名 函数名
        public static string GetStackInfo()
        {
            string strStackInfo = string.Empty;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            StackFrame sf = st.GetFrame(2);   //向前偏移2个函数位
            MethodBase method = sf.GetMethod();
            string strFunctionName = method.Name;
            string strClassName = method.ReflectedType.FullName;
            strStackInfo = "[" + strClassName + "][" + strFunctionName + "]:";

            return strStackInfo;
        }
        
    }
}
