namespace NetX.Master;

public class WorkerNode
{
    private int timeout { get; } = 10;

    public string Id { get; set; }
    public WorkNodeStatus Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public DateTime LastUsed { get; set; }
    public WorkerNodeInfo Info { get; set; } = new WorkerNodeInfo();

    public bool IsTimeout()
    {
        return DateTime.Now - this.LastHeartbeat > TimeSpan.FromSeconds(timeout);
    }
}
