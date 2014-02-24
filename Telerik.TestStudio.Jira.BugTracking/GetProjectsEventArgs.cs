using System;
using System.Windows.Controls;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// Event arguments class used when processing the GetProjectsCmd command
    /// </summary>
    public class GetProjectsEventArgs : EventArgs
    {
        #region Constructors

        public GetProjectsEventArgs(ListBox target)
        {
            this.TargetListBos = target;
        }

        #endregion

        #region Properties

        public ListBox TargetListBos { get; private set; }

        #endregion
    }
}
