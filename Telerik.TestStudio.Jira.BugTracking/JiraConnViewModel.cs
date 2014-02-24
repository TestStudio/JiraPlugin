using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// This is the ViewModel that backs the JIRA connection settings configuration UI.
    /// </summary>
    public class JiraConnViewModel : INotifyPropertyChanged
    {
        #region Fields

        private const int max_servers = 8;

        private string server_name;
        private string user;
        private Base64String password;
        private JiraProject selected_project;

        private string errorMsg;
        private List<JiraProject> jiraProjectList;
        private bool isBusy;

        #endregion

        #region Properties

        /// <summary>
        /// The URL of the JIRA server
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.server_name;
            }
            set
            {
                if (value != this.server_name)
                {
                    this.server_name = value;
                    this.ProjectsList = null;
                    OnPropertyChanged("ServerName");
                }
            }
        }

        /// <summary>
        /// A list of most recently used JIRA server URL's
        /// </summary>
        public StringCollection MruServers
        {
            get
            {
                return JiraTrackerSettings.Default.MruServers;
            }
            //set
            //{
            //    if (value != this.mru_servers)
            //    {
            //        this.mru_servers = value;
            //        this.OnPropertyChanged("MruServers");
            //    }
            //}
        }

        /// <summary>
        /// The user name to login as
        /// </summary>
        public string User
        { 
            get
            {
                return this.user;
            }
            set
            {
                if (value != this.user)
                {
                    this.user = value;
                    this.ProjectsList = null;
                    OnPropertyChanged("User");
                }
            }
        }

        /// <summary>
        /// The password that goes with the user name
        /// </summary>
        public Base64String Password
        {
            get
            {
                return this.password;
            }
            set
            {
                if (value != this.password)
                {
                    this.password = value;
                    this.ProjectsList = null;
                    OnPropertyChanged("Password");
                }
            }
        }

        /// <summary>
        /// The list of available projects the user has access to
        /// </summary>
        public List<JiraProject> ProjectsList
        {
            get
            {
                return this.jiraProjectList;
            }
            set
            {
                if (value != this.jiraProjectList)
                {
                    this.jiraProjectList = value;
                    OnPropertyChanged("ProjectsList");
                }
            }
        }

        /// <summary>
        /// The project selected to file bugs under
        /// </summary>
        public JiraProject SelectedProject
        {
            get
            {
                return this.selected_project;
            }
            set
            {
                if (value != this.selected_project)
                {
                    this.selected_project = value;
                    this.OnPropertyChanged("SelectedProject");
                }
            }
        }

        /// <summary>
        /// Whether or not to show the IsBusy indicator. We go "busy" while
        /// communicating with the JIRA server (to fetch the list of projects)..
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }
            set
            {
                if (value != this.isBusy)
                {
                    this.isBusy = value;
                    this.OnPropertyChanged("IsBusy");
                }
            }
        }

        /// <summary>
        /// Any error message to display to the user e.g. Unable to logon
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return this.errorMsg;
            }
            set
            {
                if (value != this.errorMsg)
                {
                    this.errorMsg = value;
                    OnPropertyChanged("ErrorMessage");
                }
            }
        }

        /// <summary>
        /// Whether or not the settings are fully configured and we are ready to commit them.
        /// Drives the "Done" button in the UI.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return (null != this.SelectedProject);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The MRU server list is stored in the global user settings so that
        /// any/every project shares the same list. This method adds the indicated
        /// server to the top of the list. If it already exists in the list it will
        /// be moved to the top of the list.
        /// </summary>
        /// <param name="server">The server to add/move to the top of the list.</param>
        internal void AddServer(string server)
        {
            if (JiraTrackerSettings.Default.MruServers.Contains(server))
            {
                JiraTrackerSettings.Default.MruServers.Remove(server);
            }
            // Trim the list so that it doesn't grow too large
            while (JiraTrackerSettings.Default.MruServers.Count >= max_servers)
            {
                JiraTrackerSettings.Default.MruServers.RemoveAt(max_servers - 1);
            }
            JiraTrackerSettings.Default.MruServers.Insert(0, server);
            JiraTrackerSettings.Default.Save();
            this.OnPropertyChanged("MruServers");
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
