using Dalamud.Configuration;

namespace Telesto
{
    
    public class Config : IPluginConfiguration
    {

        public int Version { get; set; } = 0;

        public bool Opened { get; set; } = true;

        public bool AutostartEndpoint { get; set; } = true;        
        public string HttpEndpoint { get; set; } = "http://localhost:45678/";
        public bool DismissUpgrade { get; set; } = false;

    }

}
