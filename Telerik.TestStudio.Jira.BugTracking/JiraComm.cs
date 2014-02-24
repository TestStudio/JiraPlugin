using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using ArtOfTest.WebAii.Design.Extensibility.BugTracking;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// A utility class for communicating with a JIRA server using the REST API.
    /// See https://docs.atlassian.com/jira/REST/latest/ for the full JIRA REST API.
    /// </summary>
    public class JiraComm
    {
        const string agent = "TelerikJira/1.0.0";

        #region Properties

        public string Server { get; private set; }
        public string User { get; private set; }
        public Base64String Password { get; private set; }

        #endregion

        #region Constructors

        public JiraComm(string server, string user, Base64String password)
        {
            this.Server = server;
            this.User = user;
            this.Password = password;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a list of all projects the current user has visibility of.
        /// </summary>
        /// <returns>A JArray of the projects.</returns>
        /// <see cref="https://docs.atlassian.com/jira/REST/latest/#idp2300240"/>
        public List<JiraProject> GetProjects(out string error)
        {
            JToken result = JiraComm.GetJson(this.Server, "/rest/api/2/project", this.User, this.Password, out error);
            if (null != result)
            {
                if (result.GetType() == typeof(JArray))
                {
                    JArray jsonProjects = result as JArray;
                    List<JiraProject> projects = new List<JiraProject>(jsonProjects.Count);
                    foreach (JToken jsonProject in jsonProjects)
                    {
                        JiraProject project = new JiraProject();
                        JsonConvert.PopulateObject(jsonProject.ToString(), project);
                        projects.Add(project);
                    }
                    return projects;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a new JIRA issue (a bug).
        /// </summary>
        /// <param name="projectKey">The projects key e.g. TSCB.</param>
        /// <param name="bugTitle">The title of the bug.</param>
        /// <param name="bugDescription">The detailed description of the bug.</param>
        /// <param name="bugPriority">The priority of the bug.</param>
        /// <param name="bugCreatedDate">The date the bug was created.</param>
        /// <param name="bugAuthor">The author of the bug.</param>
        /// <param name="bugAssignedTo">The person/group the bug is assigned to.</param>
        /// <param name="bugAttachments">List of files to attach to the bug.</param>
        /// <param name="error">Any errors during bug creation are written here.</param>
        /// <returns>The key of the bug e.g. TSCB-14</returns>
        public string SubmitBug(string projectKey, string bugTitle, string bugDescription, int bugPriority, DateTime bugCreatedDate,
            string bugAuthor, string bugAssignedTo, List<IBugAttachment> bugAttachments, out string error)
        {
            JiraIssue jiraIssue = new JiraIssue();
            jiraIssue.fields.issuetype.name = "Bug";
            jiraIssue.fields.project.key = projectKey;
            jiraIssue.fields.summary = bugTitle;
            jiraIssue.fields.description = bugDescription;
            //jiraIssue.Fields.Priority.Id = 1;

            JToken result = JiraComm.PostJson(this.Server, "/rest/api/2/issue/", this.User, this.Password, JsonConvert.SerializeObject(jiraIssue), out error);
            // Make sure the issue was successfully created before attaching files
            if (null != result)
            {
                JiraIssueCreated createdIssue = new JiraIssueCreated();
                JsonConvert.PopulateObject(result.ToString(), createdIssue);

                foreach (IBugAttachment attachment in bugAttachments)
                {
                    AttachFile(this.Server, this.User, this.Password, createdIssue.key, attachment, out error);
                    // Break if there was an error attaching this file
                    if (null != error)
                    {
                        break;
                    }
                }

                return createdIssue.key;
            }
            return null;
        }

        #endregion

        #region Core private communication methods

        /// <summary>
        /// Gets a JSON object from the JIRA server using an HTTP GET
        /// </summary>
        /// <param name="server">The URL of the server e.g. http://myserver.com</param>
        /// <param name="path">The REST path send the request to e.g. /rest/api/2/project</param>
        /// <param name="username">The user ID required for authentication. null or empty will use anonymous access.</param>
        /// <param name="password">The password required for authentication encoded using Base64. null or empty will use anonymous access.</param>
        /// <returns>A JSON object containing the results</returns>
        internal static JToken GetJson(string server, string path, string username, Base64String password, out string error)
        {
            JToken result;

            error = null;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(server + path);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = agent;
            httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();

            // Add the required authentication header
            if (!string.IsNullOrEmpty(username) && password.Length > 0)
            {
                Base64String authInfo = password.prefix(username + ":");
                httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
            }
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (JsonTextReader streamReader = new JsonTextReader(new System.IO.StreamReader(httpResponse.GetResponseStream())))
                {
                    result = JToken.ReadFrom(streamReader);
                }
                return result;
            }
            catch (WebException e)
            {
                error = e.Message;
                return null;
            }
        }

        /// <summary>
        /// Gets a JSON object from the JIRA server using an HTTP GET
        /// </summary>
        /// <param name="server">The URL of the server e.g. http://myserver.com</param>
        /// <param name="path">The REST path send the request to e.g. /rest/api/2/project</param>
        /// <param name="username">The user ID required for authentication. null or empty will use anonymous access.</param>
        /// <param name="password">The password required for authentication. null or empty will use anonymous access.</param>
        /// <returns>A JSON object containing the results</returns>
        internal static JToken GetJson(string server, string path, string username, string password, out string error)
        {
            JToken result;

            error = null;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(server + path);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = agent;
            httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();

            // Add the required authentication header
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                string authInfo = username + ":" + password;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
            }
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (JsonTextReader streamReader = new JsonTextReader(new System.IO.StreamReader(httpResponse.GetResponseStream())))
                {
                    result = JToken.ReadFrom(streamReader);
                }
                return result;
            }
            catch (WebException e)
            {
                error = e.Message;
                return null;
            }
        }

        /// <summary>
        /// Sends a JSON object to a JIRA server using an HTTP POST
        /// and gets the JSON response from the server.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="path">The REST path send the request to e.g. /rest/api/2/project</param>
        /// <param name="username">The user ID required for authentication. null or empty will use anonymous access.</param>
        /// <param name="password">The password required for authentication. null or empty will use anonymous access.</param>
        /// <param name="json_request">The JSON object to include in the POST</param>
        /// <returns></returns>
        internal static JToken PostJson(string server, string path, string username, Base64String password, JToken json_request, out string error)
        {
            JToken result;

            error = null;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(server + path);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = agent;
            httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();

            // Add the required authentication header
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                Base64String authInfo = password.prefix(username + ":");
                httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
            }

            if (null != json_request)
            {
                using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(json_request.ToString());
                    streamWriter.Flush();
                }
            }

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (JsonTextReader streamReader = new JsonTextReader(new System.IO.StreamReader(httpResponse.GetResponseStream())))
                {
                    result = JToken.ReadFrom(streamReader);
                }
                return result;
            }
            catch (WebException e)
            {
                error = e.Message;
                return null;
            }
        }

        /// <summary>
        /// Attach a file to a JIRA issue (bug).
        /// <seealso cref="https://docs.atlassian.com/jira/REST/latest/#idp1726000"/>
        /// </summary>
        /// <param name="issueKey">THe key of the bug to attach the file to e.g. TSCB-14.</param>
        /// <param name="attachment">The declaration of the file to be attached.</param>
        /// <returns>A FileAttached object.</returns>
        internal static JiraFileAttached AttachFile(string server, string user, Base64String password, string issueKey, IBugAttachment attachment, out string error)
        {
            JiraFileAttached fileAttached = new JiraFileAttached();

            string boundaryMarker = "-------------------------" + DateTime.Now.Ticks.ToString();

            HttpWebRequest httpWebRequest = HttpWebRequest.Create(server + "/rest/api/2/issue/" +
                                issueKey + "/attachments") as HttpWebRequest;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = agent;
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundaryMarker;
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.Headers.Add("X-Atlassian-Token: nocheck");
            httpWebRequest.KeepAlive = true;
            httpWebRequest.Proxy = WebRequest.GetSystemWebProxy();

            // Add the required authentication header
            if (!string.IsNullOrEmpty(user) && password.Length > 0)
            {
                Base64String authInfo = password.prefix(user + ":");
                httpWebRequest.Headers["Authorization"] = "Basic " + authInfo;
            }

            FileInfo fi = new FileInfo(attachment.FileName);
            string contentHeader = "--" + boundaryMarker + "\r\n";
            contentHeader += "Content-Disposition: form-data; name=\"file\"; filename=\"" + HttpUtility.HtmlEncode(fi.Name) + "\"\r\n";
            contentHeader += "Content-Type: application/octet-stream\r\n";  //TODO If attaching something other than .zip files, this will have to change.
            contentHeader += "\r\n";
            byte[] contentHeaderBytes = System.Text.Encoding.UTF8.GetBytes(contentHeader);

            try
            {
                FileStream fs = new FileStream(attachment.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] requestBytes = new byte[65536];
                int bytesRead = 0;
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(contentHeaderBytes, 0, contentHeaderBytes.Length);
                    while ((bytesRead = fs.Read(requestBytes, 0, requestBytes.Length)) != 0)
                    {
                        requestStream.Write(requestBytes, 0, bytesRead);
                    }
                    fs.Close();

                    string closingBoundary = "\r\n--" + boundaryMarker + "--";
                    byte[] closingBoundaryBytes = System.Text.Encoding.UTF8.GetBytes(closingBoundary);
                    requestStream.Write(closingBoundaryBytes, 0, closingBoundaryBytes.Length);
                }
            }
            catch (FileNotFoundException e)
            {
                error = e.Message;
                return null;
            }
            //READ RESPONSE FROM STREAM
            try
            {
                using (JsonTextReader streamReader = new JsonTextReader(new StreamReader(httpWebRequest.GetResponse().GetResponseStream())))
                {
                    // JIRA supports attaching multiple files at once. Hence it returns an array of FileAttached objects.
                    // Since we only attach one file at a time, we only need to parse the first one and return it.
                    JArray result = (JArray)JArray.ReadFrom(streamReader);
                    JsonConvert.PopulateObject(result[0].ToString(), fileAttached);
                }
                error = null;
                return fileAttached;
            }
            catch (WebException e)
            {
                error = e.Message;
                // Get the HTML response returned, if any
                //StreamReader rdr = new StreamReader(e.Response.GetResponseStream());
                //string response = rdr.ReadToEnd();
                return null;
            }
        }

        #endregion
    }
}
