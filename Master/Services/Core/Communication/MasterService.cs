using Grpc.Core;
using MasterWorkerService;

namespace NetX.Master;

/// <summary>
/// grpc服务master实例
/// </summary>
public class MasterService : MasterWorkerService.MasterNodeService.MasterNodeServiceBase
{
    private readonly IServiceProvider _appServices;

    /// <summary>
    /// grpc服务master实例
    /// </summary>
    /// <param name="appServices"></param>
    public MasterService(
        IServiceProvider appServices)
    {
        _appServices = appServices;
    }

    /// <summary>
    /// 工作节点注册
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<RegisterNodeResponse> RegisterNode(RegisterNodeRequest request, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<RegisterNodeRequest, RegisterNodeResponse>(context, request, new RegisterNodeResponse());

        var application = new ApplicationBuilder<GrpcContext<RegisterNodeRequest, RegisterNodeResponse>>(_appServices)
                .Use<ExceptionMiddleware<RegisterNodeRequest, RegisterNodeResponse>>()
                .Use<AuthMiddleware<RegisterNodeRequest, RegisterNodeResponse>>()
                .Use<RegisterMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
        grpcContext.Response.Response.IsSuccess = true;

        return await Task.FromResult(grpcContext.Response.Response);
    }

    /// <summary>
    /// 工作节点取消注册
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<UnregisterNodeResponse> UnregisterNode(UnregisterNodeRequest request, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<UnregisterNodeRequest, UnregisterNodeResponse>(context, request, new UnregisterNodeResponse());

        var application = new ApplicationBuilder<GrpcContext<UnregisterNodeRequest, UnregisterNodeResponse>>(_appServices)
                .Use<ExceptionMiddleware<UnregisterNodeRequest, UnregisterNodeResponse>>()
                .Use<AuthMiddleware<UnregisterNodeRequest, UnregisterNodeResponse>>()
                .Use<UnRegisterMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
        grpcContext.Response.Response.IsSuccess = true;

        return await Task.FromResult(grpcContext.Response.Response);
    }

    /// <summary>
    /// 工作节点建立的长连接
    /// 用于下发任务到工作节点
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task ListenForJob(ListenForJobRequest request, IServerStreamWriter<ListenForJobResponse> responseStream, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<ListenForJobRequest, ListenForJobResponse>(context, request, new ListenForJobResponse());
        grpcContext.Response.ResponseStream = responseStream;
        grpcContext.CancellationToken = context.CancellationToken;

        var application = new ApplicationBuilder<GrpcContext<ListenForJobRequest, ListenForJobResponse>>(_appServices)
                .Use<ExceptionMiddleware<ListenForJobRequest, ListenForJobResponse>>()
                .Use<AuthMiddleware<ListenForJobRequest, ListenForJobResponse>>()
                .Use<ListenForJobMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
    }

    /// <summary>
    /// 结果集监听任务，用户获取slave节点的所有结果
    /// </summary>
    /// <param name="requestStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<ListenForResultReponse> ListenForResult(IAsyncStreamReader<ListenForResultRequest> requestStream, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<ListenForResultRequest, ListenForResultReponse>(context, requestStream.Current, new ListenForResultReponse());
        grpcContext.Reqeust.RequestStream = requestStream;
        grpcContext.CancellationToken = context.CancellationToken;

        var application = new ApplicationBuilder<GrpcContext<ListenForResultRequest, ListenForResultReponse>>(_appServices)
                .Use<ExceptionMiddleware<ListenForResultRequest, ListenForResultReponse>>()
                .Use<AuthMiddleware<ListenForResultRequest, ListenForResultReponse>>()
                .Use<ListenForResultMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
        return grpcContext.Response.Response;
    }

    /// <summary>
    /// 心跳包检查
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<HeartbeatRequest, HeartbeatResponse>(context, request, new HeartbeatResponse());

        var application = new ApplicationBuilder<GrpcContext<HeartbeatRequest, HeartbeatResponse>>(_appServices)
                .Use<ExceptionMiddleware<HeartbeatRequest, HeartbeatResponse>>()
                .Use<AuthMiddleware<HeartbeatRequest, HeartbeatResponse>>()
                .Use<HeartbeatMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
        grpcContext.Response.Response.IsSuccess = true;

        return await Task.FromResult(grpcContext.Response.Response);
    }

    /// <summary>
    /// 节点信息上报
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<WorkerInfoResponse> WorkerInfo(WorkerInfoRequest request, ServerCallContext context)
    {
        var grpcContext = CreateGrpcContext<WorkerInfoRequest, WorkerInfoResponse>(context, request, new WorkerInfoResponse());

        var application = new ApplicationBuilder<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>>(_appServices)
                .Use<ExceptionMiddleware<WorkerInfoRequest, WorkerInfoResponse>>()
                .Use<AuthMiddleware<WorkerInfoRequest, WorkerInfoResponse>>()
                .Use<WorkerInfoMiddleware>()
                .Build();

        await application.Invoke(grpcContext);
        grpcContext.Response.Response.IsSuccess = true;

        return await Task.FromResult(grpcContext.Response.Response);
    }

    /// <summary>
    /// 构建请求处理上下文
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="context"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    private GrpcContext<TRequest, TResponse> CreateGrpcContext<TRequest, TResponse>(ServerCallContext context, TRequest request, TResponse response)
    {
        var client = new GrpcClient(context);
        var grpcRequest = GrpcRequest<TRequest>.Create(request);
        var grpcResponse = new GrpcResponse<TResponse>(response);
        var grpcContext = new GrpcContext<TRequest, TResponse>(client, grpcRequest, grpcResponse, context.GetHttpContext().Features)
        {
            CancellationToken = context.CancellationToken
        };
        return grpcContext;
    }
}
