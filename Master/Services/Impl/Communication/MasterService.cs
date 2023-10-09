using Grpc.Core;
using MasterWorkerService;
using NetX.Common;

namespace NetX.Master;

public class MasterService : MasterWorkerService.MasterNodeService.MasterNodeServiceBase
{
    private readonly INodeManagement _nodeManagement;
    private readonly IJobPublisher _jobPublisher;
    private readonly ISecurityPolicy _securityPolicy;
    private readonly ILogger _logger;
    private readonly DataTransferCenter _dataTransferCenter;


    public MasterService(
        INodeManagement nodeManagement,
        IJobPublisher jobPublisher,
        ISecurityPolicy securityPolicy,
        DataTransferCenter dataTransferCenter,
        ILogger<MasterService> logger)
    {
        _nodeManagement = nodeManagement;
        _jobPublisher = jobPublisher;
        _securityPolicy = securityPolicy;
        _logger = logger;
        _dataTransferCenter = dataTransferCenter;
    }

    /// <summary>
    /// 工作节点注册
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<RegisterNodeResponse> RegisterNode(RegisterNodeRequest request, ServerCallContext context)
    {
        try
        {
            if (!IsRequestAllowed(context))
                return await Task.FromResult(new RegisterNodeResponse() { IsSuccess = false, ErrorMessage = $"节点未在授权列表，注册失败" });
            _nodeManagement.AddNode(new WorkerNode()
            {
                Id = request.Node.Id,
                Status = request.Node.IsBusy ? WorkNodeStatus.Busy : WorkNodeStatus.Idle,
                LastUsed = request.Node.LastUsed.UnixTimestampToDateTime(),
                LastHeartbeat = request.Node.LastUsed.UnixTimestampToDateTime(),
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("节点注册失败", ex);
        }
        return await Task.FromResult(new RegisterNodeResponse() { IsSuccess = true });
    }

    /// <summary>
    /// 工作节点取消注册
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<UnregisterNodeResponse> UnregisterNode(UnregisterNodeRequest request, ServerCallContext context)
    {
        try
        {
            if (!IsRequestAllowed(context))
                return await Task.FromResult(new UnregisterNodeResponse() { IsSuccess = false, ErrorMessage = $"节点未在授权列表，注册失败" });
            _nodeManagement.RemoveNode(request.Node.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("节点取消注册失败", ex);
        }
        return await Task.FromResult(new UnregisterNodeResponse() { IsSuccess = true });
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
        try
        {
            if (null == request || string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentNullException(nameof(request));
            var jobObserver = new JobObserver(request.Id, responseStream);
            try
            {
                //观察者注入
                _jobPublisher.Subscribe(jobObserver);
                // Wait until the client disconnects.
                await Task.Delay(Timeout.Infinite, context.CancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError("客户端取消任务监听", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("节点监听任务失败", ex);
            }
            finally
            {
                _jobPublisher.Unsubscribe(jobObserver);
                _nodeManagement.RemoveNode(request.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("节点监听任务失败", ex);
        }
    }

    /// <summary>
    /// 结果集监听任务，用户获取slave节点的所有结果
    /// </summary>
    /// <param name="requestStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async override Task<ListenForResultReponse> ListenForResult(IAsyncStreamReader<ListenForResultRequest> requestStream, ServerCallContext context)
    {
        try
        {
            await foreach (var resp in requestStream.ReadAllAsync())
            {
                _dataTransferCenter.ProcessData(new ResultModel()
                {
                    JobId = resp.Id,
                    Result = resp.Result.ToByteArray(),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("结果监听任务失败", ex);
        }
        return new ListenForResultReponse();
    }

    /// <summary>
    /// 心跳包检查
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context)
    {
        try
        {
            if (!IsRequestAllowed(context))
                return await Task.FromResult(new HeartbeatResponse() { IsSuccess = false, ErrorMessage = $"节点未在授权列表，注册失败" });
            var worker = _nodeManagement.GetNode(request.Id);
            if (null == worker)
                throw new NodeNotFoundException();
            worker.LastHeartbeat = request.CurrentTime.UnixTimestampToDateTime();
            _nodeManagement.UpdateNode(request.Id, () => worker);
            _logger.LogInformation($"心跳:{worker.Id}->{worker.LastHeartbeat}");
        }
        catch (Exception ex)
        {
            _logger.LogError("心跳失败", ex);
            return await Task.FromResult(new HeartbeatResponse() { IsSuccess = false, ErrorMessage = ex.ToString() });
        }
        return await Task.FromResult(new HeartbeatResponse() { IsSuccess = true });
    }

    /// <summary>
    /// 节点信息上报
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<WorkerInfoResponse> WorkerInfo(WorkerInfoRequest request, ServerCallContext context)
    {
        try
        {
            if (!IsRequestAllowed(context))
                return await Task.FromResult(new WorkerInfoResponse() { IsSuccess = false, ErrorMessage = $"节点未在授权列表，注册失败" });
            var worker = _nodeManagement.GetNode(request.Id);
            if (null == worker)
                throw new NodeNotFoundException();
            worker.Info.Cpu = request.Cpu;
            worker.Info.Memory = request.Memory;
            _nodeManagement.UpdateNode(request.Id, () => worker);
            _logger.LogInformation($"上报信息:{worker.Id}{Environment.NewLine}{worker.Info.Cpu}{Environment.NewLine}{worker.Info.Memory}");
        }
        catch (Exception ex)
        {
            _logger.LogError("节点信息获取失败", ex);
            return await Task.FromResult(new WorkerInfoResponse() { IsSuccess = false, ErrorMessage = ex.ToString() });
        }
        return await Task.FromResult(new WorkerInfoResponse() { IsSuccess = true });
    }

    /// <summary>
    /// 请求授权验证
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private bool IsRequestAllowed(ServerCallContext context)
    {
        return _securityPolicy.IsRequestAllowed(new SecurityContext() { ClientIp = context.Host });
    }
}
