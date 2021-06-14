using System;
using System.Configuration;

namespace ADPasswordAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            // default cache duration in minutes
            string queuebasepath = null;

            string[] argvs = Environment.GetCommandLineArgs();

            if (argvs.Length != 3)
            {
                Console.WriteLine("Usage: {0} \"username\" \"password\"", argvs[0]);
                return;
            }


            try
            {
                queuebasepath = ConfigurationManager.AppSettings["QUEUEFLD"];
            }
            catch
            {
                Console.WriteLine("Missing parameters in config file: {0}.config", argvs[0]);
            }


            try
            {
                MidPointQueueSender mp = new MidPointQueueSender(queuebasepath);
                mp.UpdateUserPasswordByName(argvs[1], argvs[2]);
                Console.WriteLine("Password changed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
