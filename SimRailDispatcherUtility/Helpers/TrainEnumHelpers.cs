using SimRailDispatcherUtility.enums.Train;

namespace SimRailDispatcherUtility.Helpers;

public static class StopTypeValues
{
    public static StopType[] All { get; } =
    {
        StopType.PassengerStop,
        StopType.TechnicalStop,
        StopType.EarlyArrivalStop
    };
}

public static class TrainTypeValues
{
    public static TrainType[] All { get; } =
    {
        TrainType.Intercity,
        TrainType.Passenger,
        TrainType.Freight
    };
}