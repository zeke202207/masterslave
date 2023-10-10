using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Common;

/// <summary>
/// 重试策略
/// </summary>
public class RetryPolicy
{
    private readonly int _maxRetryCount;
    private readonly TimeSpan _initialRetryInterval;

    public RetryPolicy(int maxRetryCount, TimeSpan initialRetryInterval)
    {
        _maxRetryCount = maxRetryCount;
        _initialRetryInterval = initialRetryInterval;
    }

    /// <summary>
    /// 带有重试策略的执行方法
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="shouldRetry"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(Func<Task> operation, Func<Exception, bool> shouldRetry)
    {
        for (int i = 0; i < _maxRetryCount; i++)
        {
            try
            {
                await operation();
                return;
            }
            catch (Exception ex)
            {
                if (i == _maxRetryCount - 1 || !shouldRetry(ex))
                    throw;
                await Task.Delay(_initialRetryInterval * (int)Math.Pow(2, i));
            }
        }
    }
}
