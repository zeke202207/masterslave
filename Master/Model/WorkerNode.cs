namespace NetX.Master;

public class WorkerNode
{
    public string Id { get; set; }
    public WorkNodeStatus Status { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public DateTime LastUsed { get; set; }
    public WorkerNodeInfo Info { get; set; } = new WorkerNodeInfo();
}
