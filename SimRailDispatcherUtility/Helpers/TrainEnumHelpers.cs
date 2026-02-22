using SimRailDispatcherUtility.enums.Train;

namespace SimRailDispatcherUtility.Helpers;

public static class StopTypeValues
{
    public static StopType[] All { get; } =
    {
        StopType.Passenger,
        StopType.Technical,
        StopType.EarlyArrival
    };
}

public static class TrainTypeValues
{
    public static TrainType[] All { get; } =
    {
        TrainType.Intercity,
        TrainType.Regional,
        TrainType.Freight
    };
}