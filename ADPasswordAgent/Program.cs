using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ADPasswordAgent
{
    class Program
    {
        private const double cacheDurationMins = 30;

        static void Main(string[] args)
        {
             // default cache duration in minutes
            string wsbaseurl = null;
            string wsauthusr = null;
            string wsauthpwd = null;
            string ccachefld = @"Midpoint.ADPassword.Cache";
            double ccacheduration = cacheDurationMins; // default cache duration in minutes

            string[] argvs = Environment.GetCommandLineArgs();

            try
            {
                wsbaseurl = ConfigurationManager.AppSettings["BASEURL"];
                wsauthusr = ConfigurationManager.AppSettings["AUTHUSR"];
                wsauthpwd = ConfigurationManager.AppSettings["AUTHPWD"];
                ccachefld = ConfigurationManager.AppSettings["CACHEFLD"];
                if (!Double.TryParse(ConfigurationManager.AppSettings["CACHEDRT"], out ccacheduration)) ccacheduration = cacheDurationMins; // default cache duration in minutes
            }
            catch
            {
                Console.WriteLine("Missing parameters in config file: {0}.config", argvs[0]);
            }

            if (argvs.Length != 3)
            {
                Console.WriteLine("Usage: {0} \"username\" \"password\"", argvs[0]);
                return;
            }

            try
            {
                midPoint mp = new midPoint(wsbaseurl, wsauthusr, wsauthpwd, ccachefld, ccacheduration);
                if (mp.UpdateUserPasswordByName(argvs[1], argvs[2]))
                {
                    Console.WriteLine("Password changed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
