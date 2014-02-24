using System.Windows;
using System.Windows.Controls;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// Since the .NET PasswordBox does not give you a DependencyProperty for the password (encrypted or not)
    /// we'll extend it to add our own.
    /// </summary>
    public static class PasswordBoxAssistant
    {
        #region DependencyProperty's used in the XAML

        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached("BoundPassword", typeof(Base64String), typeof(PasswordBoxAssistant), new PropertyMetadata(new Base64String(), OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPassword =
            DependencyProperty.RegisterAttached("BindPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPassword =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false));

        #endregion

        #region Event handlers

        /// <summary>
        /// Called when the value of the property the PasswordBox is bound to changes.
        /// We'll update the bound property with the updated password value, unless we're
        /// already in the middle of updating it because the user changed it in the UI.
        /// </summary>
        /// <param name="d">The PasswordBox whose value was changed.</param>
        /// <param name="e">The event arguments</param>
        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox box = d as PasswordBox;

            // only handle this event when the property is attached to a PasswordBox
            // and when the BindPassword attached property has been set to true
            if (box == null || !GetBindPassword(box))
            {
                return;
            }

            // avoid recursive updating by ignoring the box's changed event
            box.PasswordChanged -= HandlePasswordChanged;

            Base64String newPassword = (Base64String)e.NewValue;

            if (!GetUpdatingPassword(box))
            {
                box.Password = newPassword;
            }

            box.PasswordChanged += HandlePasswordChanged;
        }

        /// <summary>
        /// Called when what the PasswordBox is bound to is changed, generally during UI initialization.
        /// We'll change our event listeners for the new PasswordBox.
        /// </summary>
        /// <param name="dp">The new PasswordBox whose bound property is being changed.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // when the BindPassword attached property is set on a PasswordBox,
            // start listening to its PasswordChanged event

            PasswordBox box = dp as PasswordBox;

            if (null != box)
            {
                bool wasBound = (bool)(e.OldValue);
                bool needToBind = (bool)(e.NewValue);

                if (wasBound)
                {
                    box.PasswordChanged -= HandlePasswordChanged;
                }

                if (needToBind)
                {
                    box.PasswordChanged += HandlePasswordChanged;
                }
            }
        }

        /// <summary>
        /// Called when the value of the PasswordBox is changed in the UI.
        /// We'll update the bound property with the updated password value.
        /// This triggers the OnBoundPasswordChanged event handler.
        /// </summary>
        /// <param name="sender">The PasswordBox whose value just changed.</param>
        /// <param name="e">The event arguments.</param>
        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox box = sender as PasswordBox;

            if (null != box)
            {
                // set a flag to indicate that we're updating the password
                SetUpdatingPassword(box, true);
                // push the new password into the BoundPassword property
                SetBoundPassword(box, box.Password);
                SetUpdatingPassword(box, false);
            }
        }

        #endregion

        #region Internal methods

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPassword, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPassword);
        }

        public static string GetBoundPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundPassword);
        }

        public static void SetBoundPassword(DependencyObject dp, Base64String value)
        {
            dp.SetValue(BoundPassword, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPassword);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPassword, value);
        }

        #endregion
    }
}
