using MidPointUpdatingService.Engine;
using System;
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
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace MidPointUpdatingService
{
    public partial class MidPointUpdateService : ServiceBase
    {
        private string wsbaseurl = null;
        private string wsauthusr = null;
        private string wsauthpwd = null;
        private int ssl = 0;
        private string certName = null;
        private string cqueuefld = @"Midpoint.ADPassword.Queue";
        private int retrycnt = 50;
        private int loglevel = 0;
        private string logpath = @"Logs\";


        private static HttpClient client;
        private ILog log;
        private bool stopping = false;
        private Task enqueuingTask;
        private CancellationTokenSource cancellationToken;
        // private FileSystemWatcher fileSystemWatcher;

        protected void SetupLogging()
        {
            try
            {
                AutoLog = true;

                Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

                PatternLayout patternLayout = new PatternLayout
                {
                    ConversionPattern = "%date [%thread] %-5level %logger - %message %stacktracedetail{1}%newline"
                };
                patternLayout.ActivateOptions();

                RollingFileAppender roller = new RollingFileAppender
                {
                    AppendToFile = false,
                    File = string.Format("{0}MidpointUpdateService-{1}.log", logpath, DateTime.Today.ToString("yyyy-MM-dd")),
                    Layout = patternLayout,
                    MaxSizeRollBackups = 5,
                    MaximumFileSize = "1GB",
                    RollingStyle = RollingFileAppender.RollingMode.Size,
                    StaticLogFileName = false
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
                ssl = Convert.ToInt32(EnvironmentHelper.GetMidpointSsl());
                certName = EnvironmentHelper.GetServiceClientCertificateName() ?? certName;
                retrycnt = Convert.ToInt32(EnvironmentHelper.GetRetryCount());
                loglevel = Convert.ToInt32(EnvironmentHelper.GetMidpointServiceLogLevel());
                logpath = EnvironmentHelper.GetMidpointServiceLogPath() ?? logpath;
                if (!logpath.EndsWith("\\")) {
                    logpath = logpath + "\\";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("MidPoint Updating Service: Missing parameters - invalid configuration in Windows Registry {0}", EnvironmentHelper.loggingHive), ex);
            }
        }

        private static X509Certificate2 GetCertificateFromStore(string certName)
        {

            // Get the certificate store for the current user.
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Place all certificates in an X509Certificate2Collection object.
                X509Certificate2Collection certCollection = store.Certificates;
                // If using a certificate with a trusted root you do not need to FindByTimeValid, instead:
                // currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, true);
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certName, false);
                if (signingCert.Count == 0)
                    return null;
                // Return the first certificate in the collection, has the right name and is current.
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }

        protected void SetupClient()
        {
            try
            {
                if (ssl > 0)
                {
                    var handler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        SslProtocols = SslProtocols.Tls12,
                        CheckCertificateRevocationList = false
                    };
                    X509Certificate2 cert = null;
                    if (ssl > 1)
                    {
                        try
                        {
                            cert = new X509Certificate2(certName);
                        }
                        catch { cert = null; }
                    }
                    else
                    {
                        cert = GetCertificateFromStore(certName);
                    }
                    if (cert != null)
                    {
                        handler.ClientCertificates.Add(cert);
                        client = new HttpClient(handler);
                    }
                    else
                    {
                        if (ssl > 1)
                            throw new Exception(String.Format("MidPoint Updating Service: Error setup HTTPS Client - Certificate file .crt not found in the path {0}", certName));
                        else
                            throw new Exception(String.Format("MidPoint Updating Service: Error setup HTTPS Client - Certificate not found in the machine local store {0}", certName));
                    }
                }
                else client = new HttpClient();
                string cleanurl = wsbaseurl.EndsWith("/") ? wsbaseurl : wsbaseurl + '/'; // the url must end with '/'
                client.BaseAddress = new Uri(cleanurl);
                string authval = Convert.ToBase64String(Encoding.ASCII.GetBytes(wsauthusr + ":" + wsauthpwd)); // encode user/pass for basic auth
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authval);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            }
            catch (Exception ex)
            {
                throw new Exception("MidPoint Updating Service: Error setup HTTP/S Client", ex);
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
            enqueuingTask = Task.Run(() => EnqueueHeap(token), token);
            base.OnStart(args);
        }
        protected override void OnStop()
        {
            if (!stopping)
                stopping = true;
            cancellationToken.Cancel();
            enqueuingTask.Wait();
            LogManager.Flush(1000);
            LogManager.Shutdown();
            base.OnStop();
        }

        protected void EnqueueHeap(CancellationToken token)
        {

            while (!stopping)
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    if (!ExecutionEngine.EnqueueHeapItem(client, log, retrycnt, cqueuefld, token))
                    {
                        stopping = true;
                        break;
                    }

                    Thread.Sleep(125);

                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }
        }

    }
}