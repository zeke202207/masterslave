using Microsoft.Extensions.Options;
using NetX.WorkerPlugin.Contract;

namespace NetX.Worker
{
    /// <summary>
    /// 节点主机服务
    /// </summary>
    public class WorkerHostedService : IHostedService
    {
        private readonly IMasterClient _masterClient;
        private readonly WorkerConfig _config;

        public WorkerHostedService(IMasterClient masterClient, IOptions<WorkerConfig> config)
        {
            _masterClient = masterClient;
            _config = config.Value;
        }

        /// <summary>
        /// 服务开启
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (await _masterClient.RegisterNodeAsync(new WorkerItem() { Id = _config.Id, IsBusy = false, LastActiveTime = DateTime.UtcNow }))
                    await _masterClient.Start();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 服务停止
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _masterClient.UnregisterNodeAsync(new WorkerItem() {  Id = _config.Id});
        }
    }
}
