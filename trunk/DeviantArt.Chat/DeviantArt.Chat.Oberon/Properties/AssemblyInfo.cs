using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Oberon - DeviantArt Chatbot")]
[assembly: AssemblyDescription("DeviantArt chat bot that connects to the dAmn chat network.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Shadowbox Solutions")]
[assembly: AssemblyProduct("Oberon - DeviantArt Chatbot")]
[assembly: AssemblyCopyright("Copyright ©  2009-2010 Jon Haywood, Shadowbox Solutions (http://oberon.thehomeofjon.net/)")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e9245870-1418-4a9b-bdfd-c7cc630577d0")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.8.5.*")]
[assembly: AssemblyFileVersion("0.8.5.*")]

// Change Log
// Version 0.8.0.* - Base version.
// Version 0.8.1.* - Added ability to ignore users
//                 - Added debug messages when loading plugins
//                 - Added try/catch around assembly loading so bot wouldn't crash on one dll
// Version 0.8.2.* - Update Manager for Bot is built-in, so updates happen automatically
// Version 0.8.3.* - Added automatic update capability for plugins
// Version 0.8.4.* - Added ability to retry when a connection fails.
// Version 0.8.5.* - When bot restarts, re-reads config file
//                 - Plugins are always on when first loaded