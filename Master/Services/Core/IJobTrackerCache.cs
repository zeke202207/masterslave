namespace NetX.Master.Services.Core;

public interface IJobTrackerCache<T> where T : CacheItem
{
    Task AddAsync(T item);
    Task UpdateAsync(T item);
    Task UpdateAsync(string jobId, Func<T, T> funUpate);
    Task<T> GetAsync(string itemId);
    Task<IEnumerable<T>> GetLatestAsync(int count);
}
