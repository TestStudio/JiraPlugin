using System;

using ArtOfTest.WebAii.Design.Extensibility.BugTracking;

using Newtonsoft.Json;

namespace Telerik.TestStudio.Jira.BugTracking
{
    [Serializable]
    [JsonObject]
    public class JiraConnectionModel : BugTrackerPersistableSettings
    {
        #region Fields

        private string server_name;
        private string user;
        private Base64String password;
        private JiraProject selected_project;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public JiraConnectionModel()
        {

        }

        /// <summary>
        /// Construct new model by copying the contents of another model.
        /// </summary>
        /// <param name="other">The other model to copy from.</param>
        public JiraConnectionModel(JiraConnectionModel other)
        {
            this.server_name = other.ServerName;
            this.user = other.User;
            this.password = other.Password;
            this.selected_project = other.SelectedProject;
        }

        #endregion

        #region Properties

        /// <summary>
        /// TODO Find out what this is for
        /// </summary>
        [JsonIgnore]
        public override string RootName
        {
            get
            {
                return "JiraConnectionModel";
            }
        }

        /// <summary>
        /// The URL of the JIRA server
        /// </summary>
        public override string ServerName
        {
            get { return this.server_name; }
            set { this.server_name = value; }
        }

        /// <summary>
        /// The user name to login as
        /// </summary>
        public string User
        {
            get { return this.user; }
            set { this.user = value; }
        }

        /// <summary>
        /// The password that goes with the user name for logging in
        /// </summary>
        public Base64String Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        /// <summary>
        /// The JIRA project to submit bugs under
        /// </summary>
        public JiraProject SelectedProject
        {
            get { return this.selected_project; }
            set { this.selected_project = value; }
        }

        /// <summary>
        /// The project name to display in Test Studio's bug tracker configuration UI
        /// </summary>
        [JsonIgnore]
        public override string ProjectName
        {
            get
            {
                if (null != this.SelectedProject)
                {
                    return this.SelectedProject.name;
                }
                else
                    return string.Empty;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region BugTrackerPersistableSettings methods

        /// <summary>
        /// Load the JIRA connection settings from the XML store.
        /// </summary>
        /// <param name="store">The XML store to read the settings out of. It originally
        /// comes from the settings.aiis file.</param>
        public override void LoadFrom(ArtOfTest.Common.Serialization.XmlStoreNode store)
        {
            this.ServerName = store.GetValue<string>("ServerName");
            this.User = store.GetValue<string>("User");
            this.Password = store.GetValue<Base64String>("Password");
            this.SelectedProject = store.GetValue<JiraProject>("Project");
        }

        /// <summary>
        /// Save the settings to an XML store.
        /// </summary>
        /// <param name="store">The XML store to save the settings to. This eventually gets persisted
        /// to the settings.aiis file.</param>
        public override void SaveTo(ArtOfTest.Common.Serialization.XmlStoreNode store)
        {
            store.AddValue("ServerName", this.ServerName);
            store.AddValue("User", this.User);
            store.AddValue("Password", this.Password);
            store.AddValue("Project", this.SelectedProject);
        }

        #endregion
    }
}
