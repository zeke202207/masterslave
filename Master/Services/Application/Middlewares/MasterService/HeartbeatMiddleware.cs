using MasterWorkerService;
using NetX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class HeartbeatMiddleware : IApplicationMiddleware<GrpcContext<HeartbeatRequest, HeartbeatResponse>>
{
    private readonly INodeManagement _nodeManagement;

    public HeartbeatMiddleware(INodeManagement nodeManagement)
    {
        _nodeManagement = nodeManagement;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<HeartbeatRequest, HeartbeatResponse>> next, GrpcContext<HeartbeatRequest, HeartbeatResponse> context)
    {
        var worker = await _nodeManagement.GetNode(context.Reqeust.Request.Id);
        if (null == worker)
            throw new NodeNotFoundException();
        worker.LastHeartbeat = context.Reqeust.Request.CurrentTime.UnixTimestampToDateTime();
        await _nodeManagement.UpdateNode(context.Reqeust.Request.Id, () => worker);
    }
}
