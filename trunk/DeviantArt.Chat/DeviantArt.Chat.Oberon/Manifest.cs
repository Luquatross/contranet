using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Class to assist with manipulating the plugin manifest xml.
    /// </summary>
    public partial class Manifest
    {
        #region Public Properties
        /// <summary>
        /// Description for this plugin. Optional.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Author of the plugin.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        /// Users who contributed to making the plugin. Optional.
        /// </summary>
        public string Contributors { get; private set; }

        /// <summary>
        /// URL for the plugin. Optional.
        /// </summary>
        public string HomepageUrl { get; private set; }

        /// <summary>
        /// Url that contains the latest manifest file for this plugin.
        /// </summary>
        public string UpdateManifestUrl { get; private set; }

        /// <summary>
        /// URL that contains the latest package for this plugin.
        /// </summary>
        public string UpdateUrl { get; private set; }

        /// <summary>
        /// Version for this plugin.
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// Minimum version of the bot this plugin supports.
        /// </summary>
        public Version MinBotVersion { get; private set; }

        /// <summary>
        /// Maximum version of the bot this plugin supports.
        /// </summary>
        public Version MaxBotVersion { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        private Manifest()
        {
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Returns true if version is compatible with bot version.
        /// </summary>
        /// <param name="botVersion">The bot version.</param>
        /// <returns>True if compatible, otherwise false.</returns>
        public bool IsCompatible(Version botVersion)
        {
            return (botVersion > MinBotVersion && botVersion < MaxBotVersion);
        }
        #endregion

        #region Static Helper Methods
        /// <summary>
        /// List of all manifests xml data that has been read. The key is the manifest file path, and the value is the manifest xml.
        /// We cache the manifest xml because multiple plugins can use the same manifest file if they are all located in the
        /// same assembly. If that's the case, we don't wan't to read the xml multiple times.
        /// </summary>
        private static Dictionary<string, XDocument> ManifestXmlData = new Dictionary<string, XDocument>();

        /// <summary>
        /// Creates an instance of the manifest data from manifest xml.
        /// </summary>
        /// <param name="pluginName">Plugin name to load manifest for.</param>
        /// <param name="manifestFile">Path to the manifest file.</param>
        /// <returns>Manifest.</returns>
        public static Manifest Create(string pluginName, string manifestFile)
        {
            // get xml document
            XDocument doc = null;
            if (ManifestXmlData.ContainsKey(manifestFile))
            {
                doc = ManifestXmlData[manifestFile];
            }
            else
            {
                // create doc and add it to cache
                doc = XDocument.Load(manifestFile);
                ManifestXmlData.Add(manifestFile, doc);
            }

            // create manifest
            return Create(pluginName, doc);
        }

        /// <summary>
        /// Creates an instance of the manifest data from manifest xml.
        /// </summary>
        /// <param name="pluginName">Plugin name to load manifest for.</param>
        /// <param name="doc">XDocument containing manifest xml.</param>
        /// <returns>Manifest.</returns>
        public static Manifest Create(string pluginName, XDocument doc)
        {            
            // get the namespace
            string ns = "{" + doc.Root.Name.Namespace + "}";

            // create manifest using linq to xml
            // note: we are casting XElement objects to strings. The element is not required by the schema
            // so it might not be in the xml. If that's the case, the cast will result in a null value.
            var manifest = from p in doc.Descendants(ns + "plugin")
                                where p.Element(ns + "name").Value == pluginName
                                select new Manifest
                                {
                                    Author = (string)p.Element(ns + "author"),
                                    Contributors = (string)p.Element(ns + "contributors"), 
                                    Description = (string)p.Element(ns + "description"),
                                    HomepageUrl = (string)p.Element(ns + "homepageUrl"),
                                    Version = new Version((string)doc.Descendants(ns + "version").First()),
                                    MinBotVersion = new Version((string)doc.Descendants(ns + "minBotVersion").First()),
                                    MaxBotVersion = new Version((string)doc.Descendants(ns + "maxBotVersion").First()),
                                    UpdateManifestUrl = (string)doc.Descendants(ns + "updateManifestUrl").First(),
                                    UpdateUrl = (string)doc.Descendants(ns + "updateUrl").First()
                                };

            // make sure we found something
            if (manifest.Count() == 0)
                throw new ArgumentOutOfRangeException(string.Format("Unable to find plugin with name '{0}' in the manifest.", pluginName));
            
            return manifest.Single();
        }

        /// <summary>
        /// Retrieves the manifest schema.
        /// </summary>
        /// <returns>Manifest schema.</returns>
        public static XmlSchema GetManifestSchema()
        {
            // get the stream for the file
            System.IO.Stream str = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                "DeviantArt.Chat.Oberon.Plugins.Manifest.xsd");

            // create schema from embeddeded schema file
            using (XmlReader reader = XmlTextReader.Create(str))
            {
                return XmlSchema.Read(reader, null);
            }
        }

        /// <summary>
        /// Returns true if the xml is valid using the manifest schema. Otherwise false.
        /// If the schema is not valid, the error array will be initialized with all unique
        /// exceptions encountered during the validation.
        /// </summary>
        /// <param name="filepath">Path to the file to validate.</param>
        /// <param name="errors">Array that will hold exceptions if errors are encountered during validation.</param>
        /// <returns>True if valid, otherwise false.</returns>
        public static bool IsValidManifest(string filepath, out Exception[] errors)
        {
            return IsValidManifest(System.IO.File.OpenRead(filepath), out errors); // overloaded method will close stream
        }

        /// <summary>
        /// Returns true if the xml is valid using the manifest schema. Otherwise false.
        /// If the schema is not valid, the error array will be initialized with all unique
        /// exceptions encountered during the validation.
        /// </summary>
        /// <param name="xmlStream">Stream containing the xml to be validated.</param>
        /// <param name="errors">Array that will hold exceptions if errors are encountered during validation.</param>
        /// <returns>True if valid, otherwise false.</returns>
        public static bool IsValidManifest(System.IO.Stream xmlStream, out Exception[] errors)
        {
            try
            {
                XmlSchema schema = GetManifestSchema();
                bool isValidXml = true;
                Dictionary<string, Exception> validationErrors = new Dictionary<string, Exception>();

                // Create schema set
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.Add(schema);

                // Create reader tied to schema
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemaSet;
                settings.ValidationEventHandler += new ValidationEventHandler(delegate(object sender, ValidationEventArgs e)
                {
                    isValidXml = false;
                    // if we haven't seen this error before, add it
                    if (!validationErrors.ContainsKey(e.Message))
                        validationErrors.Add(e.Message, e.Exception);
                });

                // loop through entire file and make sure it's valid
                using (XmlReader reader = XmlReader.Create(xmlStream, settings))
                {
                    while (reader.Read()) ;
                }

                // initialize errors if necessary
                if (isValidXml)
                {
                    errors = new Exception[] { };
                }
                else
                {
                    errors = validationErrors.Values.ToArray();
                }
               
                return isValidXml;
            }
            finally
            {
                // close the stream
                xmlStream.Close();
            }
        }
        #endregion
    }
}
