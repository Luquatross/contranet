using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// This is a bot response module for Oberon.
    /// </summary>
    public class Responses : Plugin
    {
        #region Private Variables
        private string _PluginName = "Responses";
        private string _FolderName = "Extras";

        /// <summary>
        /// Dictionary that holds setting for if responses are enabled for a given 
        /// room. The key is the chatroom, and the value is if it is enabled or not.
        /// </summary>
        private Dictionary<string, bool> ResponsesEnabled
        {
            get
            {
                if (!Settings.ContainsKey("ResponsesEnabled"))
                    Settings["ResponsesEnabled"] = new Dictionary<string, bool>();
                return (Dictionary<string, bool>)Settings["ResponsesEnabled"];
            }
            set
            {
                Settings["ResponsesEnabled"] = value;
            }
        }
        
        /// <summary>
        /// Stores responses. The key is the trigger, and the value is the response.
        /// </summary>
        private Dictionary<string, string> StoredResponses
        {
            get
            {
                if (!Settings.ContainsKey("Responses"))
                    Settings["Responses"] = new Dictionary<string, string>();
                return (Dictionary<string, string>)Settings["Responses"];
            }
            set
            {
                Settings["Responses"] = value;
            }
        }
        #endregion

        #region Public Properties
        public override string PluginName
        {
            get { return _PluginName; }
        }

        public override string FolderName
        {
            get { return _FolderName; }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Determines if responses are enabled for the chatroom. Is true by default.
        /// </summary>
        /// <param name="chatroom">Chatroom to check for.</param>
        /// <returns>True if chatroom has responses enabled, otherwise false.</returns>
        private bool IsResponseEnabled(string chatroom)
        {
            if (ResponsesEnabled.ContainsKey(chatroom))
                return ResponsesEnabled[chatroom];
            else
                return true;
        }

        /// <summary>
        /// Gets a response from the stored responses.
        /// </summary>
        /// <param name="trigger">Trigger to get response for.</param>
        /// <returns>Response.</returns>
        private string GetResponse(string trigger)
        {
            if (StoredResponses.ContainsKey(trigger))
                return StoredResponses[trigger];
            else
                return null;
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // tie into the chat event so we can detect trigger in a messages 
            RegisterEvent(dAmnPacketType.Chat, new BotServerPacketEvent(ChatReceived));

            // register command
            RegisterCommand("response", new BotCommandEvent(Response), new CommandHelp(
                "Use this command to manage your bot's responses.",
                "response add [trigger] [response] - Adds a response for [trigger].<br />" +
                "response remove [trigger] - Removes the response stored for [trigger].<br />" +
                "response (#room) [on / off] - Turns on or off responses in #room<br />" +
                "response list - Shows a list of all responses stored.<br />" +
                "response rooms - List the rooms in which responses are turned off.<br />" +
                "response clear rooms - Turns on rooms that are turned off.<br />" +
                "response clear responses - Deletes all responses<br />" +
                "<i>Optional parameter \"room\" always defaults to the channel you are in.</i>"),
                (int)PrivClassDefaults.Operators);

            // load settings
            LoadSettings();
        }

        public override void Close()
        {
            SaveSettings();
        }
        #endregion

        #region Event Handler
        private void ChatReceived(string chatroom, dAmnServerPacket packet)
        {
            // get relevant data about the packet
            dAmnCommandPacket commandPacket = new dAmnCommandPacket(packet);
            string from = commandPacket.From;            

            // make sure room is enabled before we start processing
            if (!IsResponseEnabled(chatroom))
                return;
            
            // get trigger
            string trigger = ParseArg(commandPacket.Message, 0);
            if (string.IsNullOrEmpty(trigger))
                return;

            // if we have a response, send it
            string response = GetResponse(trigger);
            if (!string.IsNullOrEmpty(response))
            {
                response = response.Replace("{from}", from);
                response = response.Replace("{channel}", chatroom);
                Say(chatroom, response);
            }
        }
        #endregion

        #region Command Handler
        public void Response(string ns, string from, string message)
        {
            string[] args = GetArgs(message);
            string room = ns; 

            // get the room to respond to
            if (args.Length >= 1 && args[0].StartsWith("#"))
            {
                room = args[0];
                // remove the room name from the string and get new args
                int index = message.IndexOf(room);
                message = message.Substring(0, index) + message.Substring(index + room.Length + 1);
                args = GetArgs(message);
            }

            string subCommand = GetArg(args, 0);
            string var1 = GetArg(args, 1);
            string response = ParseArg(message, 2);
            List<string> disabledRooms;

            switch (subCommand)
            {
                case "add":
                    if (StoredResponses.ContainsKey(var1))
                    {
                        Respond(ns, from, var1 + " is already stored as an auto-response.");
                        return;
                    }
                    if (string.IsNullOrEmpty(response))
                    {
                        Respond(ns, from, "You need to give a response to be used!");
                        return;
                    }
                    StoredResponses.Add(var1, response);
                    Respond(ns, from, string.Format("Added response \"{0}\" for \"{1}\".", response, var1));
                    return;
                case "remove":
                    if (!StoredResponses.ContainsKey(var1))
                    {
                        Respond(ns, from, var1 + " is not an auto-response.");
                        return;
                    }
                    StoredResponses.Remove(var1);
                    Respond(ns, from, "Removed auto-response for " + var1 + ".");
                    return;
                case "on":
                    if (ResponsesEnabled.ContainsKey(room))
                        ResponsesEnabled[room] = true;
                    else
                        ResponsesEnabled.Add(room, true);
                    Respond(ns, from, "Respones enabled for " + room + ".");
                    return;
                case "off":
                    if (ResponsesEnabled.ContainsKey(room))
                        ResponsesEnabled[room] = false;
                    else
                        ResponsesEnabled.Add(room, false);
                    Respond(ns, from, "Respones disabled for " + room + ".");
                    break;
                case "list":
                    if (StoredResponses.Count == 0)
                    {
                        Respond(ns, from, "There are currently no responses stored.");
                        return;
                    }
                    StringBuilder list = new StringBuilder("Stored Responses:<sup>");
                    foreach (KeyValuePair<string, string> item in StoredResponses)
                        list.AppendFormat("<br /><b>{0}</b> - {1}", item.Key, item.Value);
                    list.Append("</sup>");
                    Respond(ns, from, list.ToString());
                    break;
                case "rooms":
                    disabledRooms = (from rm in ResponsesEnabled
                                                  where rm.Value == false
                                                  select rm.Key).ToList();
                    if (disabledRooms.Count == 0)
                    {
                        Respond(ns, from, "Responses are not disabled in any rooms.");
                        return;
                    }
                    StringBuilder rooms = new StringBuilder("Responses are disabled in the following rooms:<br /><sup>");                    
                    foreach (string r in disabledRooms)
                        rooms.Append(r + ", ");
                    Respond(ns, from, rooms.ToString().Trim().Trim(','));
                    break;
                case "clear":
                    disabledRooms = (from rm in ResponsesEnabled
                                     where rm.Value == false
                                     select rm.Key).ToList();
                    switch (var1)
                    {
                        case "rooms":
                            if (disabledRooms.Count == 0)
                            {
                                Respond(ns, from, "Responses are not disabled in any rooms.");
                                return;
                            }
                            if (string.IsNullOrEmpty(response) || response != "yes")
                            {
                                Respond(ns, from, "This will re-enable responses in all rooms. Type \"<code>response clear rooms yes</code>\" to confirm.");
                                return;
                            }
                            ResponsesEnabled.Clear();
                            Respond(ns, from, "Responses re-enabled for all rooms.");
                            return;
                        case "responses":
                            if (StoredResponses.Count == 0)
                            {
                                Respond(ns, from, "There are no responses stored.");
                                return;
                            }
                            if (string.IsNullOrEmpty(response) || response != "yes")
                            {
                                Respond(ns, from, "This will delete all stored responses. Type \"<code>response clear responses yes</code>\" to confirm.");
                                return;
                            }
                            StoredResponses.Clear();
                            return;
                        default:
                            ShowHelp(ns, from, "response");
                            return;
                    }
                    break;
            }
        }
        #endregion
    }
}
