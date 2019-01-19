using MaxRev.Servers;
using MaxRev.Servers.Interfaces;
using VstupInfoParser.Parsers;

namespace VstupInfoParser
{
    internal class MainApp
    {
        private static MainApp _app;
        public static MainApp GetApp => _app ?? (_app = new MainApp());

        public enum Dirs { TmpCsv }
        public void Initialize(string[] args)
        {
            ReactorStartup.From(args)
               .Configure((with, core) =>
               {
                   var server = core.GetServer("VstupInfoParser", 3000);

                   server.SetApiControllers(typeof(Api));
                   server.DirectoryManager.AddDir(Dirs.TmpCsv, "tmp_csv");
                   server.Config.Main.AccessFolders.Add("/csv", server.DirectoryManager[Dirs.TmpCsv]);
                   server.EventMaster.ServerStarting += EventMaster_ServerStarting;
               }).Run();
        }

        private async void EventMaster_ServerStarting(IServer sender, object args = null)
        {
            await CoreParser.Current.FetchTable();
        }
    }
}