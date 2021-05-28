using System.Collections.Generic;

namespace RadioFreeZerg
{
    public interface ICuteRadioStationProvider : IEnumerable<CuteRadioStationProperties>
    {
        IReadOnlyList<CuteRadioStationProperties> All();
    }
}