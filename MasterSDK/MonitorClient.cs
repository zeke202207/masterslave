using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

public class MonitorClient : IDisposable
{
    private readonly string _host;
    private readonly string _userName;
    private readonly string _password;
    private GrpcChannel _channel;
    private SDK.MasterMonitorSDK.MasterMonitorSDKClient _client;
    public Action<Exception> Logger;
    private string _jwtToken = string.Empty;

    internal MonitorClient(string host,string username,string pwd)
    {
        _userName = username;
        _password = pwd;
        _host = host;
        _jwtToken = string.Empty;
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
            var result = await _client.ConnectAsync(new SDK.ConnectRequest() { UserName = _userName, Password = _password });
            if (!result.IsSuccess)
                throw new Exception(result.ErrorMessage);
            else
                _jwtToken = result.Token;
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

    private DateTime UnixTimestampToDateTime(long unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimestamp);
        return dateTime.ToLocalTime();
    }

    /// <summary>
    /// 获取通信元数据
    /// </summary>
    /// <returns></returns>
    private Metadata GetMetadata()
    {
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {_jwtToken}");
        return headers;
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
