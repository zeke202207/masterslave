namespace NetX.Worker;

public class WorkerItem
{
    public string Id { get; set; }

    public string Name { get; set; }

    public Dictionary<string,string> MetaData { get; set; } = new Dictionary<string,string>();

    public bool IsBusy { get; set; }

    public DateTime LastActiveTime { get; set; }
}
