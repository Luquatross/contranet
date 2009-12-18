using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DeviantArt.Chat.Oberon
{
    /// <summary>
    /// Utility class to hold static functions that don't belong to any particular object.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// SHA Encrypts a string.
        /// </summary>
        /// <param name="str">String to encrypt.</param>
        /// <returns></returns>
        public static string SHA1(string str)
        {
            return BitConverter.ToString(SHA1Managed.Create().ComputeHash(Encoding.Default.GetBytes(str))).Replace("-", "");
        }

        /// <summary>
        /// Gets the operating system full version as a string.
        /// </summary>
        /// <returns>OS version.</returns>
        public static string GetOperatingSystemVersion()
        {
            System.OperatingSystem os = System.Environment.OSVersion;
            string osName = "Unknown";

            switch (os.Platform)
            {
                case System.PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            osName = "Windows 95";
                            break;
                        case 10:
                            osName = "Windows 98";
                            break;
                        case 90:
                            osName = "Windows ME";
                            break;
                    }
                    break;
                case System.PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 3:
                            osName = "Windws NT 3.51";
                            break;
                        case 4:
                            osName = "Windows NT 4";
                            break;
                        case 5:
                            if (os.Version.Minor == 0)
                                osName = "Windows 2000";
                            else if (os.Version.Minor == 1)
                                osName = "Windows XP";
                            else if (os.Version.Minor == 2)
                                osName = "Windows Server 2003";
                            break;
                        case 6:
                            osName = "Windows Vista";
                            if (os.Version.Minor == 0)
                                osName = "Windows Vista";
                            else if (os.Version.Minor == 1)
                                osName = "Windows 7";

                            break;

                    }
                    break;
            }

            return osName + ", " + os.Version.ToString();
        }
    }
}
