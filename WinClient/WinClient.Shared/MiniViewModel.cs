using System;
using System.ComponentModel;
using Microsoft.WindowsAzure.MobileServices;

namespace WinClient
{
    public class MiniViewModel : INotifyPropertyChanged
    {
        private MobileServiceEndpoints _mobileServiceEndPointChoiceEndpoints = MobileServiceEndpoints.JavaScript;

        /// <summary>
        /// Gets or sets the mobile service endpoint choice.
        /// </summary>
        /// <value>The mobile service endpoint choice.</value>
        public MobileServiceEndpoints MobileServiceEndpointChoice
        {
            get
            {
                return _mobileServiceEndPointChoiceEndpoints;
            }
            set
            {

                _mobileServiceEndPointChoiceEndpoints = value;
                OnPropertyChanged("MobileServiceEndpointChoice");
                OnPropertyChanged("LoggedIn");
                OnPropertyChanged("LoggedOut");

            }
        }

        /// <summary>
        /// Gets a value indicating whether [LoggedIn].
        /// </summary>
        /// <value><c>true</c> if [LoggedIn]; otherwise, <c>false</c>.</value>
        public bool LoggedIn
        {
            get
            {
                return CurrentMobileServiceClient != null &&
                    CurrentMobileServiceClient.CurrentUser != null &&
                       CurrentMobileServiceClient.CurrentUser.UserId != null;
            }
        }

        private MobileServiceClient _lastMobileServiceSelected;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Gets the mobile service client.
        /// </summary>
        /// <value>The mobile service client.</value>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public MobileServiceClient CurrentMobileServiceClient
        {
            get
            {

                switch (MobileServiceEndpointChoice)
                {
                    case MobileServiceEndpoints.DotNet:
                        return App.MobileServiceDotNet;

                    case MobileServiceEndpoints.JavaScript:
                        return App.MobileServiceJavaScript;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
