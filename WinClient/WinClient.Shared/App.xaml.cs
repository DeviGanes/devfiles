using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.MobileServices;

namespace WinClient
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif
        // CLASS DEMO

        // Mobile service client instance

        // JavaScript Mobile Service
        public static MobileServiceClient MobileServiceJavaScript = new MobileServiceClient("https://csci-e64-cusapi-a-edu.azure-mobile.net/", "dBYhhtCxvmWhRVMgOHbCyaPzWOhMfs94");

        // local instance (API Key doesn't matter)
        public static MobileServiceClient MobileServiceLocal = new MobileServiceClient("http://localhost:28007/", "VDzJvceaeicPgfhIuoMTZJFMMAWKSQ22");
        
        // .NET Mobile Service
        public static MobileServiceClient MobileServiceDotNet = new MobileServiceClient("https://cse64-dganesan-blogauthentication.azure-mobile.net/", "oArwoOnOWQyAuDVIDDbHFbXjnPNFfj57");

        
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        private static MobileServiceClient _currentMobileServiceClient;

        /// <summary>
        /// authenticate as an asynchronous operation.
        /// </summary>
        /// <param name="mobileServiceClient">The mobile service client.</param>
        /// <param name="providerType">Type of the provider to authenticate with, default is twitter.</param>
        /// <returns>Task.</returns>
        public static async Task AuthenticateUserAsync(MobileServiceClient mobileServiceClient, MobileServiceAuthenticationProvider providerType = MobileServiceAuthenticationProvider.Twitter)
        {
            _currentMobileServiceClient = mobileServiceClient;

            while (mobileServiceClient.CurrentUser == null || mobileServiceClient.CurrentUser.UserId == null)
            {
                string message;

                try
                {
                    // Authenticate using provided provider type.
                    await mobileServiceClient.LoginAsync(providerType);
                    message = string.Format("You are now logged in - {0}", mobileServiceClient.CurrentUser.UserId);
                }
                catch (InvalidOperationException ex)
                {
                    message = "You must log in. Login Required" + ex.Message;
                }

                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();

            }
        }


        /// <summary>
        /// Authenticate user as an asynchronous operation caching the authorization on client token
        /// </summary>
        /// <param name="mobileServiceClient">The mobile service client.</param>
        /// <param name="providerType">Type of the provider.</param>
        /// <returns>Task.</returns>
        public static async Task AuthenticateUserCachedTokenAsync(MobileServiceClient mobileServiceClient, MobileServiceAuthenticationProvider providerType = MobileServiceAuthenticationProvider.Twitter)
        {
            _currentMobileServiceClient = mobileServiceClient;

            // Use the PasswordVault to securely store and access credentials.
            PasswordVault passwordVault = new PasswordVault();
            PasswordCredential passwordCredential = null;

            try
            {
                // Try to get an existing credential from the vault.
                passwordCredential = passwordVault.FindAllByResource(providerType.ToString()).FirstOrDefault();
            }
            catch (Exception ex)
            {
                // If there is no matching resource an error occurs
                // This is safe to ignore.
                Debug.WriteLine("Credential not in vault exception: {0}",ex.Message);
            }


            // There is a password in the vault, get it from the vault, verify that it's still valid
            if (passwordCredential != null)
            {
                _currentMobileServiceClient.CurrentUser = await GetUserFromVault(passwordVault, passwordCredential);
            }

            // If we have a user then we are done, if not then prompt the user to login
            // and save the credentials in the vault
            while (mobileServiceClient.CurrentUser == null || mobileServiceClient.CurrentUser.UserId == null)
            {
                string message;

                try
                {
                    // Authenticate using provided provider type.
                    MobileServiceUser mobileServiceUser = await mobileServiceClient.LoginAsync(providerType);
                    message = string.Format("You are now logged in - {0}", mobileServiceClient.CurrentUser.UserId);


                    // Create the credential package to store in the password vault
                    passwordCredential = new PasswordCredential(providerType.ToString(),
                                                mobileServiceUser.UserId, 
                                                mobileServiceUser.MobileServiceAuthenticationToken);

                    // Add the credential package to the vault
                    passwordVault.Add(passwordCredential);

                }
                catch (InvalidOperationException ex)
                {
                    message = "You must log in. Login Required" + ex.Message;
                }

                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();

            }
        }

        /// <summary>
        /// Gets the user from vault.
        /// </summary>
        /// <param name="passwordVault">The password vault.</param>
        /// <param name="passwordCredential">The password credential.</param>
        /// <returns>Task&lt;MobileServiceUser&gt;.</returns>
        private async static Task<MobileServiceUser> GetUserFromVault(PasswordVault passwordVault, PasswordCredential passwordCredential)
        {
            // Create a user from the stored credentials.
            MobileServiceUser mobileServiceUser = new MobileServiceUser(passwordCredential.UserName);
            passwordCredential.RetrievePassword();
            mobileServiceUser.MobileServiceAuthenticationToken = passwordCredential.Password;

            // Set the user from the stored credentials.
            _currentMobileServiceClient.CurrentUser = mobileServiceUser;

            try
            {
                // Try to make a call to verify that the credential has not expired
                await _currentMobileServiceClient.InvokeApiAsync("Hello",HttpMethod.Get,null);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Remove the credential with the expired token.
                    passwordVault.Remove(passwordCredential);
                    mobileServiceUser = null;
                }
            }

            return mobileServiceUser;
        }


        /// <summary>
        /// Handles the <see cref="E:Activated" /> event.
        /// </summary>
        /// <param name="args">The <see cref="IActivatedEventArgs"/> instance containing the event data.</param>
        /// <remarks>Critical for Universal Windows Phone Apps without this the authentication does not finsh properly
        /// and you are just redirected back to your main page.</remarks>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

#if WINDOWS_PHONE_APP
            if (args.Kind == ActivationKind.WebAuthenticationBrokerContinuation && _currentMobileServiceClient != null)
            {
                _currentMobileServiceClient.LoginComplete(args as WebAuthenticationBrokerContinuationEventArgs);
            }
#endif
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

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

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}