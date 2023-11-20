namespace NetX.Master;

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
    public string FrameworkDescription { get; set; }
    public string FrameworkVersion { get; set; }
    public string OSArchitecture { get; set; }
    public string OSPlatformID { get; set; }
    public string OSVersion { get; set; }
    public string OSDescription { get; set; }
    public string ProcessArchitecture { get; set; }
    public int ProcessorCount { get; set; }
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
        return $"物理内存：{Byte2Gb(UsedPhysicalMemory)}/{Byte2Gb(TotalPhysicalMemory)}G";
    }

    private string Byte2Gb(double bit)
    {
        long bytes = 1024 * 1024 * 1024;
        return (bit / bytes).ToString("F2");
    }
}

public class DiskInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DriveType { get; set; }
    public string FileSystem { get; set; }
    public double FreeSpace { get; set; }
    public double TotalSize { get; set; }
    public double UsedSize { get; set; }
}