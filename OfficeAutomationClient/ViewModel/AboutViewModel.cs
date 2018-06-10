using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OfficeAutomationClient.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string Title => $"关于 {Business.AssemblyName}";
    }
}
