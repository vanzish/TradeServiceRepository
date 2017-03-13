using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using NLog;

namespace TradeService
{
    class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Log.Info("Application started.");
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"));

            var config = new HttpSelfHostConfiguration("http://localhost:8080");

            config.Routes.MapHttpRoute(
               "ConvertHttpRoute", "api/convert/{action}/{filename}",
               new { controller = "Convert", filename = RouteParameter.Optional });

            config.Routes.MapHttpRoute(
               "TradeRecordHttpRoute", "api/record/{filename}/{id}",
               new { controller = "TradeRecord" });

            config.Routes.MapHttpRoute(
               "DownloadHttpRoute", "api/download/{url}",
               new { controller = "Download" });

            using(HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                server.OpenAsync().Wait();
                Log.Info("Web API started.");
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }

            string fileName = @"C:\BinaryDataFile1";
            //FileHelper.CreateBinaryFile(fileName); //uncomment to create binary file
            //ReadBinaryFile(fileName);
        }
    }
}
