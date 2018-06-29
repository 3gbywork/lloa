using OfficeAutomationClient.OA;

namespace OfficeAutomationClient.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string Title => $"关于 {Business.AssemblyName}";
    }
}