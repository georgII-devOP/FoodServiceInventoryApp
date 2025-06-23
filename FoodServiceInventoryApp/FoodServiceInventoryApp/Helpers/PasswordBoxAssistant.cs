using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace FoodServiceInventoryApp.Helpers
{
    public static class PasswordBoxAssistant
    {
        public static readonly DependencyProperty BoundPasswordProperty =
              DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxAssistant), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxAssistant), new PropertyMetadata(false, OnIsMonitoringChanged));

        public static string GetBoundPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BoundPasswordProperty, value);
        }

        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wpf.Ui.Controls.PasswordBox passwordBox = d as Wpf.Ui.Controls.PasswordBox;
            if (passwordBox != null)
            {
                if (!object.Equals(passwordBox.Password, e.NewValue))
                {
                    passwordBox.Password = (string)e.NewValue;
                }
            }
        }

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Wpf.Ui.Controls.PasswordBox passwordBox = d as Wpf.Ui.Controls.PasswordBox;
            if (passwordBox == null) return;

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Wpf.Ui.Controls.PasswordBox passwordBox = sender as Wpf.Ui.Controls.PasswordBox;
            if (passwordBox != null)
            {
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}