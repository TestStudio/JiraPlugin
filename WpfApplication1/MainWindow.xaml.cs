using System;
using System.Windows;

using ArtOfTest.WebAii.Design.Extensibility.BugTracking;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Telerik.TestStudio.Jira.BugTracking;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string server = "http://dad-pc:8090";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            string error;

            JArray projects = (JArray)JiraComm.GetJson(server, "/rest/api/2/project", "TelerikUser", "Telerik", out error);
            foreach (JToken jproject in projects)
            {
                JiraObjects.Project project = new JiraObjects.Project();
                JsonConvert.PopulateObject(jproject.ToString(), project);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string error;

            BugAttachment bugAttachment = new BugAttachment();
            bugAttachment.Author = "Bug Author1";
            bugAttachment.DateUploaded = DateTime.Now;
            bugAttachment.Description = "This is the bugs description";
            bugAttachment.FileName = "C:\\ScratchTest_FailLog_x7282013_x958AM.zip";
            bugAttachment.FileSize = 1234;

            JiraComm.AttachFile(server, "TelerikUser", new Base64String("Telerik"), "TSCB-3", bugAttachment, out error);
            this.tbError.Text = error;
        }
    }

    public class BugAttachment : IBugAttachment
    {
        #region IBugAttachment Members

        public string Author
        {
            get;
            set;
        }

        public DateTime DateUploaded
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public ulong FileSize
        {
            get;
            set;
        }

        #endregion
    }
}
