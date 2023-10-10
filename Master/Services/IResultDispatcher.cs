namespace NetX.Master;

public interface IResultDispatcher
{
    /// <summary>
    /// 注册结果监听
    /// </summary>
    /// <param name="consumer"></param>
    void ConsumerRegister(ResultDispatcherConsumer consumer);

    /// <summary>
    /// 取消结果监听注册
    /// </summary>
    /// <param name="consumer"></param>
    void ConsumerUnRegister(ResultDispatcherConsumer consumer);

    /// <summary>
    /// 记录结果
    /// </summary>
    /// <param name="result"></param>
    void WriteResult(ResultModel result);

    /// <summary>
    /// 获取全部订阅的结果消费者
    /// </summary>
    /// <returns></returns>
    IEnumerable<ResultDispatcherConsumer> GetConsumers();
}
