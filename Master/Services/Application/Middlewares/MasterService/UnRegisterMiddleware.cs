using MasterWorkerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class UnRegisterMiddleware : IApplicationMiddleware<GrpcContext<UnregisterNodeRequest, UnregisterNodeResponse>>
{
    private readonly INodeManagement _nodeManagement;

    public UnRegisterMiddleware(INodeManagement nodeManagement)
    {
        _nodeManagement = nodeManagement;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<UnregisterNodeRequest, UnregisterNodeResponse>> next, GrpcContext<UnregisterNodeRequest, UnregisterNodeResponse> context)
    {
        await _nodeManagement.NodeUnRegister(context.Reqeust.Request.Node.Id);
    }
}
