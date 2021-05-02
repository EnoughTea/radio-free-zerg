using System;
using System.Net;
using System.Net.Http;

namespace RadioFreeZerg
{
    /// <summary>
    ///     Provides shared <see cref="HttpClient" /> instance.
    ///     See https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/.
    ///     There could be certain DNS issues, so use <see cref="SetConnectionLeaseTimeout" /> if needed.
    /// </summary>
    public static class SharedHttpClient
    {
        public static HttpClient Instance { get; } = new();

        public static void SetConnectionLeaseTimeout(Uri uri, int timeout = -1) {
            string uriBase = uri.GetLeftPart(UriPartial.Authority);
            var servicePoint = ServicePointManager.FindServicePoint(new Uri(uriBase));
            servicePoint.ConnectionLeaseTimeout = timeout;
        }
    }
}