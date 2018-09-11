using MR.Servers;

namespace VstupInfoParser
{
    internal class MainApp
    {
        private static MainApp app;
        public static MainApp GetApp => app ?? (app = new MainApp());

        private Reactor Core { get; set; }
        public enum Dirs { tmp_csv}
        public void Initialize()
        {
            Core = new Reactor();

            var server = Core.GetServer("VstupInfoParser", 3000);

            server.SetApiController(typeof(Api));
            server.DirectoryManager.AddDir(Dirs.tmp_csv, "tmp_csv");
            server.Config.Main.AccessFolders.Add("/csv", server.DirectoryManager[Dirs.tmp_csv]);
            server.EventMaster.ServerStarting += EventMaster_ServerStarting;

            Core.Listen(server).Wait();
        }

        private async void EventMaster_ServerStarting(IServer sender, object args = null)
        { 
            await CoreParser.Current.FetchTable();
        }
    }
}