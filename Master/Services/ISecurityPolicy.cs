namespace NetX.Master
{
    public interface ISecurityPolicy
    {
        bool IsRequestAllowed(SecurityContext context);
    }

    public class SecurityContext
    {
        public string ClientIp { get; set; }
        // Add other properties as needed, for example:
        // public string OAuthToken { get; set; }
    }
}
