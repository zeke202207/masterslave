using NetX.MasterSDK;

namespace NetX.Monitor
{
    public class CommunicationService
    {
        private MonitorClient _client;
        private bool _isConnected = false;
        private string _username = "zeke";
        private string _password = "zeke@123!";

        public bool IsConnected => _isConnected;

        public CommunicationService()
        {

        }

        /// <summary>
        /// 连接到master
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectToMaster(ConnectionModel connectionModel)
        {
            try
            {
                var factory = new MasterSDKFactory($"http://{connectionModel.Ip}:{connectionModel.Port}", _username, _password);
                _client = factory.MasterMonitorClient();
                return await _client.Connect();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取所有的worker节点
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<WorkerNode>> GetWorkersAsync()
        {
            try
            {
                return await _client.GetWorkersAsync();
            }
            catch (Exception ex)
            {
                return default(IEnumerable<WorkerNode>);
            }
        }

        public async Task<WorkerNodeInfo> GetWorkerNodeInfoAsync(string nodeId)
        {
            try
            {
                return await _client.GetWorkerNodeInfoAsync(nodeId);
            }
            catch (Exception ex)
            {
                return default(WorkerNodeInfo);
            }
        }

        public async Task<IEnumerable<JobTrackerModel>> GetJobTracker(string nodeId)
        {
            try
            {
                return await _client.GetJobTracker(nodeId);
            }
            catch (Exception ex)
            {
                return default(IEnumerable<JobTrackerModel>);
            }
        }
    }
}
