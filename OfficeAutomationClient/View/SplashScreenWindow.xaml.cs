using System;
using System.Windows;
using System.Windows.Media.Animation;
using XamlAnimatedGif;

namespace OfficeAutomationClient.View
{
    /// <summary>
    /// SplashScreenWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow(string resourceName)
        {
            InitializeComponent();

            AnimationBehavior.SetSourceUri(SplashScreen, new Uri(resourceName, UriKind.RelativeOrAbsolute));
            AnimationBehavior.SetRepeatBehavior(SplashScreen, RepeatBehavior.Forever);
        }

        private void SplashScreenWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            Left = (workArea.Width - e.NewSize.Width) / 2 + workArea.Left;
            Top = (workArea.Height - e.NewSize.Height) / 2 + workArea.Top;
        }
    }
}
