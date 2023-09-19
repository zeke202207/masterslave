namespace NetX.Master
{
    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException()
            : base("工作节点未找到,请先进行注册")
        {
        }
    }
}
