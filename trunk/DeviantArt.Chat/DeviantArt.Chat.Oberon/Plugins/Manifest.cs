using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;

namespace DeviantArt.Chat.Oberon.Plugins
{
    /// <summary>
    /// Manifest data for a bot plugin.
    /// </summary>
    [Serializable]
    [DebuggerStepThrough] 
    [XmlType(AnonymousType = true, Namespace = "http://oberon.thehomeofjon.net")]
    [XmlRoot(Namespace="http://oberon.thehomeofjon.net", ElementName = "manifest", IsNullable = false)]
    public partial class Manifest 
    {                
        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Manifest()
        {
            // init variables to defaults
            this.Name = "";
            this.Description = "";
            this.Author = "";
            this.Contributors = "";
            this.HomepageUrl = "";
            this.Version = "";
            this.Updates = new ManifestUpdates();
        }
        #endregion

        #region Properties
        [XmlElement("name")]
        public string Name { get; set; }
        
        [XmlElement("description")]
        public string Description { get; set; }
        
        [XmlElement("author")]
        public string Author { get; set; }
        
        [XmlElement("contributors")]
        public string Contributors { get; set; }
        
        /// <remarks/>
        [XmlElement("homepageUrl", DataType="anyURI")]
        public string HomepageUrl { get; set; }
        
        [XmlElement("version")]
        public string Version { get; set; }
        
        [XmlElement("updates")]
        public ManifestUpdates Updates { get; set; }
        #endregion

        #region Virtual Properties
        /// <summary>
        /// .NET version of the plugin.
        /// </summary>
        [XmlIgnore]
        public Version AssemblyVersion
        {
            get { return new Version(Version); }
        }
        #endregion

        #region Static Helper Methods
        /// <summary>
        /// Creates a manifest object from the provided stream.
        /// </summary>
        /// <param name="filepath">Path to the manifest xml file.</param>
        /// <returns>Manifest.</returns>
        public static Manifest Create(string filepath)
        {
            return Create(System.IO.File.OpenRead(filepath)); // overload method will close stream
        }

        /// <summary>
        /// Creates a manifest object from the provided stream.
        /// </summary>
        /// <param name="manifestStream">Stream containing manifest xml.</param>
        /// <returns>Manifest.</returns>
        public static Manifest Create(System.IO.Stream manifestStream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Manifest));
            Manifest manifest = null;
            try
            {
                manifest = (Manifest)serializer.Deserialize(manifestStream);
            }
            finally
            {
                manifestStream.Close();
            }
            return manifest;
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

    /// <remarks/>    
    [Serializable]
    [DebuggerStepThrough]
    [XmlType(AnonymousType = true, Namespace = "http://oberon.thehomeofjon.net")]
    public partial class ManifestUpdates
    {
        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ManifestUpdates()
        {
            // init default values
            this.MinBotVersion = "";
            this.MaxBotVersion = "";
            this.UpdateManifestUrl = "";
            this.UpdateUrl = "";
        }
        #endregion

        #region Properties
        [XmlElement("minBotVersion")]
        public string MinBotVersion { get; set; }

        [XmlElement("maxBotVersion")]
        public string MaxBotVersion { get; set; }

        /// <remarks/>
        [XmlElement("updateManifestUrl", DataType = "anyURI")]
        public string UpdateManifestUrl { get; set; }

        [XmlElement("updateUrl", DataType = "anyURI")]
        public string UpdateUrl { get; set; }
        #endregion

        #region Virtual Properties
        /// <summary>
        /// Minimum version of the bot the plugin is compatible with.
        /// </summary>
        [XmlIgnore]
        public Version MinBotAssemblyVersion
        {
            get { return new Version(MinBotVersion); }
        }

        /// <summary>
        /// Maximum version of the bot the plugin is compatible with.
        /// </summary>
        [XmlIgnore]
        public Version MaxBotAssemblyVersion
        {
            get { return new Version(MaxBotVersion); }
        }
        #endregion
    }
}
