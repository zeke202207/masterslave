namespace NetX.Monitor;

public class ConnectionModel
{
    public string Ip { get; set; }

    public string Port { get; set; }

    public ConnectionModel(string ip, string port)
    {
        Ip = ip;
        Port = port;
    }
}