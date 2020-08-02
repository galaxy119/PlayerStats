using System.ComponentModel;
using Exiled.API.Interfaces;

namespace PlayerStats
{
    public class Config : IConfig
    {
        [Description("Whether or not the plugin is enabled.")]
        public bool IsEnabled { get; set; }
        
        [Description("Whether or not debug mode should be used.")]
        public bool Debug { get; set; }
    }
}