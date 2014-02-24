using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// A collection of .NET object definitions to be used for JSON communication with a JIRA server.
    /// </summary>

    /// <summary>
    /// Represents a signel JIRA project. This class must be marked Serializable to that
    /// Test Studio can automatically serialize/deserialize it to the projects settings.aiis file.
    /// </summary>
    /// <seealso cref="https://docs.atlassian.com/jira/REST/latest/#idp2300240"/>
    [Serializable]
    [JsonObject]
    public class JiraProject
    {
        public string self { get; set; }
        public int id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public Dictionary<string, string> avatarUrls { get; set; }
    }

    /// <summary>
    /// Represents the fields of an Issue when posting to JIRA
    /// <seealso cref="https://docs.atlassian.com/jira/REST/latest/#idp1713328"/>
    /// </summary>
    [Serializable]
    [JsonObject]
    public class JiraIssue
    {
        public class clFields
        {
            public class clProject
            {
                public string key;     // e.g. TST-24
            }
            public class clIssueType
            {
                public string name;    // e.g. Bug
            }
            public class clPriority
            {
                public int id;          // e.g. ???
            }

            public clProject project = new clProject();
            public string summary;     // e.g. something's wrong
            public string description; // e.g. Creating of an issue using project keys and issue type names using the REST API
            //public clPriority priority = new clPriority();    // Can add priority when it's configurable in the UI
            public clIssueType issuetype = new clIssueType();
        }
        public clFields fields = new clFields();
    }

    /// <summary>
    /// Represents the fields returned after creating an Issue in JIRA
    /// <seealso cref="https://docs.atlassian.com/jira/REST/latest/#idp1713328"/>
    /// </summary>
    [Serializable]
    [JsonObject]
    public class JiraIssueCreated
    {
        public string self;     // e.g. http://www.example.com/jira/rest/api/2/issue/10000
        public string key;      // e.g. TST-24
        public int id;          // e.g. 10000
    }

    /// <summary>
    /// Represents the fields returns after attaching a file to an issue in JIRA
    /// <seealso cref="https://docs.atlassian.com/jira/REST/latest/#idp1726000"/>
    /// </summary>
    [Serializable]
    [JsonObject]
    public class JiraFileAttached
    {
        public string self;                 // e.g. http://gibson:8090/rest/api/2/attachment/10001
        public int id;                      // e.g. 10001
        public string filename;             // e.g. ScratchTest_FailLog_x7282013_x958AM.zip
        public class clAuthor
        {
            public string self;             // e.g. http://gibson:8090/rest/api/2/user?username=TelerikUser
            public string name;             // e.g. TelerikUser
            public string emailAddress;     // e.g. telerikuser@telerik.com
            public Dictionary<string, string> avatarUrls { get; set; }
            public string displayName;      // e.g. Telerik User
            public bool active;             // e.g. true
        }
        public clAuthor author = new clAuthor();

        public DateTime created;            // e.g. 2013-07-30T16:55:22.692-0500
        public int size;                    // e.g. 383984
        public string mimeType;             // e.g. application/zip
        public string content;              // e.g. http://gibson:8090/secure/attachment/10001/ScratchTest_FailLog_x7282013_x958AM.zip
        public string thumbnail;            // e.g. http://www.example.com/jira/secure/thumbnail/10002
    }
}
