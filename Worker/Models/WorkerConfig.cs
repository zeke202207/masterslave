namespace NetX.Worker;

/// <summary>
/// 节点配置项目
/// </summary>
public class WorkerConfig
{
    /// <summary>
    /// 节点唯一标识，同一集群唯一
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// grpc服务主机地址
    /// </summary>
    public string GrpcServer { get; set; }
}
