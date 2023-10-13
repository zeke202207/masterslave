using MasterWorkerService;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public static class WorkerInfoExtensions
{
    /// <summary>
    /// 更新cpu信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="cpu"></param>
    public static void UpdateCpuInfo([NotNull] this CpuInfo info, MasterWorkerService.CpuInfo cpu)
    {
        info.CPULoad = cpu.CpuLoad;
    }

    /// <summary>
    /// 更新内存信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="memory"></param>
    public static void UpdateMemoryInfo([NotNull] this MemoryInfo info, MasterWorkerService.MemoryInfo memory)
    {
        info.UsedPhysicalMemory = memory.UsedPhysicalMemory;
        info.TotalPhysicalMemory = memory.TotalPhysicalMemory;
        info.AvailablePhysicalMemory = memory.AvailablePhysicalMemory;
        info.UsedVirtualMemory = memory.UsedVirtualMemory;
        info.TotalVirtualMemory = memory.TotalVirtualMemory;
        info.AvailableVirtualMemory = memory.AvailableVirtualMemory;
    }

    /// <summary>
    /// 更新硬盘信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="disk"></param>
    public static void UpdateDisksInfo([NotNull] this List<DiskInfo> info, List<MasterWorkerService.DiskInfo> disks)
    {
        info.Clear();
        disks.ForEach(p => info.Add(new DiskInfo()
        {
            DriveType = p.DriveType,
            FileSystem = p.FileSystem,
            FreeSpace = p.FreeSpace,
            Id = p.Id,
            Name = p.Name,
            TotalSize = p.TotalSize,
            UsedSize = p.UsedSize,
        }));
    }

    /// <summary>
    /// 更新平台信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="platform"></param>
    public static void UpdatelatformInfo([NotNull] this PlatformInfo info, MasterWorkerService.PlatformInfo platform)
    {
        info.FrameworkDescription = platform.FrameworkDescription;
        info.FrameworkVersion = platform.FrameworkVersion;
        info.OSArchitecture = platform.OSArchitecture;
        info.OSPlatformID = platform.OSPlatformID;
        info.OSVersion = platform.OSVersion;
        info.OSDescription = platform.OSDescription;
        info.ProcessArchitecture = platform.ProcessArchitecture;
        info.ProcessorCount = platform.ProcessorCount;
        info.MachineName = platform.MachineName;
        info.UserName = platform.UserName;
        info.UserDomainName = platform.UserName;
        info.IsUserInteractive = platform.IsUserInteractive;
    }
}
