using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Small chat room games.
    /// </summary>
    public class Games : Plugin
    {
        #region Private Variables
        private string _PluginName = "Games";
        private string _FolderName = "Extras";
        private XmlDocument _GameConfig;
        private string[] RockPaperScissors = { "rock", "paper", "scissors" };

        /// <summary>
        /// Random number generator
        /// </summary>
        private Random Randomizer = new Random();

        /// <summary>
        /// Game config file.
        /// </summary>
        private XmlDocument GameConfig
        {
            get
            {
                if (_GameConfig == null)
                {                    
                    _GameConfig = new XmlDocument();
                    _GameConfig.Load(System.IO.Path.Combine(PluginPath, "Games.config"));                    
                }
                return _GameConfig;
            }
        }

        /// <summary>
        /// Chuck Norris responses.
        /// </summary>
        private List<string> ChuckNorrisResponses
        {
            get
            {
                if (!Settings.ContainsKey("ChuckNorrisResponses"))
                    Settings["ChuckNorrisResponses"] = GetChuckNorrisDefaults();
                return (List<string>)Settings["ChuckNorrisResponses"];
            }
            set
            {
                Settings["ChuckNorrisResponses"] = value;
            }
        }

        /// <summary>
        /// Fortune cookie responses.
        /// </summary>
        private List<string> FortuneCookieResponses
        {
            get
            {
                if (!Settings.ContainsKey("FortuneCookieResponses"))
                    Settings["FortuneCookieResponses"] = GetFortuneDefaults();
                return (List<string>)Settings["FortuneCookieResponses"];
            }
            set
            {
                Settings["FortuneCookieResponses"] = value;
            }
        }

        /// <summary>
        /// Eight ball responses.
        /// </summary>
        private List<string> EightBallResponses
        {
            get
            {
                if (!Settings.ContainsKey("EightBallResponses"))                
                    Settings["EightBallResponses"] = GetEightBallDefaults();                
                return (List<string>)Settings["EightBallResponses"];
            }
            set
            {
                Settings["EightBallResponses"] = value;
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
        /// Get the default strings for chuck norris responses.
        /// </summary>
        /// <returns></returns>
        private List<string> GetChuckNorrisDefaults()
        {
            return ReadList("ChuckNorrisResponses");
        }

        /// <summary>
        /// Get default strings for 8-ball responses.
        /// </summary>        
        private List<string> GetEightBallDefaults()
        {
            return ReadList("EightBallResponses");
        }

        /// <summary>
        /// Get default strings for Fortune cookie responses.
        /// </summary>        
        private List<string> GetFortuneDefaults()
        {
            return ReadList("FortuneResponses");
        }

        /// <summary>
        /// Read list of values from xml file.
        /// </summary>
        /// <param name="nodeName">Node to get values from.</param>
        /// <returns>List.</returns>
        private List<string> ReadList(string nodeName)
        {
            List<string> result = new List<string>();
            XmlNodeList adds = GameConfig.DocumentElement.SelectNodes(nodeName + "/add");
            foreach (XmlNode node in adds)
            {
                result.Add(node.Attributes["value"].Value);
            }
            return result;
        }
        #endregion

        #region Plugin Methods
        public override void Load()
        {
            // register game commands
            RegisterCommand("8ball", new BotCommandEvent(EightBall), new CommandHelp(
                "Ask the 8ball a question",
                "8ball [my question]"), (int)PrivClassDefaults.Guests);
            RegisterCommand("fortune", new BotCommandEvent(FortuneCookie), new CommandHelp(
                "Get a fortune cookie!",                
                "fortune"), (int)PrivClassDefaults.Guests);
            RegisterCommand("chucknorris", new BotCommandEvent(ChuckNorris), new CommandHelp(
                "Gets a chuck norris fact.",
                "chucknorris"), (int)PrivClassDefaults.Guests);
            RegisterCommand("rr", new BotCommandEvent(RussianRoulette), new CommandHelp(
                "Play Russian Roulette, if you dare!",
                "rr"), (int)PrivClassDefaults.Guests);
            RegisterCommand("shoot", new BotCommandEvent(Shoot), new CommandHelp(
                "Play rock paper scissors against me!",
                "shoot [rock | paper | scissors]"), (int)PrivClassDefaults.Guests);
            RegisterCommand("games", new BotCommandEvent(GamesList), new CommandHelp(
                "Get info about my games!",
                "games - info about this game plugin<br />" +
                "games list - see a list of available games"), (int)PrivClassDefaults.Guests);

            // load settings
            LoadSettings();
        }

        public override void Close()
        {
            // save game settings
            SaveSettings();
        }
        #endregion

        #region Command Handlers
        private void EightBall(string ns, string from, string message)
        {
            string question = message;
            if (string.IsNullOrEmpty(question))
            {
                Respond(ns, from, "Give a question to the 8ball!");
                return;
            }

            // strip question mark if it's there
            question = question.TrimEnd('?');

            // pick random response and send it 
            int randomIndex = Randomizer.Next(EightBallResponses.Count);
            Respond(ns, from, string.Format(
                "You said '<i>{0}?</i>' I say '<b>{1}</b>'",
                question,
                EightBallResponses[randomIndex]));
        }

        private void FortuneCookie(string ns, string from, string message)
        {
            // pick random and send it
            int randomIndex = Randomizer.Next(FortuneCookieResponses.Count);
            Respond(ns, from, string.Format("Confucious say: <b>{0}</b>",
                FortuneCookieResponses[randomIndex]));
        }

        private void ChuckNorris(string ns, string from, string message)
        {
            // pick random and sent it
            int randomIndex = Randomizer.Next(ChuckNorrisResponses.Count);
            Say(ns, "<b>" + ChuckNorrisResponses[randomIndex] + "</b>");
        }

        private void RussianRoulette(string ns, string from, string message)
        {
            int spin = Randomizer.Next(1, 6);
            int bullet = Randomizer.Next(1, 6);
            Respond(ns, from, "You place a bullet in the :gun: and spin it around. Then you put it to your head, and pull...");
            if (spin == bullet)
                Respond(ns, from, "BLAM! You lose.");
            else
                Respond(ns, from, ":phew: You're alright.");
        }

        private void Shoot(string ns, string from, string message)
        {
            string motion = RockPaperScissors[Randomizer.Next(3)];
            string say = string.Empty;
            switch (message.ToLower())
            {
                case "rock":
                    if (motion == "rock") say = "Rock ties rock.";
                    if (motion == "paper") say = "Paper covers rock. You lose.";
                    if (motion == "scissors") say = "Rock smashes scissors. You win.";
                    break;
                case "paper":
                    if (motion == "rock") say = "Paper covers rock. You win.";
                    if (motion == "paper") say = "Paper ties paper.";
                    if (motion == "scissors") say = "Scissors cut paper. You lose.";
                    break;
                case "scissors":
                    if (motion == "rock") say = "Rock smashes scissors. You lose.";
                    if (motion == "paper") say = "Scissors cut paper. You win.";
                    if (motion == "scissors") say = "Scissors tie scissors.";
                    break;
                default:
                    ShowHelp(ns, from, "shoot");
                    return;
            }
            Respond(ns, from, say);
        }

        private void GamesList(string ns, string from, string message)
        {
            if (message.ToLower() == "list")
                Respond(ns, from, "Available games;<br/><sub>- 8ball, fortune, rr, shoot</sub>");
            else
                Respond(ns, from, "This module just contains a few small games for you to mess around with!");
        }
        #endregion
    }
}
