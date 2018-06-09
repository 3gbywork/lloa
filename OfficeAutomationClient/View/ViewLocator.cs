using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.View
{
    public class ViewLocator
    {
        static ViewLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<LoginWindow>();
            SimpleIoc.Default.Register<InfoWindow>();
        }

        public static LoginWindow LoginWindow => ServiceLocator.Current.GetInstance<LoginWindow>();
        public static InfoWindow InfoWindow => ServiceLocator.Current.GetInstance<InfoWindow>();
    }
}
