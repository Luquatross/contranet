using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Oberon.Plugins.LiveSearchService;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Plugin that allows a user to perform a web search using the Bing API.
    /// </summary>
    public class Search : Plugin
    {
        #region Private Variables
        private string _PluginName = "Search";
        private string _FolderName = "Extras";

        /// <summary>
        /// SOAP Search client object.
        /// </summary>
        LiveSearchService.LiveSearchService SearchClient = new DeviantArt.Chat.Oberon.Plugins.LiveSearchService.LiveSearchService();
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
            // register our command
            RegisterCommand("search", new BotCommandEvent(PerformSearch), new CommandHelp(
                "Peforms an internet search.", "search [search text]"), (int)PrivClassDefaults.Guests);
        }
        #endregion

        #region Command Handlers
        private void PerformSearch(string ns, string from, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Respond(ns, from, "You must provide text to search on.");
                return;
            }

            SearchRequest request = new SearchRequest();
            request.AppId = "1673C17DF0ADCEA175BF492E9689AB886B2A8747";
            request.Sources = new SourceType[] { SourceType.Web };
            request.Query = message;
            SearchResponse response = SearchClient.Search(request);
            if (response.Web != null && response.Web.Results.Count() > 0)
            {
                StringBuilder output = new StringBuilder(string.Format(":magnify: Results <b>{0} &#8211; {1}</b> of {2}<ol>",
                    response.Web.Offset,
                    response.Web.Results.Count(),
                    response.Web.Total));
                foreach (WebResult result in response.Web.Results)
                {
                    output.AppendFormat("<li><strong><u><a href=\"{0}\">{1}</a></u></strong><br />{2}<br /><sup><em>{3}</em></sup></li>",
                        result.Url.Replace("https://", "http://"),   // chat won't allow secure links
                        result.Title,
                        result.Description,
                        result.DisplayUrl);
                }
                output.Append("</ol>");
                Say(ns, output.ToString());
            }
            else
            {
                Respond(ns, from, string.Format("No results found for query '{0}'.", message));
            }
        }
        #endregion
    }
}
