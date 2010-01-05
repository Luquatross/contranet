using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using DeviantArt.Chat.Oberon.Plugins.ChartLyrics;
using System.ServiceModel;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;

namespace DeviantArt.Chat.Oberon.Plugins
{
    public class LastFm : Plugin
    {
        #region Private Variables
        private string _PluginName = "Last.fm";
        private string _FolderName = "LastFm";

        /// <summary>
        /// LastFm base url
        /// </summary>
        private const string LastFmBaseUrl = "http://ws.audioscrobbler.com/2.0/?method=";

        /// <summary>
        /// API Key to use during requests.
        /// </summary>
        private const string LastFmApiKey = "0aff44a1cb9e80a073823bc16fdc1236";

        /// <summary>
        /// URL to the ChartLyrics API.
        /// </summary>
        private const string ChartLyricsUrl = "http://api.chartlyrics.com/apiv1.asmx";
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
        /// Returns a fully formed API url.
        /// </summary>
        /// <param name="method">Method to execute.</param>
        /// <param name="arguments">Arguments to pass.</param>
        /// <returns>URL string.</returns>
        private string ConstructUrl(string method, NameValueCollection arguments)
        {
            // construct url and query variables
            StringBuilder url = new StringBuilder(LastFmBaseUrl + method);
            if (arguments != null)
            {
                foreach (string key in arguments.Keys)
                    url.Append("&" + key + "=" + arguments[key]);
                url.Append("&api_key=" + LastFmApiKey);
            }
            return url.ToString();
        }

        /// <summary>
        /// Gets xml REST response from Last.fm service.
        /// </summary>
        /// <param name="url">URl to request xml from.</param>
        /// <returns>Xml string of data.</returns>
        private string GetLastFmXml(string url)
        {
            // create request and get response
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                WebResponse response = request.GetResponse();

                // get string from response
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                // if it was a bad request, load the error xml
                return new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
        }

        /// <summary>
        /// Returns results for a lyric search.
        /// </summary>
        /// <param name="lyrics">Lyrics to search for.</param>
        /// <returns>Search results.</returns>
        private SearchLyricResult[] GetSongsForLyrics(string lyrics)
        {
            apiv1SoapClient client = new apiv1SoapClient(
                new BasicHttpBinding(BasicHttpSecurityMode.None),
                new EndpointAddress(ChartLyricsUrl));
            return client.SearchLyricText(lyrics);
        }

        /// <summary>
        /// Returns artist metadata.
        /// </summary>
        /// <param name="artist">Artist to search for.</param>
        /// <returns>Query results.</returns>
        private XmlDocument GetArtistMetaData(string artist)
        {
            NameValueCollection arguments = new NameValueCollection
            {
                { "artist", artist }
            };

            string xml = GetLastFmXml(ConstructUrl("artist.getinfo", arguments));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// Returns album metadata.
        /// </summary>
        /// <param name="artist">Artist to search for. Optional.</param>
        /// <param name="album">Album to searh for.</param>
        /// <returns>Query Results</returns>
        private XmlDocument GetAlbumMetaData(string artist, string album)
        {
            NameValueCollection arguments = new NameValueCollection();
            if (!string.IsNullOrEmpty(artist))
                arguments.Add("artist", artist);
            if (!string.IsNullOrEmpty(album))
                arguments.Add("album", album);

            string xml = GetLastFmXml(ConstructUrl("album.getinfo", arguments));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
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
            string artist = message;

            // make the request
            XmlDocument result = GetArtistMetaData(artist);
            XmlNode root = result.DocumentElement;                        
            if (result.DocumentElement.Attributes["status"].Value == "ok")
            {
                StringBuilder say = new StringBuilder();

                // form artist data
                say.Append(string.Format(
                    "<b><a href=\"{0}\">{1}</a></b><br /><sub><b>Play count:</b> {2}<br /><b>Listeners:</b> {3}",
                    root.SelectSingleNode("artist/url").InnerText,
                    root.SelectSingleNode("artist/name").InnerText,
                    root.SelectSingleNode("artist/stats/playcount").InnerText,
                    root.SelectSingleNode("artist/stats/listeners").InnerText));

                // if there are similar artists, add them
                XmlNodeList similarArtists = root.SelectNodes("artist/similar/artist");
                if (similarArtists.Count > 0)                    
                {
                    say.Append("<br /><b>Simiar artists:</b> ");
                    string similarString = "<a href=\"{0}\">{1}</a>";
                    List<string> similars = new List<string>();
                    foreach (XmlNode similar in similarArtists)
                    {
                        string name = similar.SelectSingleNode("name").InnerText;
                        string url = similar.SelectSingleNode("url").InnerText;
                        similars.Add("<a href=\"" + url + "\">" + name + "</a>");
                    }
                    say.Append(string.Join(",", similars.ToArray()));
                }

                // if there is a summary, add it
                XmlNode summary = root.SelectSingleNode("artist/bio/summary");
                if (summary != null)
                    say.Append("<br /><b>Info:</b> " + summary.InnerText);                

                // send it off!
                say.Append("</sub>");
                Say(ns, say.ToString());
            }
            else
            {
                // show the user the error message
                Respond(ns, from, root.SelectSingleNode("error").InnerText);
            }
        }
        #endregion
    }
}
