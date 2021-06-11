using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using DiskQueue;
using ADPasswordSecureCache;
using ADPasswordSecureCache.Policies;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MidPointUpdatingService.Models;
using System.Net.Http.Headers;
using System.Net.Http;

namespace MidPointUpdatingService
{
    public partial class MidPointUpdateService : ServiceBase
    {
        private const double cacheDurationMins = 30;

        private string wsbaseurl = null;
        private string wsauthusr = null;
        private string wsauthpwd = null;
        private string ccachefld = @"Midpoint.ADPassword.Cache";
        private string cqueuefld = @"Midpoint.ADPassword.Queue";
        private double ccacheduration = cacheDurationMins; // default cache duration in minutes

        private static HttpClient client = new HttpClient();
        private static DiskCache<string> diskCacheInstance;
        private static PersistentQueue queue;
       

        public MidPointUpdateService()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            ConfigureService();
            SetupQueue();
            SetupCache();
            SetupClient();
        }

        protected override void OnStop()
        {
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

        protected void SetupQueue()
        {
            queue = new PersistentQueue(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cqueuefld));
        }

        protected void SetupCache()
        {
            diskCacheInstance = new DiskCache<string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ccachefld),
                                new FixedTimespanCachePolicy<string>(TimeSpan.FromMinutes((double)ccacheduration)),
                                2 * 1024 * 1024);
        }

        protected void ConfigureService()
        {
            try
            {
                wsbaseurl = ConfigurationManager.AppSettings["BASEURL"];
                wsauthusr = ConfigurationManager.AppSettings["AUTHUSR"];
                wsauthpwd = ConfigurationManager.AppSettings["AUTHPWD"];
                ccachefld = ConfigurationManager.AppSettings["CACHEFLD"];
                cqueuefld = ConfigurationManager.AppSettings["QUEUEFLD"];
                if (!Double.TryParse(ConfigurationManager.AppSettings["CACHEDRT"], out ccacheduration)) ccacheduration = cacheDurationMins; // default cache duration in minutes
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
