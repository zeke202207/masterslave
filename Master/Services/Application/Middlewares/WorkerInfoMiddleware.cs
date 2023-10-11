using MasterWorkerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class WorkerInfoMiddleware : IApplicationMiddleware<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>>
{
    private readonly INodeManagement _nodeManagement;

    public WorkerInfoMiddleware(INodeManagement nodeManagement)
    {
        _nodeManagement = nodeManagement;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>> next, GrpcContext<WorkerInfoRequest, WorkerInfoResponse> context)
    {
        var worker = _nodeManagement.GetNode(context.Reqeust.Request.Id);
        if (null == worker)
            throw new NodeNotFoundException();
        worker.Info.Cpu = context.Reqeust.Request.Cpu;
        worker.Info.Memory = context.Reqeust.Request.Memory;
        _nodeManagement.UpdateNode(context.Reqeust.Request.Id, () => worker);
        await Task.CompletedTask;
    }
}
