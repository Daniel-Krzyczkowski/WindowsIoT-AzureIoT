using AzureIoT.DigitalTwins.Core.Config;
using AzureIoT.DigitalTwins.Core.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AzureIoT.DigitalTwins.UWP.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<AppViews, Type> _pagesByKey = new Dictionary<AppViews, Type>();
        private Frame _currentRootView;
        private Frame _rootView;

        public string CurrentPageKey
        {
            get
            {
                lock (_pagesByKey)
                {
                    if (_currentRootView.CurrentSourcePageType == null)
                    {
                        return null;
                    }

                    var pageType = _currentRootView.CurrentSourcePageType;

                    return _pagesByKey.ContainsValue(pageType)
                                      ? _pagesByKey.First(p => p.Value == pageType).Key.ToString() : null;
                }
            }
        }

        public void GoBack()
        {
            _currentRootView.GoBack();
        }

        public void NavigateTo(AppViews pageKey)
        {
            NavigateTo(pageKey, null);
        }

        public void NavigateTo(AppViews pageKey, object parameter)
        {
            lock (_pagesByKey)
            {

                if (_pagesByKey.ContainsKey(pageKey))
                {
                    var type = _pagesByKey[pageKey];
                    ConstructorInfo constructor;
                    object[] parameters = new object[] { };

                    constructor = type.GetTypeInfo()
                        .DeclaredConstructors
                        .FirstOrDefault();

                    if (constructor == null)
                    {
                        throw new InvalidOperationException(
                            "No suitable constructor found for page " + pageKey);
                    }

                    var page = constructor.Invoke(parameters) as Page;
                    _currentRootView.Navigate(page.GetType(), parameter);
                }
                else
                {
                    throw new ArgumentException(
                        string.Format(
                            "No such page: {0}. Did you forget to call NavigationService.Configure?",
                            pageKey), nameof(pageKey));
                }
            }
        }

        public void Configure(AppViews pageKey, Type pageType)
        {
            lock (_pagesByKey)
            {
                if (_pagesByKey.ContainsKey(pageKey))
                {
                    _pagesByKey[pageKey] = pageType;
                }
                else
                {
                    _pagesByKey.Add(pageKey, pageType);
                }
            }
        }

        public void InitializeCurrentRootView(object currentRootView)
        {
            if (currentRootView.GetType() != typeof(Frame))
            {
                throw new InvalidOperationException(
                           "Root view should be set to type of: " + typeof(Frame));
            }
            else
            {
                _currentRootView = currentRootView as Frame;
            }
        }

        public void InitializeRootView(object rootView)
        {
            if (rootView.GetType() != typeof(Frame))
            {
                throw new InvalidOperationException(
                           "Root view should be set to type of: " + typeof(Frame));
            }
            else
            {
                _rootView = rootView as Frame;
            }
        }

        public void SwitchToRootViewAsMainNavigationView()
        {
            _currentRootView = _rootView;
        }

        public void ClearViewStack()
        {
            _rootView.BackStack.Clear();
            _currentRootView.BackStack.Clear();
        }
    }
}
