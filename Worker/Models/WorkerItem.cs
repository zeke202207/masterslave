namespace NetX.Worker;

public class WorkerItem
{
    public string Id { get; set; }

    public bool IsBusy { get; set; }

    public DateTime LastActiveTime { get; set; }
}
