using RestSharp;

namespace RadioFreeZerg.CuteRadio
{
    public class CuteRadioClient
    {
        private const string ApiEndpoint = "http://marxoft.co.uk/api/cuteradio";
        
        /// <summary> RestClient is thread-safe except for handler operations. </summary>
        public static RestClient Instance { get; } = new(ApiEndpoint);
    }
}