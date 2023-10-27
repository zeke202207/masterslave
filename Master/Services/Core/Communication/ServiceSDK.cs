using Grpc.Core;
using SDK;
using NetX.MemoryQueue;
using Newtonsoft.Json.Linq;
using NetX.Master.Services.Core;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的SDK
/// </summary>
public class ServiceSDK : SDK.MasterServiceSDK.MasterServiceSDKBase
{
    private readonly IServiceProvider _appServices;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="logger"></param>
    /// <param name="dataTransferCenter"></param>
    public ServiceSDK(IServiceProvider serviceProvider)
    {
        _appServices = serviceProvider;
    }

    /// <summary>
    /// 开始执行任务
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task ExecuteTask(ExecuteTaskRequest request, IServerStreamWriter<ExecuteTaskResponse> responseStream, ServerCallContext context)
    {
        var grpcContext = context.CreateGrpcContext(request, new ExecuteTaskResponse());
        grpcContext.Response.ResponseStream = responseStream;

        var application = new ApplicationBuilder<GrpcContext<ExecuteTaskRequest, ExecuteTaskResponse>>(_appServices)
                .Use<ExceptionMiddleware<ExecuteTaskRequest, ExecuteTaskResponse>>()
                .Use<ExecuteTaskMiddleware>()
                .Build();

        await application.Invoke(grpcContext);

        await Task.FromResult(grpcContext.Response.Response);
    }
}
