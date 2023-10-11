namespace NetX.Master;

/// <summary>
/// 工作节点未找到异常
/// </summary>
public class NodeNotFoundException : Exception
{
    public NodeNotFoundException()
        : base("工作节点未找到,请先进行注册")
    {
    }
}
