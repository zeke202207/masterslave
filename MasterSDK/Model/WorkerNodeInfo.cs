using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

/// <summary>
/// 工作节点计算机信息实体对象
/// </summary>
public class WorkerNodeInfo
{
    /// <summary>
    /// 平台
    /// </summary>
    public PlatformInfo Platform { get; set; } = new PlatformInfo();

    /// <summary>
    /// Cpu
    /// </summary>
    public CpuInfo Cpu { get; set; } = new CpuInfo();

    /// <summary>
    /// 内存
    /// </summary>
    public MemoryInfo Memory { get; set; } = new MemoryInfo();

    /// <summary>
    /// 硬盘
    /// </summary>
    public List<DiskInfo> Disks { get; set; } = new List<DiskInfo>();
}

public class PlatformInfo
{
    [DisplayAtribute("      版本：",0)]
    public string FrameworkDescription { get; set; }
    public string FrameworkVersion { get; set; }
    [DisplayAtribute("  系统类型：",2)]
    public string OSArchitecture { get; set; }
    public string OSPlatformID { get; set; }
    [DisplayAtribute("  系统版本：",3)]
    public string OSVersion { get; set; }
    public string OSDescription { get; set; }
    [DisplayAtribute("   CPU类型：", 4)]
    public string ProcessArchitecture { get; set; }
    [DisplayAtribute("CPU 核心数：", 4)]
    public int ProcessorCount { get; set; }
    [DisplayAtribute("  设备名称：",1)]
    public string MachineName { get; set; }
    public string UserName { get; set; }
    public string UserDomainName { get; set; }
    public bool IsUserInteractive { get; set; }
}

public class CpuInfo
{
    public double CPULoad { get; set; }
}

public class MemoryInfo
{
    public double TotalPhysicalMemory { get; set; }
    public double AvailablePhysicalMemory { get; set; }
    public double UsedPhysicalMemory { get; set; }
    public double TotalVirtualMemory { get; set; }
    public double AvailableVirtualMemory { get; set; }
    public double UsedVirtualMemory { get; set; }

    public override string ToString()
    {
        return $"{UsedPhysicalMemory.Byte2Gb()}/{TotalPhysicalMemory.Byte2Gb()}G";
    }
}

public class DiskInfo
{
    public string Id { get; set; }
    [ColumnName("盘符")]
    public string Name { get; set; }
    public string DriveType { get; set; }
    [ColumnName("文件系统")]
    public string FileSystem { get; set; }
    public double FreeSpace { get; set; }
    public double TotalSize { get; set; }
    public double UsedSize { get; set; }

    [ColumnName("剩余容量")]
    public string Free => FreeSpace.Byte2Gb();
    [ColumnName("总容量")]
    public string Total => TotalSize.Byte2Gb();
    [ColumnName("使用率")]
    public string UsedPercent => ((UsedSize / TotalSize) * 100).ToString("F2") + "%";
}

[AttributeUsage(AttributeTargets.Property)]
public class DisplayAtribute : Attribute
{
    public string DescriptionName { get; set; }

    public int Order { get; set; }

    public DisplayAtribute(string name, int order)
    {
        DescriptionName = name;
        Order = order;
    }
}

public static class ConvertExtentions
{
    public static string Byte2Gb(this double bit)
    {
        long bytes = 1024 * 1024 * 1024;
        return (bit / bytes).ToString("F2") + " GB";
    }
}