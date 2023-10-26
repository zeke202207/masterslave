using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

public class MonitorClient : IDisposable
{
    private readonly string _host;
    private GrpcChannel _channel;
    private SDK.MasterMonitorSDK.MasterMonitorSDKClient _client;
    public Action<Exception> Logger;

    public MonitorClient(string host)
    {
        _host = host;
        InitializeClient();
    }

    private void InitializeClient()
    {
        _channel = GrpcChannel.ForAddress(_host, new GrpcChannelOptions()
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue,
        });
        _client = new SDK.MasterMonitorSDK.MasterMonitorSDKClient(_channel);
    }

    public async Task<bool> Connect()
    {
        try
        {
            var result = await _client.ConnectAsync(new SDK.ConnectRequest());
            if(!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(bool);
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
            var result = await _client.GetWorkersAsync(new SDK.GetWorkersRequest());
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
            var result = await _client.GetWorkerInfoAsync(new SDK.WorkerInfoRequest() { Id = nodeId });
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

            info.Memory= new MemoryInfo()
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
            var result = await _client.GetJobTrackerAsync(new SDK.JobTrackerRequest() { Id = nodeId });
            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            return result.JobTracker.Select(p => new JobTrackerModel()
            {
                JobId = p.JobId,
                NodeId = p.NodeId,
                NodeName = p.NodeName,
                Status = p.Status,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
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
