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

namespace MidPointUpdatingService
{
    public partial class MidPointUpdateService : ServiceBase
    {
        private string wsbaseurl = null;
        private string wsauthusr = null;
        private string wsauthpwd = null;
        private string cqueuefld = @"Midpoint.ADPassword.Queue";

        private static readonly HttpClient client = new HttpClient();
        private static IPersistentSecureQueue queue;

        private bool stopping = false;
       

        public MidPointUpdateService()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            ConfigureService();
            SetupClient();
            Task.Run(() => ProcessQueue());
        }

        protected void ProcessQueue()
        {
            while (!stopping)
            {
                try
                {
                    using (queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cqueuefld), TimeSpan.FromSeconds(30)))
                    {
                        ExecutionEngine.ProcessItem(client, queue);
                    }
                    Thread.Sleep(150);
                }
                catch
                { }
            }
        }

        protected override void OnStop()
        {
            if (!stopping)
                stopping = true;
        }

        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);
            if (command == (int)CustomCommnds.HeartBeat)
            {
                //TODO: Add the handler for heartbeating
            }
        }

        protected void SetupClient()
        {
            string cleanurl = wsbaseurl.EndsWith("/") ? wsbaseurl : wsbaseurl + '/'; // the url must end with '/'
            client.BaseAddress = new Uri(cleanurl);
            string authval = Convert.ToBase64String(Encoding.ASCII.GetBytes(wsauthusr + ":" + wsauthpwd)); // encode user/pass for basic auth
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authval);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }

        protected void ConfigureService()
        {
            try
            {
                wsbaseurl = ConfigurationManager.AppSettings["BASEURL"];
                wsauthusr = ConfigurationManager.AppSettings["AUTHUSR"];
                wsauthpwd = ConfigurationManager.AppSettings["AUTHPWD"];
                cqueuefld = ConfigurationManager.AppSettings["QUEUEFLD"];               
            }
            catch
            {
                Console.WriteLine("Missing parameters in config file");
            }
        }
    }

    public enum CustomCommnds
    {
        HeartBeat = 128
    }
}
