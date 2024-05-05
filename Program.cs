using System.ServiceProcess;

namespace NetMonitoringService
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new NetMonitoringService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
