using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// Validation rule used on the Server combobox.
    /// Checks whether or not the entered URL is of legal form.
    /// If it is, no error icon is shown.
    /// If it isn't, an error icon is shown and a tooltip is added to the combobox.
    /// </summary>
    public class UriValidation : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((string)value) || Uri.IsWellFormedUriString((string)value, UriKind.Absolute))
                return new ValidationResult(true, null);
            else
                return new ValidationResult(false, (string)value + " is not a valid URL");
        }
    }

    /// <summary>
    /// Converter used to drive IsEnabled property of the JIRA credentials groupbox.
    /// If a valid server URL is entered, the groupbox becomes enabled
    /// allowing the user to enter user name and password. Else it is disabled.
    /// </summary>
    public class UriStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Uri.IsWellFormedUriString((string)value, UriKind.Absolute);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter used to drive IsDefault property of the GetProjects button
    /// and the Done button. If no project is currently selected IsDefault 
    /// of the GetProjects button is set to true (the Enter key activates it)
    /// while the Done button is set to false. Once the user selects a project
    /// the default switches from the GetProjects button to the Done button.
    /// </summary>
    public class SelectedProjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object CheckSelectedIsEmpty, CultureInfo culture)
        {
            if ((string)CheckSelectedIsEmpty == "True")
            {
                return (null == value);
            }
            else
            {
                return (null != value);
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
