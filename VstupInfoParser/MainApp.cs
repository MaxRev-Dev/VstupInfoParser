using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using MaxRev.Servers;
using MaxRev.Servers.Core.Modules.RequestProcessing;
using MaxRev.Servers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VstupInfoParser.Parsers;

namespace VstupInfoParser
{
    internal class MainApp
    {
        internal static readonly CultureInfo DefaultCultureInfo = new CultureInfo("uk-UA");
        public enum Dirs { TmpCsv }
        public static Task Initialize(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            // or just use ReactorStartup.Default;
            // but Balancer will be unable to start without passing args 
            var runtime = ReactorStartup.From(args, new ReactorStartupConfig
            {
                AutoregisterControllers = true,
                AwaitForConsoleInput = true, // suspends on Alt+C by default, don`t use with redirected output
                // default - SuspendingKeyInfo = new ConsoleKeyInfo('C', ConsoleKey.C, false, true, false)
            });
            runtime.Configure((with, core) =>
            {
                with.Services(c =>
                {
                    // to use DI container add package Microsoft.Extensions.DependencyInjection
                    c.AddSingleton<CoreParser>();
                });
                with.Modules(mp =>
                {
                    // add module to the beginning of request pipeline 
                    mp.AddAfter<RequestLoggerModule, RequestInfoLogModule_Sample>();
                });
                var server = core.GetServer("VstupInfoParser", 3000);

                // Reactor can automatically find non generic controllers
                // for specific cases - generic implementations (like Api<Impl>) we need to provide custom one 
                // server.SetApiControllers(typeof(Api));
                // add directory with access key to dir manager
                server.DirectoryManager.AddDir(Dirs.TmpCsv, "tmp_csv");
                // add virtual directory 
                server.Config.Main.AccessFolders.Add("/csv", server.DirectoryManager[Dirs.TmpCsv]);
                // register an event handler
                server.EventMaster.ServerStarting += EventMaster_ServerStarting;

                // here we can add more servers... 
            });
            //starts all configured services and servers
            return runtime.RunAsync();
        }

        private static async void EventMaster_ServerStarting(IServer sender, object args = null)
        {
            var currentParser = sender.Parent.Services.GetRequiredService<CoreParser>();
            await currentParser.FetchTableAsync(); 
        }
    }
}