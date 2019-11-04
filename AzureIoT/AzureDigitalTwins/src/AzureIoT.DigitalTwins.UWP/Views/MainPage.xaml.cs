using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AzureIoT.DigitalTwins.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Compositor _compositor = Window.Current.Compositor;
        private SpringVector3NaturalMotionAnimation _springAnimation;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ((MainViewModel)DataContext).NavigationService.InitializeCurrentRootView(rootFrame);
                ((MainViewModel)DataContext).NavigationInvoked(AppViews.SettingsView);
            }

            else if (args.InvokedItem.ToString() == "Space Configuration")
            {
                ((MainViewModel)DataContext).NavigationService.InitializeCurrentRootView(rootFrame);
                ((MainViewModel)DataContext).NavigationInvoked(AppViews.SpacesConfigurationView);
            }

            else if (args.InvokedItem.ToString() == "Level 0 Space Management")
            {
                ((MainViewModel)DataContext).NavigationService.InitializeCurrentRootView(rootFrame);
                ((MainViewModel)DataContext).NavigationInvoked(AppViews.Level0SpaceManagement);
            }

            else if (args.InvokedItem.ToString() == "Level 2 Space Management")
            {
                ((MainViewModel)DataContext).NavigationService.InitializeCurrentRootView(rootFrame);
                ((MainViewModel)DataContext).NavigationInvoked(AppViews.Level2SpaceManagement);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //((MainViewModel)DataContext).SensorsParameters = (SensorsParameters)e.Parameter;
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = _compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void element_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // Scale up to 1.5
            CreateOrUpdateSpringAnimation(1.5f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }

        private void element_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Scale back down to 1.0
            CreateOrUpdateSpringAnimation(1.0f);

            (sender as UIElement).StartAnimation(_springAnimation);
        }
    }
}
