using Grpc.Core;
using MasterWorkerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class ListenForResultMiddleware : IApplicationMiddleware<GrpcContext<ListenForResultRequest, ListenForResultReponse>>
{
    private readonly IResultDispatcher _resultDispatcher;

    public ListenForResultMiddleware(IResultDispatcher dataTransferCenter)
    { 
        _resultDispatcher = dataTransferCenter;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<ListenForResultRequest, ListenForResultReponse>> next, GrpcContext<ListenForResultRequest, ListenForResultReponse> context)
    {
        List<ResultModel> list= new List<ResultModel>();
        string jobId = string.Empty;
        await foreach (var resp in context.Reqeust.RequestStream.ReadAllAsync())
        {
            if(string.IsNullOrWhiteSpace(jobId))
                jobId = resp.Id;
            list.Add(new ResultModel()
            {
                JobId = resp.Id,
                Result = resp.Result.ToByteArray(),
            });
        }
        //合并上传
        var resultByte = MergeResult(list);
        _resultDispatcher.WriteResult(new ResultModel()
        {
            JobId = jobId,
            Result = resultByte,
        });
    }

    /// <summary>
    /// 合并数组
    /// </summary>
    /// <param name="resultList"></param>
    /// <returns></returns>
    private byte[] MergeResult(List<ResultModel> resultList)
    {
        int totalLength = resultList.Sum(r => r.Result.Length);
        byte[] mergedArray = new byte[totalLength];
        int offset = 0;
        foreach (ResultModel resultModel in resultList)
        {
            byte[] result = resultModel.Result;
            result.AsSpan().CopyTo(mergedArray.AsSpan(offset));
            offset += result.Length;
        }
        return mergedArray;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arrays"></param>
    /// <returns></returns>
    private byte[] MergeResult(params byte[][] arrays)
    {
        int totalLength = arrays.Sum(a => a.Length);
        byte[] mergedArray = new byte[totalLength];
        int offset = 0;
        foreach (byte[] array in arrays)
        {
            array.AsSpan().CopyTo(mergedArray.AsSpan(offset));
            offset += array.Length;
        }
        return mergedArray;
    }
}
