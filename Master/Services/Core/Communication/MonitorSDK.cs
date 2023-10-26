using Grpc.Core;
using NetX.Common;
using NetX.MemoryQueue;
using SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的Monitor SDK
/// </summary>
public class MonitorSDK : SDK.MasterMonitorSDK.MasterMonitorSDKBase
{
    private readonly ILogger _logger;
    private readonly INodeManagement _nodeManager;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="logger"></param>
    public MonitorSDK(ILogger<MonitorSDK> logger, INodeManagement nodeManager)
    {
        _logger = logger;
        _nodeManager = nodeManager;
    }

    public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
    {
        ConnectResponse response = new ConnectResponse();
        try
        {
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
        }
        return Task.FromResult(response);
    }

    public override async Task<GetWorkersResponse> GetWorkers(GetWorkersRequest request, ServerCallContext context)
    {
        GetWorkersResponse response = new GetWorkersResponse();
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
            response.Nodes.AddRange(nodes);
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
        }
        return await Task.FromResult(response);
    }

    public override Task<WorkerInfoResponse> GetWorkerInfo(WorkerInfoRequest request, ServerCallContext context)
    {
        WorkerInfoResponse response = new WorkerInfoResponse();
        try
        {
            var node = _nodeManager.GetNode(request.Id);
            if (node == null)
            {
                response.IsSuccess = false;
                response.ErrorMessage = $"Node {request.Id} not found";
                return Task.FromResult(response);
            }
            response.PlatformInfo = new SDK.PlatformInfo()
            {
                MachineName = node.SystemInfo.Platform.MachineName,
                FrameworkDescription = node.SystemInfo.Platform.FrameworkDescription,
                FrameworkVersion = node.SystemInfo.Platform.FrameworkVersion,
                OSArchitecture = node.SystemInfo.Platform.OSArchitecture,
                OSDescription = node.SystemInfo.Platform.OSDescription,
                OSPlatformID = node.SystemInfo.Platform.OSPlatformID,
                OSVersion = node.SystemInfo.Platform.OSVersion,
                ProcessArchitecture = node.SystemInfo.Platform.ProcessArchitecture,
                ProcessorCount = node.SystemInfo.Platform.ProcessorCount,
                UserName = node.SystemInfo.Platform.UserName,
                UserDomainName = node.SystemInfo.Platform.UserDomainName,
                IsUserInteractive = node.SystemInfo.Platform.IsUserInteractive                 
            };

            response.CpuInfo = new SDK.CpuInfo()
            {
               CpuLoad = node.SystemInfo.Cpu.CPULoad
            };

            response.MemoryInfo = new SDK.MemoryInfo()
            {
                AvailablePhysicalMemory = (ulong)node.SystemInfo.Memory.AvailablePhysicalMemory,
                AvailableVirtualMemory = (ulong)node.SystemInfo.Memory.AvailableVirtualMemory,
                TotalPhysicalMemory = (ulong)node.SystemInfo.Memory.TotalPhysicalMemory,
                TotalVirtualMemory = (ulong)node.SystemInfo.Memory.TotalVirtualMemory,
                UsedPhysicalMemory = (ulong)node.SystemInfo.Memory.UsedPhysicalMemory,
                UsedVirtualMemory = (ulong)node.SystemInfo.Memory.UsedVirtualMemory
            };

            node.SystemInfo.Disks.ForEach(p =>
            {
                response.DiskInfo.Add(new SDK.DiskInfo()
                {
                    DriveType = p.DriveType.ToString(),
                    TotalSize = (long)p.TotalSize,
                    UsedSize = (long)p.UsedSize,
                    FileSystem = p.FileSystem,
                    FreeSpace = (long)p.FreeSpace,
                    Id = p.Id,
                    Name = p.Name,
                });
            });

            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.ToString());
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
        }
        return Task.FromResult(response);
    }

    public override async Task<JobTrackerResponse> GetJobTracker(JobTrackerRequest request, ServerCallContext context)
    {
        JobTrackerResponse response = new JobTrackerResponse();
        try
        {
            //TODO: 获取缓存
            response.JobTracker.Add(new JobTracker()
            {
                JobId = "01",
                Status = "ok",
                NodeId = "01",
                NodeName = "zeke",
                StartTime = DateTime.Now.DateTimeToUnixTimestamp(),
                EndTime = DateTime.Now.DateTimeToUnixTimestamp(),
                Message = ""
            });
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
        }
        return await Task.FromResult(response);
    }
}
