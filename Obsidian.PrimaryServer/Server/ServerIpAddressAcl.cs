namespace Obsidian.PrimaryServer.Server;

public class ServerIpAddressAcl
{
    private bool IsAccessControlEnabled()
    {
        return false;
    }

    public bool IpClientIpAddressAllowed(string address)
    {
        if (!IsAccessControlEnabled()) return true;

        return true;
    }
}
