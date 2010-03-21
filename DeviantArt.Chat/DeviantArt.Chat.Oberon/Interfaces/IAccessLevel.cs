using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviantArt.Chat.Oberon
{    
    /// <summary>
    /// Represents methods a class must implement to handle assigning access levels 
    /// within the system.
    /// </summary>
    public interface IAccessLevel
    {
        /// <summary>
        /// The default user access level when a user isn't registered with the bot.
        /// </summary>
        int DefaultUserAccessLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Call to initialize settings based on other bot settings.
        /// </summary>
        /// <param name="bot">Bot to use to configure.</param>
        void InitializeFor(Bot bot);

        /// <summary>
        /// Gets the user access level.
        /// </summary>
        int GetUserLevel(string username);

        /// <summary>
        /// Sets the user access level. Must be between 100 and 0.
        /// </summary>
        void SetUserLevel(string username, int value);

        /// <summary>
        /// Sets the user access level using the named priv classes.
        /// </summary>        
        void SetUserLevel(string username, string privClass);

        /// <summary>
        /// Returns true if the priv class exists. Otherwise false.
        /// </summary>        
        bool HasUserLevel(string privClass);

        /// <summary>
        /// Removes a user's access level.
        /// </summary>
        /// <param name="username">Username to remove.</param>
        void RemoveUserLevel(string username);

        /// <summary>
        /// Gets the command access level.
        /// </summary>
        int GetCommandLevel(string command);

        /// <summary>
        /// Sets the command access level.
        /// </summary>
        void SetCommandLevel(string command, int value);

        /// <summary>
        /// Gets a copy of all the priv classes in the system.
        /// </summary>
        /// <returns>Copy of all the priv classes in the system.</returns>
        List<PrivClass> GetAllPrivClasses();

        /// <summary>
        /// Get the access level for a priv class.
        /// </summary>
        /// <param name="privClass">Priv Class to retrieve.</param>
        /// <returns>Access level.</returns>
        int GetPrivClassAccessLevel(string privClass);

        /// <summary>
        /// Updates a Priv Class's access level and updates all commands
        /// which also have that access level. 
        /// </summary>
        /// <param name="privClass"></param>
        /// <param name="value"></param>
        void UpdatePrivClassAccessLevel(string privClass, int value);

        /// <summary>
        /// Add a new priv class.
        /// </summary>        
        void AddPrivClass(PrivClass privClass);

        /// <summary>
        /// Removes a priv class.
        /// </summary>
        /// <param name="privClass">Priv class to remove.</param>
        void RemovePrivClass(string privClassName);

        /// <summary>
        /// Deletes all access levels.
        /// </summary>
        void ClearAccessLevels();

        /// <summary>
        /// Load settings from config file.
        /// </summary>
        void LoadAccessLevels();

        /// <summary>
        /// Save settings to config file.
        /// </summary>
        void SaveAccessLevels();

        /// <summary>
        /// Determins if a user has access to a command/
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <param name="command">Command to check.</param>
        /// <returns>True if user has access to command, otherwise false.</returns>
        bool UserHasAccess(string username, string command);

        /// <summary>
        /// Gets all user acess levels.
        /// </summary>
        /// <returns>Dictionary of users and access levels.</returns>
        Dictionary<string, int> GetAllUserAccessLevels();
    }
}
