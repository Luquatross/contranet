using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon.Plugins;

namespace MyPlugin
{
    /// <summary>
    /// Hello world!
    /// </summary>
    public class HelloWorld : Plugin
    {
        public override string PluginName
        {
            get { return "Hello World"; }
        }

        public override string FolderName
        {
            get { return "HelloWorld"; }
        }

        public override void Load()
        {            
            RegisterCommand("hello", new BotCommandEvent(Hello), new CommandHelp(
                "My first plugin!.", "hello"), (int)PrivClassDefaults.Guests);
        }

        private void Hello(string ns, string from, string message)
        {
            Say(ns, "Hello world!");
        }
    }
}