using System.Windows.Controls;
using System.Windows.Input;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// Interaction logic for JiraConnectionUI.xaml
    /// </summary>
    public partial class JiraConnectionUI : UserControl
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public JiraConnectionUI()
        {
            InitializeComponent();
        }

        #endregion

        #region Command Handlers

        /// <summary>
        /// Called when the user clicks the 'Get Projects' button.
        /// Get the list of projects from the specified server using the specified credentials
        /// </summary>
        /// <param name="sender">The object sending the command, should be a this.</param>
        /// <param name="e">The event arguments.</param>
        private void GetProjectsCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ListBox target = e.OriginalSource as ListBox;
            if (null != target)
            {
                if (null != OnGetProjects)
                {
                    OnGetProjects(sender, new GetProjectsEventArgs(target));
                }
            }
            e.Handled = true;
        }

        // Determine whether or not we have required info to get list of projects.
        private void GetProjectsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.tbUser.Text.Length > 0 && this.passwordBox1.SecurePassword.Length > 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion

        #region Event Handlers

        // A delegate type for hooking up change notifications.
        public delegate void GetProjectsEventHandler(object sender, GetProjectsEventArgs e);
        public event GetProjectsEventHandler OnGetProjects;

        #endregion
    }
}
