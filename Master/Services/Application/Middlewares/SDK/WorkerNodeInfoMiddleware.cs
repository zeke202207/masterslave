using SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class WorkerNodeInfoMiddleware : IApplicationMiddleware<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>>
{
    private readonly INodeManagement _nodeManager;

    public WorkerNodeInfoMiddleware(INodeManagement nodeManager)
    {
        _nodeManager = nodeManager;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<WorkerInfoRequest, WorkerInfoResponse>> next, GrpcContext<WorkerInfoRequest, WorkerInfoResponse> context)
    {
        try
        {
            var node = await _nodeManager.GetNode(context.Reqeust.Request.Id);
            if (node == null)
            {
                context.Response.Response.IsSuccess = false;
                context.Response.Response.ErrorMessage = $"Node {context.Reqeust.Request.Id} not found";
                return;
            }
            context.Response.Response.PlatformInfo = new SDK.PlatformInfo()
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

            context.Response.Response.CpuInfo = new SDK.CpuInfo()
            {
                CpuLoad = node.SystemInfo.Cpu.CPULoad
            };

            context.Response.Response.MemoryInfo = new SDK.MemoryInfo()
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
                context.Response.Response.DiskInfo.Add(new SDK.DiskInfo()
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

            context.Response.Response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            context.Response.Response.IsSuccess = false;
            context.Response.Response.ErrorMessage = ex.Message;
        }
    }
}
