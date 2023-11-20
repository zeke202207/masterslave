using System.Collections.Concurrent;

namespace NetX.Master.Services.Core;

public class JobTrackerCache<T> : IJobTrackerCache<T> where T : CacheItem
{
    private readonly int capacity;
    private readonly ConcurrentQueue<T> queue;
    private readonly ConcurrentDictionary<string, T> cache;

    public JobTrackerCache(int capacity)
    {
        this.capacity = capacity;
        this.queue = new ConcurrentQueue<T>();
        this.cache = new ConcurrentDictionary<string, T>();
    }

    /// <summary>
    /// 添加缓存（异步）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task AddAsync(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        // 如果缓存数量达到上限，移除最早的数据
        if (queue.Count >= capacity)
        {
            if (queue.TryDequeue(out var removedItem))
                cache.TryRemove(removedItem.JobId, out _);
        }
        queue.Enqueue(item);
        cache[item.JobId] = item;
        await Task.CompletedTask;
    }

    /// <summary>
    /// 更新缓存内容（异步）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task UpdateAsync(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        cache[item.JobId] = item;
        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取缓存对象（异步）
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<T> GetAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentNullException(nameof(itemId));
        if (cache.TryGetValue(itemId, out var item))
            return await Task.FromResult<T>(item);
        return default;
    }

    /// <summary>
    /// 获取最新N条缓存（异步）
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public async Task<IEnumerable<T>> GetLatestAsync(int count)
    {
        var queueList = queue.ToList();
        if (queueList.Count == 0)
            return Enumerable.Empty<T>();
        int maxCount = Math.Min(count, queueList.Count);
        //queueList.Reverse();
        return await Task.FromResult(queueList.ToArray()[..maxCount]);
    }

    /// <summary>
    /// 根据jobid，更新缓存内容（异步）
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="funUpate"></param>
    /// <returns></returns>
    public async Task UpdateAsync(string jobId, Func<T, T> funUpate)
    {
        var item = await GetAsync(jobId);
        if (null == item)
            return;
        funUpate?.Invoke(item);
        await UpdateAsync(item);
    }
}
