using System;
using System.Threading.Tasks;
using NLog;
using RadioFreeZerg.CuteRadio;

namespace RadioFreeZerg
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private static async Task Main(string[] args) {
            var stationsPage = await CuteRadioStationsPage.FetchAsync(CuteRadioStationSearchModel.FromSearch("", 0, 10))
                                                          .ConfigureAwait(false);
            var nextPage = await stationsPage.FetchNextAsync().ConfigureAwait(false);
            var radioStations = stationsPage.Stations.ToRadioStations();
            
            var testStation = RadioStation.FromRawSource(0, "NA", "NA", "http://balkan.dj.topstream.net:8070");
            var (uri, headers) = await testStation.FindStreamUriAsync().ConfigureAwait(false);
            Console.WriteLine($"{uri.AbsoluteUri} [{string.Join(", ", headers)}]");
        }
    }
}