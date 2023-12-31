﻿using MasterWorkerService;
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
        await _nodeManagement.NodeRegister(new WorkerNode()
        {
            Id = context.Reqeust.Request.Node.Id,
            Name = context.Reqeust.Request.Node.Name,
            MetaData = context.Reqeust.Request.Node.MetaData.ToDictionary(kv => kv.Key, kv => kv.Value),
            Status = WorkNodeStatus.Offline,
            LastUsed = context.Reqeust.Request.Node.LastUsed.UnixTimestampToDateTime(),
            LastHeartbeat = context.Reqeust.Request.Node.LastUsed.UnixTimestampToDateTime(),
        });
    }
}
