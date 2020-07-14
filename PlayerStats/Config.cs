using System.ComponentModel;
using Exiled.API.Interfaces;

namespace PlayerStats
{
    public class Config : IConfig
    {
        [Description("Wether or not the plugin is enabled.")]
        public bool IsEnabled { get; set; }
    }
}