using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RadioFreeZerg
{
    public class RadioStationFinder
    {
        private static readonly HashSet<string> StopWords = new() {
            "a", "an", "the", "and", "but", "or", "as", "of", "at", "by", "for", "with", "to", "s", "t"
        };

        public IEnumerable<RadioStation> Find(string userInput, IReadOnlyCollection<RadioStation> radioStations) {
            var keywords = RemoveNoise(Regex.Split(userInput, @"\s+", RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1)));
            return from station in radioStations
                   from keyword in keywords
                   where station.Title.Contains(keyword) || station.Description.Contains(keyword)
                   select station;
        }

        private static IEnumerable<string> RemoveNoise(string[] rawKeywords) {
            var deduplicatedKeywords = rawKeywords.ToHashSet(StringComparer.OrdinalIgnoreCase);
            deduplicatedKeywords.ExceptWith(StopWords);
            return deduplicatedKeywords;
        }
    }
}