namespace NetX.Master;

/// <summary>
/// job实体对象
/// </summary>
/// <param name="jobId">job唯一标识</param>
/// <param name="jobData">job内容</param>
/// <param name="metaData">job元数据</param>
public record class JobItem(string jobId, byte[] jobData, Dictionary<string,string> metaData);
