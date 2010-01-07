using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Base class for core plugins.
    /// </summary>
    public abstract class CorePluginBase : Plugin
    {
        public CorePluginBase() { }

        /// <summary>
        /// Load override - sets plugin status to On.
        /// </summary>
        public override void Load()
        {
            // turn status to on, since it is required for the bot to operate.
            Status = PluginStatus.On;
        }
    }
}
