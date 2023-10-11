using MasterWorkerService;
using NetX.Common;

namespace NetX.Master;

public class RegisterMiddleware : IApplicationMiddleware<GrpcContext<RegisterNodeRequest, RegisterNodeResponse>>
{

    private readonly INodeManagement _nodeManagement;

    public RegisterMiddleware(INodeManagement nodeManagement)
    {
        _nodeManagement = nodeManagement;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<RegisterNodeRequest, RegisterNodeResponse>> next, GrpcContext<RegisterNodeRequest, RegisterNodeResponse> context)
    {
        _nodeManagement.NodeRegister(new WorkerNode()
        {
            Id = context.Reqeust.Request.Node.Id,
            Status = context.Reqeust.Request.Node.IsBusy ? WorkNodeStatus.Busy : WorkNodeStatus.Idle,
            LastUsed = context.Reqeust.Request.Node.LastUsed.UnixTimestampToDateTime(),
            LastHeartbeat = context.Reqeust.Request.Node.LastUsed.UnixTimestampToDateTime(),
        });
        await Task.CompletedTask;
    }
}
