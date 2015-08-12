using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace BackEndProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("Tracker");
                x.SetDisplayName("Akka.NET Crawl Tracker");
                x.SetDescription("Akka.NET Cluster Demo - Web Crawler.");

                x.UseAssemblyInfoForServiceInfo();
                x.RunAsLocalSystem();
                x.StartAutomatically();
                //x.UseNLog();
                x.Service<BackEndActorSystem>();
                x.EnableServiceRecovery(r => r.RestartService(1));
            });
        }
    }
}
