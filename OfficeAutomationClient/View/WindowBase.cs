using OfficeAutomationClient.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

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
                    Close(result);
                };
                vm.Hide += Hide;
                vm.Show += Show;
                vm.ShowDialog += ShowDialog;
            };

            KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Escape)
                    Close(false);
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

        public T GetViewModel<T>() where T : ViewModelBase
        {
            return DataContext as T;
        }

        private void Close(bool result)
        {
            if (ShowStyle == ShowStyle.Show)
                Close();
            else
                this.DialogResult = result;
        }
    }

    public enum ShowStyle
    {
        Show,
        ShowDialog,
    }
}
