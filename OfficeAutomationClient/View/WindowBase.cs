using OfficeAutomationClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OfficeAutomationClient.View
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            DataContextChanged += delegate
            {
                var vm = DataContext as ViewModelBase;
                vm.Activate += Activate;
                vm.Close += (result) =>
                {
                    if (ShowStyle == ShowStyle.Show)
                        Close();
                    else
                        this.DialogResult = result;
                };
                vm.Hide += Hide;
                vm.Show += Show;
                vm.ShowDialog += ShowDialog;
            };
        }

        public ShowStyle ShowStyle { get; private set; }

        public bool? ShowDialog(Window owner = null)
        {
            ShowStyle = ShowStyle.ShowDialog;
            //ShowInTaskbar = false;
            Owner = owner;

            return base.ShowDialog();
        }

        public void Show(Window owner = null)
        {
            ShowStyle = ShowStyle.Show;
            //ShowInTaskbar = true;
            Owner = owner;

            base.Show();
        }
    }

    public enum ShowStyle
    {
        Show,
        ShowDialog,
    }
}
