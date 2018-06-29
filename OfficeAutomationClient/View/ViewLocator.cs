using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace OfficeAutomationClient.View
{
    public class ViewLocator
    {
        static ViewLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<LoginWindow>();
            SimpleIoc.Default.Register<InfoWindow>();

            SimpleIoc.Default.Register<AboutWindow>();
            SimpleIoc.Default.Register<AttendanceInfoControl>();
        }

        public static LoginWindow LoginWindow => SimpleIoc.Default.GetInstanceWithoutCaching<LoginWindow>();
        public static InfoWindow InfoWindow => ServiceLocator.Current.GetInstance<InfoWindow>();

        public static AboutWindow AboutWindow => SimpleIoc.Default.GetInstanceWithoutCaching<AboutWindow>();

        public static AttendanceInfoControl AttendanceInfo =>
            SimpleIoc.Default.GetInstanceWithoutCaching<AttendanceInfoControl>();
    }
}