namespace OpenSyno
{
    public interface IOpenSynoSettings
    {        
        string Token { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        string Host { get; set; }
        int Port { get; set; }
    }
}