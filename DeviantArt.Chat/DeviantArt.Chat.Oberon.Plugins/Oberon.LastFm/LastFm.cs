using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LastFmLib.WebRequests;

namespace DeviantArt.Chat.Oberon.Plugins
{
    public class LastFm : Plugin
    {
        #region Private Variables
        private string _PluginName = "Last.fm";
        private string _FolderName = "LastFm";        
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

        #region Plugin Methods
        public override void Load()
        {
            RegisterCommand("artist", new BotCommandEvent(Artist), new CommandHelp(
                "Gets info for an artist",
                "artist [artist] - returns info on the artist given."), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Last.fm Methods
        private void Artist(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                ShowHelp(ns, from, "artist");
                return;
            }

            // make the request
            ArtistMetadataRequest request = new ArtistMetadataRequest(message);
            try
            {
                request.Start();
            }
            catch (Exception ex)
            {
                Bot.Console.Log("Error fetching last.fm artist data. " + ex.ToString());
                Respond(ns, from, "unable to find artist.");
                return;
            }

            // show request results
            if (request.succeeded)
            {
                StringBuilder say = new StringBuilder();

                // form artist data
                say.Append(string.Format(
                    "<b><a href=\"{0}\">{1}</a></b><br /><sub><b>Play count:</b> {2}<br /><b>Listeners:</b> {3}<br /><b>Average plays/listeners:</b> {4}",
                    request.Metadata.artistPageUrl,
                    request.Metadata.ArtistName,
                    request.Metadata.PlayCount,
                    request.Metadata.numListeners,
                    request.Metadata.numPlays));

                // if there are similar artists, add them
                if (request.Metadata.similarArtists != null &&
                    request.Metadata.similarArtists.Count > 0)
                {
                    say.Append("<br /><b>Simiar artists:</b> ");
                    string similarString = "<a href=\"{0}\">{1}</a>";
                    List<string> similars = new List<string>();
                    foreach (string similar in request.Metadata.similarArtists)
                        similars.Add(similar);
                    say.Append(string.Join(",", similars.ToArray()));
                }

                // if there is a summar, add it
                //if (request.Metadata.
                say.Append("</sub>");

                // send it off!
                Say(ns, say.ToString());
            }
            else
            {
                Respond(ns, from, "unable to find artist.");
            }
        }
        #endregion
    }
}
