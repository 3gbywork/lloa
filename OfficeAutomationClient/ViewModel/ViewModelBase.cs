using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace OfficeAutomationClient.ViewModel
{
    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase
    {
        public Func<bool> Activate { get; set; }
        public Action<bool> Close { get; set; }
        public Action Hide { get; set; }
        public Action Show { get; set; }
        public Func<bool?> ShowDialog { get; set; }

        public bool Set<T>(ref T field, T newValue, [CallerMemberName]string propertyName = "")
        {
            return Set<T>(propertyName, ref field, newValue);
        }
    }
}
