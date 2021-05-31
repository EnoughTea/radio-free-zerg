using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RadioFreeZerg
{
    public class RadioStationFinder
    {
        public IEnumerable<RadioStation> Find(string userInput, IReadOnlyCollection<RadioStation> radioStations) {
            var keywords = RemoveNoise(Regex.Split(userInput, @"\s+", RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1)).ToList());
            return from station in radioStations
                   from keyword in keywords
                   where station.Title.Contains(keyword) || station.Description.Contains(keyword)
                   select station;
        }
        
        private static List<string> RemoveNoise(List<string> rawKeywords) {
            rawKeywords.Remove("a");
            return rawKeywords;
        }
    }
}