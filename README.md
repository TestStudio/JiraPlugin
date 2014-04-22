JiraPlugin
==========
This project provides a JIRA compatible plug-in DLL that may be used with Telerik Test Studio for bug tracking. See http://docs.telerik.com/teststudio/user-guide/extensions/bug-tracking/configuration.aspx for additional information on bug tracking with Test Studio.

You must have Test Studio 2014.1.421 or above to use the plug-in. Contact Telerik Test Studio Technical Support to obtain this custom build if needed.

INSTALLATION INSTRUCTIONS

You will need Visual Studio to use this plug-in DLL.

1) Download the project onto a computer that has both Visual Studio and Test Studio installed

2) Compile the project. If you get compile errors, check the DLL references. The project references Test Studio DLL's and assumes they can be found in the default install folder for Test Studio.

NOTE: The plug-in is version specific. If you upgrade Test Studio you will need to come back and repeat steps 2-4 before the Jira plug-in will be functional again.

3) Copy Telerik.TestStudio.Jira.BugTracking.dll and Telerik.TestStudio.Jira.BugTracking.dll.config to the C:\Program Files (x86)\Telerik\Test Studio\Bin\Plugins folder

4) Copy Telerik.TestStudio.Jira.BugTracking.dll to the C:\Program Files (x86)\Telerik\Test Studio\Bin folder

5) Copy Xceed.Wpf.Toolkit.dll to the C:\Program Files (x86)\Telerik\Test Studio\Bin folder

Once everything is in place and you relaunch Test Studio, you should see a the Jira plug-in show in the Bug Tracking dialog.

DISCLAIMER: This project is being released to the public as an open source project. It is not officially supported by Telerik. It is not covered by any Telerik license or support contract.
