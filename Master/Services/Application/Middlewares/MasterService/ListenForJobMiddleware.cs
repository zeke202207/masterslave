using MasterWorkerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class ListenForJobMiddleware : IApplicationMiddleware<GrpcContext<ListenForJobRequest, ListenForJobResponse>>
{
    private readonly IJobPublisher _jobPublisher;
    private readonly INodeManagement _nodeManagement;

    public ListenForJobMiddleware(IJobPublisher jobPublisher, INodeManagement nodeManagement)
    {
        _jobPublisher = jobPublisher;
        _nodeManagement = nodeManagement;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<ListenForJobRequest, ListenForJobResponse>> next, GrpcContext<ListenForJobRequest, ListenForJobResponse> context)
    {
        if (null == context.Reqeust.Request || string.IsNullOrWhiteSpace(context.Reqeust.Request.Id))
            throw new ArgumentNullException(nameof(context.Reqeust.Request));
        var jobObserver = new JobObserver(context.Reqeust.Request.Id, context.Response.ResponseStream);
        try
        {
            //观察者注入
            _jobPublisher.Subscribe(jobObserver);
            // Wait until the client disconnects.
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
        }
        finally
        {
            _jobPublisher.Unsubscribe(jobObserver);
            await _nodeManagement.NodeUnRegister(context.Reqeust.Request.Id);
        }
    }
}
