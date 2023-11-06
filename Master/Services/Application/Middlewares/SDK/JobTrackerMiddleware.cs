using NetX.Common;
using NetX.Master.Services.Core;
using SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class JobTrackerMiddleware : IApplicationMiddleware<GrpcContext<JobTrackerRequest, JobTrackerResponse>>
{
    private readonly IJobTrackerCache<JobTrackerItem> _jobTrackerCache;

    public JobTrackerMiddleware(IJobTrackerCache<JobTrackerItem> jobTrackerCache)
    {
        _jobTrackerCache = jobTrackerCache;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<JobTrackerRequest, JobTrackerResponse>> next, GrpcContext<JobTrackerRequest, JobTrackerResponse> context)
    {
        try
        {
            var items = await _jobTrackerCache.GetLatestAsync(10);
            foreach (var item in items)
            {
                context.Response.Response.JobTracker.Add(new JobTracker()
                {
                    JobId = item.JobId,
                    Status = item.Status.ToString(),
                    NodeId = item.NodeId,
                    NodeName = "",
                    StartTime = item.StartTime.DateTimeToUnixTimestamp(),
                    EndTime = item.EndTime.DateTimeToUnixTimestamp(),
                    Message = item.Message ?? ""
                });
            }
            context.Response.Response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            context.Response.Response.IsSuccess = false;
            context.Response.Response.ErrorMessage = ex.Message;
        }
    }
}
