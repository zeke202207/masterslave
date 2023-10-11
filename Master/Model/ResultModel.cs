
namespace NetX.Master;

/// <summary>
/// 结果实体对象
/// </summary>
public class ResultModel
{
    /// <summary>
    /// job唯一标识
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// 结果集合
    /// </summary>
    public byte[] Result { get; set; }
}
