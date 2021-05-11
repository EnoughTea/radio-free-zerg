using System;
using System.IO;

namespace RadioFreeZerg.CuteRadio
{
    /// <summary> A CuteRadio station resource identifies an internet radio station. </summary>
    public record CuteRadioStationResource(int Id,
                                           string Title,
                                           string Description,
                                           string Genre,
                                           string Country,
                                           string Language,
                                           string Source,
                                           int PlayCount,
                                           DateTime? LastPlayed,
                                           int CreatorId,
                                           bool Approved)
    {
        public RadioStation? ToRadioOrNull() {
            try {
                return RadioStation.FromRawSource(Id, Title, Description, Genre, Country, Language, Source,
                    Array.Empty<string>());
            } catch (InvalidDataException) {
                return null;
            }
        }
    }
}