using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

/// <summary>
/// 任务跟踪器实体对象
/// </summary>
public class JobTrackerModel
{
    /// <summary>
    ///  任务唯一标识
    /// </summary>
    [ColumnName("任务标识")]
    public string JobId { get; set; }

    /// <summary>
    ///  任务处理的节点计算机id标识
    /// </summary>
    //[ColumnName("处理节点唯一标识")]
    public string NodeId { get; set; }

    /// <summary>
    ///  任务处理的节点计算机名称
    /// </summary>
    public string NodeName { get; set; }

    /// <summary>
    ///  任务开始时间
    /// </summary>
    [ColumnName("任务开始时间")]
    public long StartTime { get; set; }

    /// <summary>
    ///  任务结束时间
    /// </summary>
    [ColumnName("任务结束时间")]
    public long EndTime { get; set; }

    /// <summary>
    ///  任务执行状态
    /// </summary>
    [ColumnName("任务执行结果")]
    public string Status { get; set; }

    /// <summary>
    /// 其他信息
    /// </summary>
    [ColumnName("详情")]
    public string Message { get; set; }
}

public class ColumnNameAttribute : Attribute
{
    public string Name { get; set; }

    public ColumnNameAttribute(string name)
    {
        Name = name;
    }
}
