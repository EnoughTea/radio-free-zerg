using System;

namespace RadioFreeZerg
{
    public record CuteRadioStationProperties(int Id,
                                             string Title,
                                             string Description,
                                             string Genre,
                                             string Country,
                                             string Language,
                                             Uri Source,
                                             string[] ContentType);
}