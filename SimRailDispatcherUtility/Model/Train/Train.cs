using SimRailDispatcherUtility.enums.Train;

namespace SimRailDispatcherUtility.Model.Train;

public class Train
{
    public int Id { get; }
    public string PreviousPost { get; }
    public string NextPost { get; }
    public short TrackNumber { get; }
    public DateTime DepartureTime { get; }
    public StopType StopType { get; }
    public TrainType TrainType { get; }
    public bool IsDrivenByPlayer { get; private set; }

    public TimeSpan ReminderOffset { get; }

    public Train(
        int id,
        string previousPost,
        string nextPost,
        short trackNumber,
        DateTime departureTime,
        StopType stopType,
        TrainType trainType,
        bool isDrivenByPlayer,
        TimeSpan reminderOffset)
    {
        Id = id;
        PreviousPost = previousPost;
        NextPost = nextPost;
        TrackNumber = trackNumber;
        DepartureTime = departureTime;
        StopType = stopType;
        TrainType = trainType;
        IsDrivenByPlayer = isDrivenByPlayer;
        ReminderOffset = reminderOffset;
    }

    public void SwitchIsDrivenByPlayer() => IsDrivenByPlayer = !IsDrivenByPlayer;

    public override string ToString() =>
        $"{Id} {PreviousPost} → {NextPost} Track {TrackNumber} Dep {DepartureTime:g}";
}