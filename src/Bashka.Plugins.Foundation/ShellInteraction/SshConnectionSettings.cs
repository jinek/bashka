using IniParser;
using IniParser.Model;
using Renci.SshNet;

namespace Bashka.Plugins.Foundation.ShellInteraction;

public record SshConnectionSettings(string Host, int Port, string Username, string Password)
{
    private const string SectionName = "Connection";

    public static SshConnectionSettings Load(string filePath)
    {
        IniData iniData = new FileIniDataParser().ReadFile(filePath);
        KeyDataCollection keys = iniData[SectionName];
        return new SshConnectionSettings(
            keys["Host"],
            int.TryParse(keys["Port"], out int port) ? port : 22,
            keys["Username"],
            keys["Password"]);
    }

    public static void CreateDefault(string filePath)
    {
        var iniData = new IniData();
        iniData[SectionName]["Host"] = string.Empty;
        iniData[SectionName]["Port"] = "22";
        iniData[SectionName]["Username"] = string.Empty;
        iniData[SectionName]["Password"] = string.Empty;
        new FileIniDataParser().WriteFile(filePath, iniData);
    }

    public ConnectionInfo ToConnectionInfo()
    {
        return new ConnectionInfo(Host, Port, Username, new PasswordAuthenticationMethod(Username, Password));
    }
}