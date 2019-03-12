using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public sealed class Logger : IDisposable
    {
        private static Logger instance = null;
        private static readonly object padlock = new object();
        private static string path;
        private static string currentTime;
        private static bool run = true;
        private string logFileName = "verbose-";
        private string debugFileName = "debug-";
        private string errorFileName = "error-";
        private static ConcurrentQueue<string> logQueue;
        private static StreamWriter vsw; //verbose
        private static StreamWriter dsw; //debug
        private static StreamWriter esw; //error

        private const string LOG_FOLDERNAME = "LOGS";

        private Logger()
        {
            logQueue = new ConcurrentQueue<string>();
            currentTime = DateTime.Now.ToString("dd-MM-yyyy HHmm");
            Thread logout =new Thread(new ThreadStart(LogToFile));
            logout.Start();
            //File name with time
            logFileName += currentTime + ".txt";
            debugFileName += currentTime + ".txt";
            errorFileName += currentTime + ".txt";

            //Current Directory
            path = Directory.GetCurrentDirectory();

            //Create Folder for Logs if it doesn't exist
            path = System.IO.Path.Combine(path, LOG_FOLDERNAME);
            System.IO.Directory.CreateDirectory(path);
            

            //Complete filepath for file creation
            logFileName = System.IO.Path.Combine(path, logFileName);
            debugFileName = System.IO.Path.Combine(path, debugFileName);
            errorFileName = System.IO.Path.Combine(path, errorFileName);


            //Create File and open the streamwriter
            CreateFile(logFileName);
            CreateFile(debugFileName);
            CreateFile(errorFileName);

            //Open streamwriter for use.
            vsw = new StreamWriter(logFileName);
            dsw = new StreamWriter(debugFileName);
            esw = new StreamWriter(errorFileName);
        }

        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        public void V(string vString)
        {
            currentTime = DateTime.Now.ToString("hh:mm:ss.ffff");
            string log = "V " + Thread.CurrentThread.ManagedThreadId.ToString() + " " + currentTime + " :" + vString;
            logQueue.Enqueue(log);

        }

        public void D(string dString)
        {
            currentTime = DateTime.Now.ToString("hh:mm:ss.ffff");
            string log = "D " + Thread.CurrentThread.ManagedThreadId.ToString() + " " + currentTime + " :" + dString;
            logQueue.Enqueue(log);

        }

        public void E(string eString)
        {
            currentTime = DateTime.Now.ToString("hh:mm:ss.ffff");
            string log = "E " + Thread.CurrentThread.ManagedThreadId.ToString() + " " + currentTime + " :" + eString;
            logQueue.Enqueue(log);

        }


        private void CreateFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(filePath))
                {
                }
            }
        }

        public void LogToFile()
        {
            string log;
            while (run)
            {
                if (logQueue.TryDequeue(out log))
                {
                    switch (log[0])
                    {
                        case 'E':
                            esw.WriteLine(log);
                            esw.Flush();
                            goto case 'D';
                        case 'D':
                            dsw.WriteLine(log);
                            dsw.Flush();
                            goto case 'V';
                        case 'V':
                            vsw.WriteLine(log);
                            vsw.Flush();
                            break;
                    }
                }
            }
        }

        public void Dispose()
        {
            run = false;
            if (vsw != null)
                vsw.Dispose();
            if (dsw != null)
                dsw.Dispose();
            if (esw != null)
                esw.Dispose();
        }
    }
}