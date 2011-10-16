namespace Forge.MountFTP
{
    interface IFtpOptions
    {
        string HostName { get; set; }
        string Password { get; set; }
        string UserName { get; set; }
    }
}