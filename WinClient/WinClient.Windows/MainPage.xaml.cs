using System.Collections.Generic;
using System.Net.Http;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json.Linq;
using WinClient.Common;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.MobileServices;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace WinClient
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        private MiniViewModel _miniViewModel = new MiniViewModel();

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this._miniViewModel.MobileServiceEndpointChoice = MobileServiceEndpoints.JavaScript;
            this.DataContext = _miniViewModel;

        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


       
       

        /// <summary>
        /// Handles the Click event of the LoginButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void LoginLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_miniViewModel.CurrentMobileServiceClient.CurrentUser == null || _miniViewModel.CurrentMobileServiceClient.CurrentUser.UserId == null)
            {
                // DEMO 1 - Authenticate every time, no caching
                await App.AuthenticateUserAsync(_miniViewModel.CurrentMobileServiceClient);

                // DEMO 2 - Leveraging password vault for cached credentials
                //await App.AuthenticateUserCachedTokenAsync(_miniViewModel.CurrentMobileServiceClient);

                LoginLogoutButton.Content = "Logout";
                LoginLogoutButton.Background = new SolidColorBrush(Colors.Green);
                DotNetRadioButton.IsEnabled = false;
                JavaScriptRadioButton.IsEnabled = false;
            }
            else
            {
                _miniViewModel.CurrentMobileServiceClient.Logout();

                LoginLogoutButton.Background = new SolidColorBrush(Colors.DarkRed);
                LoginLogoutButton.Content = "Login";
                DotNetRadioButton.IsEnabled = true;
                JavaScriptRadioButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the CallAPIGetButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private async void CallAPIGetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTextBlock.Text = "GET Request Made, waiting for response...";
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.White);
                StatusBorder.Background = new SolidColorBrush(Colors.Blue);

                // Let the user know something happening
                progressBar.IsIndeterminate = true;

                
                // Make the call to the hello resource asynchronously using GET verb
                var resultJson = await _miniViewModel.CurrentMobileServiceClient.InvokeApiAsync("hello", HttpMethod.Get, null);
                StatusBorder.Background = new SolidColorBrush(Colors.Green);
                StatusTextBlock.Text = "Request completed!";

                // Verfiy that a result was returned
                if (resultJson.HasValues)
                {
                    // Extract the value from the result
                    string messageResult = resultJson.Value<string>("message");

                    // Set the text block with the result
                    OutTextBlock.Text = messageResult;
                }
                else
                {
                    StatusBorder.Background = new SolidColorBrush(Colors.Orange);
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    OutTextBlock.Text = "Nothing returned!";
                }


            }
            catch (Exception ex)
            {
                // Display the exception message for the demo
                OutTextBlock.Text = "";
                StatusTextBlock.Text = ex.Message;
                StatusBorder.Background = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                // Let the user know something happening
                progressBar.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the CallAPIPostButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void CallAPIPostButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTextBlock.Text = "POST Request Made, waiting for response...";
                StatusTextBlock.Foreground = new SolidColorBrush(Colors.White);
                StatusBorder.Background = new SolidColorBrush(Colors.Blue);

                // Let the user know something happening
                progressBar.IsIndeterminate = true;

                
                // Create the json to send using an anonymous type 
                JToken payload = JObject.FromObject(new { msg = "hello joe"});
                // Make the call to the hello resource asynchronously using POST verb
                var resultJson = await _miniViewModel.CurrentMobileServiceClient.InvokeApiAsync("hello", payload);

                StatusBorder.Background = new SolidColorBrush(Colors.Green);
                StatusTextBlock.Text = "Request completed!";

                // Verify that a result was returned
                if (resultJson.HasValues)
                {
                    // Extract the value from the result
                    string messageResult = resultJson.Value<string>("message");

                    // Set the text block with the result
                    OutTextBlock.Text = messageResult;
                }
                else
                {
                    StatusBorder.Background = new SolidColorBrush(Colors.Orange);
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                    OutTextBlock.Text = "Nothing returned!";
                }


            }
            catch (Exception ex)
            {
                // Display the exception message for the demo
                OutTextBlock.Text = "";
                StatusTextBlock.Text = ex.Message;
                StatusBorder.Background = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                // Let the user know something happening
                progressBar.IsIndeterminate = false;
            }
        }
      
    }
}
