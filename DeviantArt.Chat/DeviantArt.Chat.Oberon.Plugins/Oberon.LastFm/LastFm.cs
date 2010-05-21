using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;
using DeviantArt.Chat.Library;
using DeviantArt.Chat.Oberon.Plugins.LyricsWiki;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Class that allows the user to have some Last.fm functionality in the chatroom.
    /// Namely, looking up songs, albums, and lyrices.
    /// </summary>
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
        /// URL to the LyricsWiki API.
        /// </summary>
        private const string LyricsWikiUrl = "http://lyrics.wikia.com/server.php";

        /// <summary>
        /// LyricsWiki SOAP client.
        /// </summary>
        private LyricWikiPortTypeClient LyricWikiClient
        {
            get
            {
                if (_Client == null)
                    _Client = new LyricWikiPortTypeClient(
                        new BasicHttpBinding(BasicHttpSecurityMode.None),
                        new EndpointAddress(LyricsWikiUrl));
                return _Client;
            }
        }
        private LyricWikiPortTypeClient _Client;        
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
                request.Timeout = 30000; // 30 seconds
                WebResponse response = request.GetResponse();

                // get string from response
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException ex)
            {
                // if it was a time out, rethrow it, as we have no xml
                if (ex.Status == WebExceptionStatus.Timeout)
                    throw new TimeoutException("Last.fm is being slow right now. Try again in a second.", ex);

                // if it was a bad request, load the error xml
                return new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
        }

        /// <summary>
        /// Retrieves lyrics for a song.
        /// </summary>
        private LyricsResult GetLyricsForSong(string artist, string song)
        {
            return LyricWikiClient.getSong(artist, song);
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
        /// Returns track metadata.
        /// </summary>
        /// <param name="artist">Artist to search for.</param>
        /// <param name="track">Track to search for.</param>
        /// <returns>Query results</returns>
        private XmlDocument GetTrackMetaData(string artist, string track)
        {
            NameValueCollection arguments = new NameValueCollection();
            if (!string.IsNullOrEmpty(artist))
                arguments.Add("artist", artist);
            if (!string.IsNullOrEmpty(track))
                arguments.Add("track", track);

            string xml = GetLastFmXml(ConstructUrl("track.getinfo", arguments));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        /// <summary>
        /// Returns album metadata.
        /// </summary>
        /// <param name="artist">Artist to search for.</param>
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
                "Gets info for a musical artist",
                "artist [artist name] - returns info on the artist specified."), (int)PrivClassDefaults.Guests);
            RegisterCommand("album", new BotCommandEvent(Album), new CommandHelp(
                "Gets info for a musical album",
                "album [artist name] - [album name] - returns info on the album specified."), (int)PrivClassDefaults.Guests);
            RegisterCommand("song", new BotCommandEvent(Song), new CommandHelp(
                "Gets info for a song",
                "song [artist] - [title] - returns info on the track given."), (int)PrivClassDefaults.Guests);
            RegisterCommand("lyrics", new BotCommandEvent(Lyrics), new CommandHelp(
                "Uses LyricsWiki to retrieve sampling of lyrics for a song.",
                "lyrics [artist] - [title] - returns results if there are any"), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Last.fm Methods
        private void Artist(string ns, string from, string message)
        {
            string artist = message;
            if (string.IsNullOrEmpty(artist))
            {
                ShowHelp(ns, from, "artist");
                return;
            }            

            // make the request
            try
            {
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
                        List<string> similars = new List<string>();
                        foreach (XmlNode similar in similarArtists)
                        {
                            string name = similar.SelectSingleNode("name").InnerText;
                            string url = similar.SelectSingleNode("url").InnerText;
                            similars.Add("<a href=\"" + url + "\">" + name + "</a>");
                        }
                        say.Append(string.Join(", ", similars.ToArray()));
                    }

                    // if there is a summary, add it
                    XmlNode summary = root.SelectSingleNode("artist/bio/summary");
                    if (summary != null)
                        say.Append("<br /><b>Info:</b> " + StringHelper.StripTags(summary.InnerText));

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
            catch (TimeoutException ex)
            {
                // last.fm is taking too long - let user know
                Respond(ns, from, ex.Message);
            }
        }

        private void Album(string ns, string from, string message)
        {
            string[] args = message.Split('-');
            if (args.Length != 2)
            {
                ShowHelp(ns, from, "album");
                return;
            }

            string artist = args[0].Trim();
            string album = args[1].Trim();

            // make the request
            XmlDocument result = GetAlbumMetaData(artist, album);
            XmlNode root = result.DocumentElement;
            if (result.DocumentElement.Attributes["status"].Value == "ok")
            {
                StringBuilder say = new StringBuilder();

                // form artist data
                say.Append(string.Format(
                    "<b><a href=\"{0}\">{1}</a></b> by <b><a href=\"http://www.last.fm/music/{2}\">{3}</a></b><br/><sub>",
                    root.SelectSingleNode("album/url").InnerText,
                    root.SelectSingleNode("album/name").InnerText,
                    root.SelectSingleNode("album/artist").InnerText,
                    root.SelectSingleNode("album/artist").InnerText));

                // add release data
                XmlNode releaseDate = root.SelectSingleNode("album/releasedate");
                if (releaseDate != null)
                    say.Append("<b>Released:</b> " + releaseDate.InnerText + "<br />");

                // add play count
                say.Append(string.Format("<b>Play count:</b> {0}<br /><b>Listeners:</b> {1}",
                    root.SelectSingleNode("album/playcount").InnerText,
                    root.SelectSingleNode("album/listeners").InnerText));
                
                // add tags
                XmlNodeList topTags = root.SelectNodes("album/toptags/tag");
                if (topTags.Count > 0)
                {
                    say.Append("<br /><b>Tags:</b> ");                    
                    List<string> tags = new List<string>();
                    foreach (XmlNode topTag in topTags)
                    {
                        string name = topTag.SelectSingleNode("name").InnerText;
                        string url = topTag.SelectSingleNode("url").InnerText;
                        tags.Add("<a href=\"" + url + "\">" + name + "</a>");
                    }
                    say.Append(string.Join(", ", tags.ToArray()));
                }

                // if there is a summary, add it
                XmlNode summary = root.SelectSingleNode("album/wiki/summary");
                if (summary != null)
                {
                    string summaryString = StringHelper.StripTags(summary.InnerText);
                    summaryString = (summaryString.Length > 500) ? summaryString.Remove(500) + "..." : summaryString;
                    say.Append("<br /><b>Info:</b> " + summaryString);
                }

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

        private void Song(string ns, string from, string message)
        {
            string[] args = message.Split('-');
            if (args.Length != 2)
            {
                ShowHelp(ns, from, "song");
                return;
            }

            string artist = args[0].Trim();
            string song = args[1].Trim();

            // make the request
            XmlDocument result = GetTrackMetaData(artist, song);
            XmlNode root = result.DocumentElement;
            if (result.DocumentElement.Attributes["status"].Value == "ok")
            {
                StringBuilder say = new StringBuilder();

                // add song info
                say.AppendFormat("<b><a href=\"{0}\">{1}</a></b> by <b><a href=\"{2}\">{3}</a></b>:<br /><sub>",
                    root.SelectSingleNode("track/url").InnerText,
                    root.SelectSingleNode("track/name").InnerText,
                    root.SelectSingleNode("track/artist/url").InnerText,
                    root.SelectSingleNode("track/artist/name").InnerText);

                // album title
                XmlNode title = root.SelectSingleNode("track/album/title");
                if (title != null)
                    say.AppendFormat("<b>Album:</b> <a href=\"{0}\">{1}</a><br />",
                        root.SelectSingleNode("track/album/url").InnerText,
                        title.InnerText);

                // track position
                XmlNode album = root.SelectSingleNode("track/album");
                if (album != null)
                    say.AppendFormat("<b>Track Number:</b> {0}<br />", album.Attributes["position"].Value);

                // get track details
                decimal min = System.Math.Floor(Convert.ToDecimal(root.SelectSingleNode("track/duration").InnerText) / 60000);
                decimal sec = System.Math.Round((Convert.ToDecimal(root.SelectSingleNode("track/duration").InnerText) / 60000 - min) * 60);
                say.AppendFormat("<b>Length</b> {0}:{1}<br />",
                    min.ToString().PadLeft(2, '0'),
                    sec.ToString().PadLeft(2, '0'));

                // add play count
                say.AppendFormat("<b>Play count:</b> {0}<br /><b>Listeners:</b> {1}",
                    root.SelectSingleNode("track/playcount").InnerText,
                    root.SelectSingleNode("track/listeners").InnerText);

                // add tags
                XmlNodeList topTags = root.SelectNodes("track/toptags/tag");
                if (topTags.Count > 0)
                {
                    say.Append("<br /><b>Tags:</b> ");
                    List<string> tags = new List<string>();
                    foreach (XmlNode topTag in topTags)
                    {
                        string name = topTag.SelectSingleNode("name").InnerText;
                        string url = topTag.SelectSingleNode("url").InnerText;
                        tags.Add("<a href=\"" + url + "\">" + name + "</a>");
                    }
                    say.Append(string.Join(", ", tags.ToArray()));
                }

                // if there is a summary, add it
                XmlNode summary = root.SelectSingleNode("track/wiki/summary");
                if (summary != null)
                {
                    string summaryString = StringHelper.StripTags(summary.InnerText);
                    summaryString = (summaryString.Length > 500) ? summaryString.Remove(500) + "..." : summaryString;
                    say.Append("<br /><b>Info:</b> " + summaryString);
                }

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

        private void Lyrics(string ns, string from, string message)
        {
            string[] args = message.Split('-');
            if (args.Length != 2)
            {
                ShowHelp(ns, from, "lyrics");
                return;
            }

            string artist = args[0].Trim();
            string song = args[1].Trim();

            LyricsResult result = GetLyricsForSong(artist, song);
            if (result == null)
            {
                Respond(ns, from, "No results found for <b>\"" + artist + " - " + song + "\"</b>");
            }
            else
            {                
                StringBuilder say = new StringBuilder();

                // add song info
                say.AppendFormat("<b><a href=\"{0}\">{1}</a></b> by <b>{2}</b>:<br /><sub>{3}</sub>",
                    result.url,
                    result.song,
                    result.artist,
                    result.lyrics);
                Say(ns, say.ToString());
            }         
        }
        #endregion
    }
}
