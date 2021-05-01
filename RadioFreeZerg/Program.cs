using System;
using System.Threading.Tasks;

namespace RadioFreeZerg
{
    internal class Program
    {
        private static async Task Main(string[] args) {
            var unavailableRadio = new RadioStation(0, "NA", "NA", "http://balkan.dj.topstream.net:8070");
            var (uri, headers) = await unavailableRadio.FindStreamUriAsync().ConfigureAwait(false);
            Console.WriteLine($"{uri.AbsoluteUri} [{string.Join(", ", headers)}]");
        }
    }
}