using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using NetX.Common;
using NetX.Master.Services.Core;
using SDK;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的Monitor SDK
/// </summary>
public class MonitorSDK : SDK.MasterMonitorSDK.MasterMonitorSDKBase
{
    private readonly IServiceProvider _appServices;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="logger"></param>
    public MonitorSDK(IServiceProvider appServices)
    {
        _appServices = appServices;
    }

    public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new ConnectResponse());

        var application = new ApplicationBuilder<GrpcContext<ConnectRequest, ConnectResponse>>(_appServices)
                .Use<ExceptionMiddleware<ConnectRequest, ConnectResponse>>()
                .Use<ConnectMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        return grpcContext.Response.Response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Authorize]
    public override async Task<GetWorkersResponse> GetWorkers(GetWorkersRequest request, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new GetWorkersResponse());

        var application = new ApplicationBuilder<GrpcContext<GetWorkersRequest, GetWorkersResponse>>(_appServices)
                .Use<ExceptionMiddleware<GetWorkersRequest, GetWorkersResponse>>()
                .Use<AuthSDKMiddleware<GetWorkersRequest, GetWorkersResponse>>()
                .Use<WorkNodeMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        return grpcContext.Response.Response;        
    }

    [Authorize]
    public override async Task<WorkerInfoResponse> GetWorkerInfo(WorkerInfoRequest request, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new WorkerInfoResponse());

        var application = new ApplicationBuilder<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>>(_appServices)
                .Use<ExceptionMiddleware<WorkerInfoRequest, WorkerInfoResponse>>()
                .Use<AuthSDKMiddleware<WorkerInfoRequest, WorkerInfoResponse>>()
                .Use<WorkerNodeInfoMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        return grpcContext.Response.Response;
    }

    [Authorize]
    public override async Task<JobTrackerResponse> GetJobTracker(JobTrackerRequest request, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new JobTrackerResponse());

        var application = new ApplicationBuilder<GrpcContext<JobTrackerRequest, JobTrackerResponse>>(_appServices)
                .Use<ExceptionMiddleware<JobTrackerRequest, JobTrackerResponse>>()
                .Use<AuthSDKMiddleware<JobTrackerRequest, JobTrackerResponse>>()
                .Use<JobTrackerMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        return grpcContext.Response.Response;

    }
}
