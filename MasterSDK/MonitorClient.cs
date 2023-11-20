namespace NetX.MasterSDK;

public class MonitorClient : BaseClient<SDK.MasterMonitorSDK.MasterMonitorSDKClient>, IDisposable
{
    internal MonitorClient(string host, string username, string pwd)
        : base(host, username, pwd)
    {

    }

    protected override SDK.MasterMonitorSDK.MasterMonitorSDKClient CreateClient(GrpcChannel channel)
    {
        return new SDK.MasterMonitorSDK.MasterMonitorSDKClient(channel);
    }

    /// <summary>
    /// 登录连接
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    protected override string Login(string userName, string password)
    {
        try
        {
            var result = _client.Login(new SDK.LoginRequest() { UserName = userName, Password = password });
            if (!result.IsSuccess)
                throw new Exception($"登录失败：{result.ErrorMessage}");
            return result.Token;
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(string);
        }
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<IEnumerable<WorkerNode>> GetWorkersAsync()
    {
        try
        {
            var result = await _client.GetWorkersAsync(new SDK.GetWorkersRequest(), GetMetadata());
            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            return result.Nodes.Select(p => new WorkerNode()
            {
                Id = p.Id,
                Name = p.Name,
                Status = p.Status,
                LastUsed = p.LastUsed
            });
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(IEnumerable<WorkerNode>);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<WorkerNodeInfo> GetWorkerNodeInfoAsync(string nodeId)
    {
        try
        {
            var result = await _client.GetWorkerInfoAsync(new SDK.WorkerInfoRequest() { Id = nodeId }, GetMetadata());
            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            var info = new WorkerNodeInfo();

            info.Platform = new PlatformInfo()
            {
                FrameworkDescription = result.PlatformInfo.FrameworkDescription,
                FrameworkVersion = result.PlatformInfo.FrameworkVersion,
                OSArchitecture = result.PlatformInfo.OSArchitecture,
                OSDescription = result.PlatformInfo.OSDescription,
                OSPlatformID = result.PlatformInfo.OSPlatformID,
                OSVersion = result.PlatformInfo.OSVersion,
                ProcessArchitecture = result.PlatformInfo.ProcessArchitecture,
                ProcessorCount = result.PlatformInfo.ProcessorCount,
                MachineName = result.PlatformInfo.MachineName,
                UserName = result.PlatformInfo.UserName,
                UserDomainName = result.PlatformInfo.UserDomainName,
                IsUserInteractive = result.PlatformInfo.IsUserInteractive
            };

            info.Cpu = new CpuInfo()
            {
                CPULoad = result.CpuInfo.CpuLoad
            };

            info.Memory = new MemoryInfo()
            {
                TotalPhysicalMemory = result.MemoryInfo.TotalPhysicalMemory,
                AvailablePhysicalMemory = result.MemoryInfo.AvailablePhysicalMemory,
                UsedPhysicalMemory = result.MemoryInfo.UsedPhysicalMemory,
                TotalVirtualMemory = result.MemoryInfo.TotalVirtualMemory,
                AvailableVirtualMemory = result.MemoryInfo.AvailableVirtualMemory,
                UsedVirtualMemory = result.MemoryInfo.UsedVirtualMemory
            };

            info.Disks = result.DiskInfo.Select(p => new DiskInfo()
            {
                Name = p.Name,
                TotalSize = p.TotalSize,
                DriveType = p.DriveType,
                FileSystem = p.FileSystem,
                Id = p.Id,
                FreeSpace = p.FreeSpace,
                UsedSize = p.UsedSize
            }).ToList();

            return info;
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(WorkerNodeInfo);
        }
    }

    /// <summary>
    /// 任务链路跟踪
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<JobTrackerModel>> GetJobTracker(string nodeId)
    {
        try
        {
            var result = await _client.GetJobTrackerAsync(new SDK.JobTrackerRequest() { Id = nodeId }, GetMetadata());
            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            return result.JobTracker.Select(p => new JobTrackerModel()
            {
                JobId = p.JobId,
                NodeId = p.NodeId,
                NodeName = p.NodeName,
                Status = p.Status,
                StartTime = UnixTimestampToDateTime(p.StartTime).ToString("yyy/MM/dd HH:mm:ss ffff"),
                EndTime = UnixTimestampToDateTime(p.EndTime).ToString("yyy/MM/dd HH:mm:ss ffff"),
                Duration = $"{p.EndTime - p.StartTime} ms",
                Message = p.Message
            });
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(IEnumerable<JobTrackerModel>);
        }
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void Dispose()
    {
        _channel.ShutdownAsync().Wait(TimeSpan.FromSeconds(60));
        _client = null;
        _channel = null;
    }

}
