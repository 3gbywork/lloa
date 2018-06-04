using OfficeAutomationClient.Helper;
using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var combobox = sender as ComboBox;
            if (null != combobox && !string.IsNullOrEmpty(combobox.Text))
                password.Password = Business.Instance.QueryPassword(combobox.Text).Text();
            else password.Password = string.Empty;
        }
    }
}
