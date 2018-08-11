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
            SimpleIoc.Default.Register<QueryControl>();

            SimpleIoc.Default.Register<EmailWindow>();
        }

        public static LoginWindow LoginWindow => SimpleIoc.Default.GetInstanceWithoutCaching<LoginWindow>();
        public static InfoWindow InfoWindow => ServiceLocator.Current.GetInstance<InfoWindow>();

        public static AboutWindow AboutWindow => SimpleIoc.Default.GetInstanceWithoutCaching<AboutWindow>();

        public static AttendanceInfoControl AttendanceInfo =>
            SimpleIoc.Default.GetInstanceWithoutCaching<AttendanceInfoControl>();
        public static QueryControl QueryControl => SimpleIoc.Default.GetInstanceWithoutCaching<QueryControl>();

        public static EmailWindow EmailWindow => SimpleIoc.Default.GetInstanceWithoutCaching<EmailWindow>();
    }
}