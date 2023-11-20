namespace NetX.MasterSDK;

public class WorkerNode
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public long LastUsed { get; set; }

    public override string ToString()
    {
        return $"{Name}({Id})";
    }
}
