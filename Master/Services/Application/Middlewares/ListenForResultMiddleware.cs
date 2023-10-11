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
        await foreach (var resp in context.Reqeust.RequestStream.ReadAllAsync())
        {
            _resultDispatcher.WriteResult(new ResultModel()
            {
                JobId = resp.Id,
                Result = resp.Result.ToByteArray(),
            });
        }
    }
}
