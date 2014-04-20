using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using ArtOfTest.WebAii.Design.Extensibility.BugTracking;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// This is the main Test Studio plug-in class. It ties everything together
    /// and provides the main communication between Test Studio and this plug-in.
    ///
    /// The sequence of events is this:
    /// On Project load:
    ///     1) ResetSettings is called. Intended to let the plugin clear out old
    ///        setting in prepartion of new settings being loaded.
    ///     2) ApplyPersistableSettings is called. The deserialized JiraConnectionModel
    ///        object is passed in. It contains the settings loaded from the projects
    ///        settings.aiis file.
    ///
    /// On clicking the "Bug Tracking" button in the ribbon bar:
    ///     1) GetPersistableSettings is called. The plugin is expected to return
    ///        a filled out BugTrackerPersistableSettings object. Test Studio
    ///        uses this data to fill in the Manaege Bug Tracking dialog.
    ///
    /// On clicking the "Configure the connection" button
    ///     1) CanSave is called. Controls whether or not the Save button is enabled.
    ///     2) CanClose is called. Controls whether or not the Cancel button is enabled.
    ///
    /// On clicking Save:
    ///     1) OnSave is called. Save the UI settings and make them active.
    ///     2) GetPersistableSettings is called.
    ///
    /// On clicking Cancel:
    ///     1) OnClose is called. The plugin can clean up any resources or connections that were being used.
    ///     2) ApplyPersistableSettings is called.
    ///     3) CanSave is called.
    ///
    /// On clicking Save  in the Manage Bug Tracking dialog
    ///     1) Actually nothing in the bug tracker is called
    ///
    /// On clicking Cancel in the Manage Bug Tracking dialog
    ///     1) ApplyPersistableSettings is called
    ///     2) CanSave is called
    ///
    /// On closing the project:
    ///     1) ResetSettings is called. The plugin should remove all active settings so that the next project doesn't recieve
    ///        carried over settings.
    ///     2) CanSave is called.
    ///
    /// On clicking Submit Bug in the Results view
    ///     1) SubmitBug is called
    ///
    /// </summary>
    public class JiraBugTracker : IBugTracker, IBugTrackerConnectionUI
    {
        #region Fields

        private JiraConnectionUI settingsUiControl;
        private JiraConnViewModel jiraConnSettings;
        private JiraConnectionModel activeJiraConnection;

        #endregion

        #region Properties

        /// <summary>
        /// The ViewModel used by the configuration UI
        /// </summary>
        public JiraConnViewModel JiraSettings
        {
            get
            {
                if (null == this.jiraConnSettings)
                {
                    this.jiraConnSettings = new JiraConnViewModel();
                    this.jiraConnSettings.PropertyChanged += new PropertyChangedEventHandler(jiraConnSettings_PropertyChanged);
                }
                return this.jiraConnSettings;
            }
            set
            {
                this.jiraConnSettings = value;
                if (null != this.jiraConnSettings)
                {
                    this.jiraConnSettings.PropertyChanged += new PropertyChangedEventHandler(jiraConnSettings_PropertyChanged);
                }
            }
        }

        /// <summary>
        /// Get the system name. This name is displayed by Test Studio in the list of bug trackers.
        /// </summary>
        public string Name
        {
            get { return "JIRA"; }
        }

        /// <summary>
        /// Get the connection settings UI control, an object that implements IBugTrackerConnectionUI.
        /// Each provider exposes its own specific settings and handles connection internally.
        /// </summary>
        public IBugTrackerConnectionUI ConnectionUI
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the current error message if any.
        /// Required by the interface IBugTracker but not used in this implementation.
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the provider is configured so that the user can submit bugs to the bug tracking tool.
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return this.JiraSettings.CanSave;
            }
        }

        /// <summary>
        /// The WPF UserControl to present to the users so that they can fill in the necessary configuration data.
        /// </summary>
        public System.Windows.Controls.Control SettingsControl
        {
            get
            {
                if (this.settingsUiControl == null)
                {
                    this.settingsUiControl = new JiraConnectionUI();
                    this.settingsUiControl.Loaded += new System.Windows.RoutedEventHandler(SettingsControlLoaded);
                    this.settingsUiControl.DataContext = JiraSettings;
                    // TODO
                    // For some unknown reason binding in XAML is not working. No error but it doesn't pick up the data either.
                    // Force binding manually in code until I can figure out why since it works this way.
                    this.settingsUiControl.cbServers.ItemsSource = JiraSettings.MruServers;

                    this.settingsUiControl.OnGetProjects += new JiraConnectionUI.GetProjectsEventHandler(settingsUiControl_OnGetProjects);
                }
                return settingsUiControl;
            }
        }

        #endregion

        #region IBugTracker Members

        /// <summary>
        /// Applies the persistable settings upon loading the user settings from the XML file where persisted, or when Cancel is clicked.
        /// The settings are persisted so that the user can get the configured bug tracker on the next project load.
        /// </summary>
        /// <param name="settings">The deserialized settings to make active.</param>
        /// <returns>Returns 'true' if the settings are valid and the operation is successful.</returns>
        public bool ApplyPersistableSettings(BugTrackerPersistableSettings settings)
        {
            this.activeJiraConnection = settings as JiraConnectionModel;
            if (null != this.activeJiraConnection)
            {
                this.JiraSettings.ServerName = this.activeJiraConnection.ServerName;
                this.JiraSettings.User = this.activeJiraConnection.User;
                this.JiraSettings.Password = this.activeJiraConnection.Password;
                this.JiraSettings.ErrorMessage = string.Empty;
                // This will cause SelectedProject to be reset to null due to data binding.
                // Be sure to reselect SelectedProject afterward.
                this.JiraSettings.ProjectsList = new List<JiraProject>();
                this.JiraSettings.ProjectsList.Add(this.activeJiraConnection.SelectedProject);
                this.JiraSettings.SelectedProject = this.activeJiraConnection.SelectedProject;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieve the object that implements the BugTrackerPersistableSettings interface.
        /// This includes the LoadFrom and SaveTo methods used to serialize and deserialize the
        /// the configuration into/out of the current Test Studio project.
        /// </summary>
        /// <returns>The object that implements the BugTrackerPersistableSettings interface.
        /// Test Studio will later call the LoadFrom and/or SaveTo to deserialize/serialize
        /// the configuration.</returns>
        public BugTrackerPersistableSettings GetPersistableSettings()
        {
            return this.activeJiraConnection;
        }

        /// <summary>
        /// Resets bug tracker connection settings to default settings.
        /// Called when a project containg Jira settings is first opened, but before settings are loaded.
        /// Also called when the project is closed.
        /// Useful method to clean up the bug tracker data internally.
        /// </summary>
        public void ResetSettings()
        {
            this.JiraSettings = null;
            this.activeJiraConnection = null;
            this.settingsUiControl = null;
        }

        /// <summary>
        /// Submits a bug to the bug tracking tool.
        /// Return an int that represents the newly created bug ID.
        /// </summary>
        /// <param name="bug">The bug from Test Studio to be created.</param>
        /// <returns>The newly created bug ID. Unfortunately Test Studio only accepts an int while
        /// JIRA bug ID's are something like TSCB-13.</returns>
        public int SubmitBug(IBug bug)
        {
            string error;

            JiraComm jiraComm = new JiraComm(activeJiraConnection.ServerName, activeJiraConnection.User, activeJiraConnection.Password);
            string bugID = jiraComm.SubmitBug(activeJiraConnection.SelectedProject.key, bug.Title, bug.Description, bug.Priority,
                bug.CreatedDate, bug.Author, bug.AssignedTo, bug.Attachments, out error);
            this.ErrorMessage = error;
            // JIRA returns something like TSCB-14.
            // Return only the integer portion (until Test Studio is changed to take a string).
            return int.Parse(bugID.Substring(bugID.IndexOf('-')));
        }

        #endregion

        #region IBugTrackerConnectionUI Members

        /// <summary>
        /// Whether to let the user close the UI. Optional, return 'true' so that the user can click
        /// the 'Cancel' button i.e. to enable it.
        /// </summary>
        public bool CanClose
        {
            get
            {
                return !this.JiraSettings.IsBusy;
            }
        }

        /// <summary>
        /// Whether to allow the user to save the state (makes sense in addition to OnSave implementation).
        /// Return 'true' so that the user can click the 'Done' button i.e. to enable it.
        /// </summary>
        public bool CanSave
        {
            get
            {
                return this.JiraSettings.CanSave;
            }
        }

        /// <summary>
        /// Optional. Called whenever the bug tracker UI is closed for any reason.
        /// Clear the view for the next project to be loaded, which may not have Jira settings.
        /// </summary>
        public void OnClose()
        {
            // Nothing we need to do here.
        }

        /// <summary>
        /// Optional. Called whenever user clicks Save in the UI to save the settings.
        /// Enables customizing the save click handling.
        /// We'll make the settings in the ViewModel the active settings for submitting bugs.
        /// </summary>
        public void OnSave()
        {
            this.activeJiraConnection = new JiraConnectionModel();
            this.activeJiraConnection.ServerName = this.JiraSettings.ServerName;
            this.activeJiraConnection.User = this.JiraSettings.User;
            this.activeJiraConnection.Password = this.JiraSettings.Password;
            this.activeJiraConnection.SelectedProject = this.JiraSettings.SelectedProject;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler called when the WPF UserControl is loaded.
        /// The wrapping window listens to the CanSave and CanClose property changed events.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private void SettingsControlLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // We need to update the 'Save' (CanSave) and 'Cancel' (CanCancel) buttons in the UI.
            this.OnPropertyChanged("CanSave");
            this.OnPropertyChanged("CanClose");

            // Locate the Save button in the frame wrapping us
            FrameworkElement SaveButton;

            SaveButton = (FrameworkElement)VisualTreeHelper.GetParent(this.SettingsControl);
            while (SaveButton.Name != "AppContent")
            {
                SaveButton = (FrameworkElement)VisualTreeHelper.GetParent(SaveButton);
            }

            SaveButton = (FrameworkElement)VisualTreeHelper.GetParent(SaveButton);    // returns Grid
            SaveButton = (FrameworkElement)VisualTreeHelper.GetChild(SaveButton, 1);  // returns ButtonsPanelBorder
            SaveButton = (FrameworkElement)VisualTreeHelper.GetChild(SaveButton, 0);  // returns ButtonsPanel
            SaveButton = (FrameworkElement)VisualTreeHelper.GetChild(SaveButton, 1);  // returns Yes Button aka Done
            
            // Bind the IsDefault property of the Save button
            // to the SelectedProject property of the UI's listbox.
            // This will cause IsDefault to be set to true when the user
            // selects a project from the list.
            Binding myBinding = new Binding("SelectedItem");
            myBinding.Source = ((JiraConnectionUI)this.SettingsControl).lbProjects;
            myBinding.Path = new PropertyPath("SelectedItem");
            myBinding.Converter = new SelectedProjectToBoolConverter();
            myBinding.ConverterParameter = "False";
            SaveButton.SetBinding(Button.IsDefaultProperty, myBinding);
        }

        /// <summary>
        /// Handle property changes in the ViewModel. For now all we do
        /// is bubble up the SelectedProject property change to CanSave which
        /// enables/disables the 'Done' button on the Test Studio wrapping WPF window.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        public void jiraConnSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
       {
            if (e.PropertyName == "SelectedProject")
            {
                this.OnPropertyChanged("CanSave");
            }
        }

        /// <summary>
        /// Event handler that is called when the user clicks the 'Get Projects' button in the UI.
        /// We'll spawn a background thread which will fetch the list of projects the user has
        /// access to from JIRA.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void settingsUiControl_OnGetProjects(object sender, GetProjectsEventArgs e)
        {
            this.JiraSettings.IsBusy = true;
            this.JiraSettings.SelectedProject = null;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object sender2, DoWorkEventArgs e2)
            {
                WorkerMethod();
            };
            worker.RunWorkerCompleted += delegate(object sender2, RunWorkerCompletedEventArgs e2)
            {
                this.JiraSettings.IsBusy = false;
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Worker thread that fetches the list projects from JIRA.
        /// </summary>
        private void WorkerMethod()
        {
            string error;
            JiraComm jiraComm = new JiraComm(this.JiraSettings.ServerName, this.JiraSettings.User, this.JiraSettings.Password);

            this.JiraSettings.ErrorMessage = string.Empty;
            List<JiraProject> jiraProjects = jiraComm.GetProjects(out error);
            this.JiraSettings.ErrorMessage = error;
            this.JiraSettings.ProjectsList = jiraProjects;
            if (null == error)
            {
                this.JiraSettings.AddServer(this.JiraSettings.ServerName);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
