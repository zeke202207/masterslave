namespace NetX.Master;

/// <summary>
/// job执行结果实体对象
/// </summary>
public class JobItemResult
{
    /// <summary>
    /// job唯一标识
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// job结果集合
    /// </summary>
    public byte[] Result { get; set; }
}
