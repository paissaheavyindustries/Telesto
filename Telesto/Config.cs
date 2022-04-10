using Dalamud.Configuration;

namespace Telesto
{
    
    public class Config : IPluginConfiguration
    {

        public int Version { get; set; } = 0;

        public bool AutostartEndpoint { get; set; } = true;

        public string HttpEndpoint { get; set; } = "http://localhost:51323/";

    }

}
