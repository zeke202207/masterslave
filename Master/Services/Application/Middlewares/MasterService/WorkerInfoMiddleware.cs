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
        worker.SystemInfo.Cpu.UpdateCpuInfo(context.Reqeust.Request.CpuInfo);
        worker.SystemInfo.Memory.UpdateMemoryInfo(context.Reqeust.Request.MemoryInfo);
        worker.SystemInfo.Disks.UpdateDisksInfo(context.Reqeust.Request.DiskInfo.ToList());
        worker.SystemInfo.Platform.UpdatelatformInfo(context.Reqeust.Request.PlatformInfo);
        _nodeManagement.UpdateNode(context.Reqeust.Request.Id, () => worker);
        await Task.CompletedTask;
    }
}
