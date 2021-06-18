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
using Common;

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
        private ILog log;
        private bool stopping = false;
        private Task processingTask;
        private CancellationTokenSource cancellationToken;

        protected void SetupLogging()
        {
            try
            {
                AutoLog = true;

                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                PatternLayout patternLayout = new PatternLayout
                {
                    ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
                };
                patternLayout.ActivateOptions();

                RollingFileAppender roller = new RollingFileAppender
                {
                    AppendToFile = false,
                    File = logpath + @"EventLog.txt",
                    Layout = patternLayout,
                    MaxSizeRollBackups = 5,
                    MaximumFileSize = "1GB",
                    RollingStyle = RollingFileAppender.RollingMode.Size,
                    StaticLogFileName = true
                };
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);

                MemoryAppender memory = new MemoryAppender();
                memory.ActivateOptions();
                hierarchy.Root.AddAppender(memory);

                EventLogAppender evtLog = new EventLogAppender
                {
                    ApplicationName = "MidPoint Updating Service",
                    LogName = this.EventLog.Log,
                    Threshold = Level.Warn
                };
                evtLog.ActivateOptions();
                hierarchy.Root.AddAppender(evtLog);

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
                wsbaseurl = EnvironmentHelper.GetMidpointBaseUrl() ?? wsbaseurl;
                wsauthusr = EnvironmentHelper.GetMidpointAuthUser() ?? wsauthusr;
                wsauthpwd = EnvironmentHelper.GetMidpointAuthPwd() ?? wsauthpwd;
                cqueuefld = EnvironmentHelper.GetQueueFolder() ?? cqueuefld;
                queuewait = Convert.ToInt32(EnvironmentHelper.GetQueueWaitSeconds());
                retrycnt =  Convert.ToInt32(EnvironmentHelper.GetRetryCount());
                loglevel =  Convert.ToInt32(EnvironmentHelper.GetMidpointServiceLogLevel());
                logpath = EnvironmentHelper.GetMidpointServiceLogPath() ?? logpath;                
            }
            catch (Exception ex)
            {                
                throw new Exception(String.Format("MidPoint Updating Service: Missing parameters - invalid configuration in Windows Registry {0}", EnvironmentHelper.loggingHive), ex);
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
            cancellationToken = new CancellationTokenSource();
            var token = cancellationToken.Token;
            processingTask = Task.Run(() => ProcessQueue(token), token);
            base.OnStart(args); 
        }
        protected override void OnStop()
        {
            if (!stopping)
                stopping = true;
            cancellationToken.Cancel();
            processingTask.Wait();
            LogManager.Flush(1000);
            LogManager.Shutdown();
            base.OnStop();
        }

        protected void ProcessQueue(CancellationToken token)
        {
            while (!stopping)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    ExecutionEngine.ProcessItem(client, log, retrycnt, cqueuefld, queuewait, token);                 
                    Thread.Sleep(250);
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }
    }
}
