using AzureIoT.DigitalTwins.Core.Authentication;
using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.Navigation;
using AzureIoT.DigitalTwins.Core.Providers;
using AzureIoT.DigitalTwins.Core.Services;
using AzureIoT.DigitalTwins.Core.ViewModel;
using AzureIoT.DigitalTwins.Management.Actions;
using AzureIoT.DigitalTwins.Management.API;
using AzureIoT.DigitalTwins.Management.Providers;
using AzureIoT.DigitalTwins.UWP.Navigation;
using AzureIoT.DigitalTwins.UWP.Providers;
using AzureIoT.DigitalTwins.UWP.Services;
using AzureIoT.DigitalTwins.UWP.Views;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace AzureIoT.DigitalTwins.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            RegisterDependencies();
            this.Suspending += OnSuspending;
        }

        private void RegisterDependencies()
        {
            SimpleIoc.Default.Register<ILogger>(() => Log.Logger);
            SimpleIoc.Default.Register<IAuthenticationService, AuthenticationService>();
            SimpleIoc.Default.Register<IDigitalTwinsApiConnector, DigitalTwinsApiConnector>();
            SimpleIoc.Default.Register<IDigitalTwinsManagementService, DigitalTwinsManagementService>();
            SimpleIoc.Default.Register<IRoomDetailsService, RoomDetailsService>();
            SimpleIoc.Default.Register<IPlatformParameters>(() => new PlatformParameters(PromptBehavior.Auto, false));
            SimpleIoc.Default.Register<IConfigurationProvider, ConfigurationProvider>();
            SimpleIoc.Default.Register<IAccessTokenProvider, AccessTokenProvider>();
            SimpleIoc.Default.Register<ISpaceDefinitionProvider, SpaceDefinitionProvider>();
            SimpleIoc.Default.Register<IUserDefinedActionProvider, UserDefinedActionProvider>();
            SimpleIoc.Default.Register<IDialogService, DialogService>();

            SimpleIoc.Default.Register<LoginViewModel>(true);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SpaceConfigurationViewModel>();
            SimpleIoc.Default.Register<Level0SpaceManagementViewModel>();
            SimpleIoc.Default.Register<Level2SpaceManagementViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            //TODO: Use this line to get redirect URL for the app which should be added in the registered applicaton in the Azure AD:
            //var redirectURI = Windows.Security.Authentication.Web.WebAuthenticationBroker.GetCurrentApplicationCallbackUri();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(LoginPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

            InitializeNavigation(rootFrame);
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        private void InitializeNavigation(Frame rootFrame)
        {
            INavigationService navigationService;

            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                // Setup navigation service:
                navigationService = new NavigationService();

                // Configure pages:
                navigationService.Configure(AppViews.LoginView, typeof(LoginPage));
                navigationService.Configure(AppViews.MainView, typeof(MainPage));
                navigationService.Configure(AppViews.Level0SpaceManagement, typeof(Level0SpaceManagementPage));
                navigationService.Configure(AppViews.Level2SpaceManagement, typeof(Level2SpaceManagementPage));
                navigationService.Configure(AppViews.SpacesConfigurationView, typeof(SpacesConfigurationPage));
                navigationService.Configure(AppViews.SettingsView, typeof(SettingsPage));

                // Register NavigationService in IoC container:
                SimpleIoc.Default.Register<INavigationService>(() => navigationService);
            }

            else
            {
                navigationService = SimpleIoc.Default.GetInstance<INavigationService>();
            }

            // Set Navigation page as default page for Navigation Service:
            navigationService.InitializeRootView(rootFrame);
            navigationService.InitializeCurrentRootView(rootFrame);
        }
    }
}
