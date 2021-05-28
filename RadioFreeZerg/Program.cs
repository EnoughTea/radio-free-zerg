namespace RadioFreeZerg
{
    internal class Program
    {
        private static void Main(string[] args) {
            var radioStationsProvider = new CuteRadioStationProviderJson("stations.json");
        }
    }
}