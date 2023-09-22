using Google.Protobuf;
using Grpc.Core;
using MasterSDKService;
using System.Collections.Concurrent;

namespace NetX.Master
{
    public class DataTransferCenter
    {
        private BlockingCollection<ResultModel> _results = new BlockingCollection<ResultModel>();
        private ConcurrentBag<DataTransferResultConsumer> _consumers = new ConcurrentBag<DataTransferResultConsumer>();

        public DataTransferCenter()
        {
            Task.Factory.StartNew(() =>
            {

                foreach (var result in _results.GetConsumingEnumerable())
                {
                    try
                    {
                        var consumer = _consumers.FirstOrDefault(p=>p.JobId == result.JobId);
                        if (null == consumer)
                            continue;
                        consumer.StreamWriter.WriteAsync(new GetJobResultResponse() { Result = ByteString.CopyFrom(result.Result) });
                        consumer.TokenSource.Cancel();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });
        }


        public void Regist(DataTransferResultConsumer consumer)
        {
            _consumers.Add(consumer);
        }

        public void UnRegist(DataTransferResultConsumer consumer)
        {
            _consumers.TryTake(out _);
        }

        public void ProcessData(ResultModel result)
        {
            _results.TryAdd(result);
        }
    }

    public class ResultModel
    {
        public string JobId { get; set; }

        public byte[] Result { get;set; }
    }

    public class DataTransferResultConsumer
    {
        public string JobId { get; set; }

        public CancellationTokenSource TokenSource { get; set; }

        public IServerStreamWriter<GetJobResultResponse> StreamWriter { get; set; }
    }
}
