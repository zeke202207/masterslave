using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

public abstract class BaseClient<T>
{
    protected readonly string _host;
    protected readonly string _userName;
    protected readonly string _password;
    protected string _jwtToken;
    protected GrpcChannel _channel;
    protected T _client;

    public string JwtToken => _jwtToken;
    public Action<Exception> Logger;

    public BaseClient(string host, string username, string pwd)
    {
        this._host = host;
        this._userName = username;
        this._password = pwd;
        _jwtToken = string.Empty;
        InitializeClient();
    }

    protected virtual void InitializeClient()
    {
        _channel = GrpcChannel.ForAddress(_host, new GrpcChannelOptions()
        {
            MaxSendMessageSize = int.MaxValue,
            MaxReceiveMessageSize = int.MaxValue,
        });
        _client = CreateClient(_channel);
        //生成jwtToken
        _jwtToken = Login(_userName, _password);
    }

    /// <summary>
    /// 创建grpc客户端
    /// </summary>
    /// <param name="channel"></param>
    /// <returns></returns>
    protected abstract T CreateClient(GrpcChannel channel);

    /// <summary>
    /// 获取Jwt Token字符串
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    protected abstract string Login(string userName, string password);

    /// <summary>
    /// unix时间戳转换windows日期格式
    /// </summary>
    /// <param name="unixTimestamp"></param>
    /// <returns></returns>
    protected virtual DateTime UnixTimestampToDateTime(long unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimestamp);
        return dateTime.ToLocalTime();
    }

    /// <summary>
    /// 获取通信元数据
    /// 统一jwt授权
    /// </summary>
    /// <returns></returns>
    protected virtual Metadata GetMetadata()
    {
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {_jwtToken}");
        return headers;
    }
}
