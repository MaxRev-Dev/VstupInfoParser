using System.Threading.Tasks;

namespace VstupInfoParser
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            return MainApp.GetApp.Initialize(args);
        }
    }
}
