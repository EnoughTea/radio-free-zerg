using System;

namespace RadioFreeZerg
{
    public sealed record RadioStation(int Id,
                                      string Title,
                                      string Description,
                                      string Genre,
                                      string Country,
                                      string Language,
                                      Uri Source)
    {
        public static RadioStation Empty = new(0, "No station", "", "", "", "", new Uri("about:blank"));

        public bool Equals(RadioStation? other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override int GetHashCode() => Id;

        public override string ToString() => $"{Title} [{Genre}]";
    }
}