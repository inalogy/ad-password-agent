using MidPointUpdatingService.Engine;
using SecureDiskQueue;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;

namespace MidPointUpdatingService
{
    public partial class MidPointUpdateService : ServiceBase
    {
        private string wsbaseurl = null;
        private string wsauthusr = null;
        private string wsauthpwd = null;
        private string cqueuefld = @"Midpoint.ADPassword.Queue";
        private int queuewait = 30;
        private int retrycnt = 50;
        private int loglevel = 0;
        private string logpath = @"Logs\";


        private static readonly HttpClient client = new HttpClient();
        private static IPersistentSecureQueue queue;
        private ILog log;
        private bool stopping = false;
        private Task processingTask;

        protected void SetupLogging()
        {
            try
            {
                AutoLog = true;

                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                PatternLayout patternLayout = new PatternLayout();
                patternLayout.ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
                patternLayout.ActivateOptions();

                RollingFileAppender roller = new RollingFileAppender();
                roller.AppendToFile = false;
                roller.File = logpath + @"EventLog.txt";
                roller.Layout = patternLayout;
                roller.MaxSizeRollBackups = 5;
                roller.MaximumFileSize = "1GB";
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.StaticLogFileName = true;
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);

                MemoryAppender memory = new MemoryAppender();
                memory.ActivateOptions();
                hierarchy.Root.AddAppender(memory);

                EventLogAppender evtLog = new EventLogAppender();
                evtLog.ApplicationName = "MidPoint Updating Service";
                evtLog.LogName = this.EventLog.Log;
                evtLog.ActivateOptions();

                switch (loglevel)
                {
                    case 0:
                        hierarchy.Root.Level = Level.Info;
                        break;
                    case 1:
                        hierarchy.Root.Level = Level.Debug;
                        break;
                    case 2:
                        hierarchy.Root.Level = Level.Warn;
                        break;
                    case 3:
                        hierarchy.Root.Level = Level.Error;
                        break;
                    case 4:
                        hierarchy.Root.Level = Level.Fatal;
                        break;
                    default:
                        hierarchy.Root.Level = Level.Info;
                        break;
                }
                hierarchy.Configured = true;

                log = LogManager.GetLogger("MidPoint Updating Service Logger");
            }
            catch (Exception ex)
            {
                throw new Exception("MidPoint Updating Service: Unable to setup logging", ex);
            }
        }

        protected void ConfigureService()
        {
            try
            {
                wsbaseurl = ConfigurationManager.AppSettings["BASEURL"];
                wsauthusr = ConfigurationManager.AppSettings["AUTHUSR"];
                wsauthpwd = ConfigurationManager.AppSettings["AUTHPWD"];
                cqueuefld = ConfigurationManager.AppSettings["QUEUEFLD"] ?? cqueuefld;
                int.TryParse(ConfigurationManager.AppSettings["QUEUEWAIT"], out queuewait);
                int.TryParse(ConfigurationManager.AppSettings["RETRYCNT"], out retrycnt);
                int.TryParse(ConfigurationManager.AppSettings["LOGLEVEL"], out loglevel);
                logpath = ConfigurationManager.AppSettings["LOGPATH"] ?? logpath;
                
            }
            catch (Exception ex)
            {                
                throw new Exception("MidPoint Updating Service: Missing parameters - invalid config file", ex);
            }
        }


        protected void SetupClient()
        {
            try
            {
                string cleanurl = wsbaseurl.EndsWith("/") ? wsbaseurl : wsbaseurl + '/'; // the url must end with '/'
                client.BaseAddress = new Uri(cleanurl);
                string authval = Convert.ToBase64String(Encoding.ASCII.GetBytes(wsauthusr + ":" + wsauthpwd)); // encode user/pass for basic auth
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authval);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            }
            catch (Exception ex)
            {
                throw new Exception("MidPoint Updating Service: Error setup HTTP Client", ex);
            }
        }


        public MidPointUpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ConfigureService();
            SetupLogging();
            SetupClient();
            processingTask = Task.Run(() => ProcessQueue());
            base.OnStart(args); 
        }
        protected override void OnStop()
        {
            if (!stopping)
                stopping = true;
            processingTask.Wait();
            LogManager.Flush(1000);
            LogManager.Shutdown();
            base.OnStop();
        }

        protected void ProcessQueue()
        {
            while (!stopping)
            {
                try
                {
                    using (queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cqueuefld), TimeSpan.FromSeconds(30)))
                    {
                        ExecutionEngine.ProcessItem(client, log, retrycnt, queue);
                    }
                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
            if (command == (int)CustomCommnds.HeartBeat)
            {
                //TODO: Add the handler for heartbeating
            }
        }
    }

    public enum CustomCommnds
    {
        HeartBeat = 128
    }
}
