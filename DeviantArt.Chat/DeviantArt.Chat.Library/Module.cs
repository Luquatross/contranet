using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Library
{
    public abstract class Module
    {
        // variables to access parts of the core
        protected Bot Bot;
        protected Console Console;
        protected Timer Timer;
        protected bool IsLoading = false;

        public Module(Bot bot)
        {
            Bot = bot;
            Console = bot.Console;
            // dAmn = bot.dAmn;
            Timer = bot.Timer;
            // User = bot.User;
            // Now we're loading the module!
            IsLoading = true;
            // Call the init method so the module can be configured properly.
            Init();
            // Call the load method to verify the load
            Load();
            // Mark that loading is complete
            IsLoading = false;
        }

        protected abstract void Init();

        protected void Load()
        {

        }
    }
}
