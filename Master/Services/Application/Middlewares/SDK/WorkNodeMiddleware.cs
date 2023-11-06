using NetX.Common;
using SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class WorkNodeMiddleware : IApplicationMiddleware<GrpcContext<GetWorkersRequest, GetWorkersResponse>>
{
    private readonly INodeManagement _nodeManager;

    public WorkNodeMiddleware(INodeManagement nodeManager)
    {
        _nodeManager = nodeManager;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<GetWorkersRequest, GetWorkersResponse>> next, GrpcContext<GetWorkersRequest, GetWorkersResponse> context)
    {
        try
        {
            var nodes = _nodeManager.GetAllNodes().Select(p =>
            new Node()
            {
                Id = p.Id,
                Name = p.SystemInfo.Platform.MachineName,
                Status = p.Status.ToString(),
                LastUsed = p.LastUsed.DateTimeToUnixTimestamp()
            });
            context.Response.Response.Nodes.AddRange(nodes);
            context.Response.Response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            context.Response.Response.IsSuccess = false;
            context.Response.Response.ErrorMessage = ex.Message;
        }
        await Task.CompletedTask;
    }
}
