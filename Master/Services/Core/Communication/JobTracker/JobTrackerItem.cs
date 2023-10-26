using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master.Services.Core;

public class JobTrackerItem : CacheItem
{
    public string NodeId { get; set; }

    public DateTime StartTime { get; set; } 

    public DateTime EndTime { get; set; }

    public TrackerStatus Status { get; set; }

    public string Message { get; set; }

    public JobTrackerItem(string jobId , DateTime startTime)
    {
        base.JobId = jobId;
        StartTime = startTime;
        Status = TrackerStatus.Pending;
    }
}

public enum TrackerStatus
{
    /// <summary>
    /// 等待处理
    /// </summary>
    Pending = 0,
    /// <summary>
    /// 正在处理
    /// </summary>
    Processing = 1,
    /// <summary>
    /// 处理失败
    /// </summary>
    Failure = 2,
    /// <summary>
    /// 处理成功
    /// </summary>
    Success = 3,
    /// <summary>
    /// 处理超时
    /// </summary>
    Timeout = 4
}
