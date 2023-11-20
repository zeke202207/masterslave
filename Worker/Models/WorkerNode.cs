namespace NetX.Worker;

/// <summary>
/// 节点配置项目
/// </summary>
public class WorkerNode
{
    /// <summary>
    /// 节点唯一标识，同一集群唯一
    /// </summary>
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// grpc服务主机地址
    /// </summary>
    public string GrpcServer { get; set; }

    public Dictionary<string, string> MetaData { get; set; }
}

public record class KeyValue(string Key, string Value);
