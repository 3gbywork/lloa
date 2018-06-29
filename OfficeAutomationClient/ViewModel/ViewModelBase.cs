using System;
using System.Runtime.CompilerServices;

namespace OfficeAutomationClient.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        public Func<bool> Activate { get; set; }
        public Action<bool> Close { get; set; }
        public Action Hide { get; set; }
        public Action Show { get; set; }
        public Func<bool?> ShowDialog { get; set; }

        public bool Set<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            return Set(propertyName, ref field, newValue);
        }
    }
}