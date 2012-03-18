using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Browser;
using System.ServiceModel.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Browser;
using Facebook;
using System.Text;
using GameNick_Web.GameNickService;
using System.Data.Services;

namespace GameNick_Web
{
    internal enum MainTabSelectedTab
    {
        Friends,
        Me,
        Events
    };

    internal enum EventsTabSelectedTab
    {
        Create,
        View
    };

    [ScriptableType]
    public partial class MainPage
    {
        private GameNickServiceClient _gnsClient;
        private User _userMe;
        private ChildWindowAddGameNick _childWindow;
        private ObservableCollection<User> _friends;
        private ObservableCollection<Platform> _platforms;
        private ObservableCollection<User> Friends
        {
            get { return _friends; }
            set { _friends = value;
                listBoxFriendsFriends.ItemsSource = value;
            }
        }
        private ObservableCollection<Platform> Platforms { 
            get { return _platforms; } 
            set { _platforms = value;
                listBoxEventsCreatePlatform.ItemsSource = value;
                listBoxBrowsePlatforms.ItemsSource = value;
                _childWindow.listBoxPlatform.ItemsSource = value;
            }
        }
        private User UserMe
        {
            get { return _userMe; }
            set { _userMe = value;
                dataGridMeGameNicks.ItemsSource = _userMe.GameNicks;
                dataGridMePastStatus.ItemsSource = _userMe.Status;
            }
        }

        private Guid GameNickAccessToken
        {
            get { return (Guid)UserMe.GameNickAccessToken; }
        }

        public MainPage()
        {
            InitializeComponent();
            HtmlPage.RegisterScriptableObject("slObject", this);
            WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
            MainGrid.Visibility = Visibility.Collapsed;
            TextLoading.Visibility = Visibility.Collapsed;
            TextBlockLoadingStatus.Visibility = Visibility.Collapsed;
          

        }

        #region LOGIN

        private const string AppId = "212683568783078";
        /// <summary>
        /// Extended permissions is a comma separated list of permissions to ask the user.
        /// </summary>
        /// <remarks>
        /// For extensive list of available extended permissions refer to 
        /// https://developers.facebook.com/docs/reference/api/permissions/
        /// </remarks>
        private const string ExtendedPermissions = "user_about_me,friends_about_me,user_events,friends_events,create_event,publish_stream,rsvp_event";

        // Note:
        // Host CS-SL4-InBrowser.Web in IIS (or IIS express) and not cassini (visual studio web server).
        // and change this url accordingly.
        // make sure you run this sample from the same domain where SilverlightFacebookCallback.aspx file is located
        // i.e. http://localhost:1530/
        // make sure to also set the SiteUrl to http://localhost:1530/
        private const string SilverlightFacebookCallback = "http://gamenick.net/GameNickFacebookCallback.aspx";

        private string _facebookAccessToken;

        public string FacebookAccessToken
        {
            get { return _facebookAccessToken; }
            set { _facebookAccessToken = value; }
        }

        private void ButtonLoginWithFacebookClick(object sender, RoutedEventArgs e)
        {
            ButtonLoginWithFacebook.Visibility = Visibility.Collapsed;
            TextLoading.Visibility = Visibility.Visible;
            TextBlockLoadingStatus.Visibility = Visibility.Visible;
            // release
            LoginToFbViaJs();
            TextBlockLoadingStatus.Text += "\nAwaiting your login!\n";
            // end release
            // debug
            //_facebookAccessToken =
                
               
            //LoadPage();
            // end debug
        }

        [ScriptableMember]
        public void LoginComplete(string value)
        {
            var result = (IDictionary<string, object>)JsonSerializer.Current.DeserializeObject(value);
            string errorDescription = (string)result["error_description"];
            string accessToken = (string)result["access_token"];

            if (string.IsNullOrEmpty(errorDescription))
            {
                LoginSucceeded(accessToken);
            }
            else
            {
                MessageBox.Show(errorDescription);
            }
        }

        private void LoginSucceeded(string accessToken)
        {
            _facebookAccessToken = accessToken;
            LoadPage();
        }

        private void LoginToFbViaJs()
        {
            var loginParameters = new Dictionary<string, object>
                                      {
                                          { "display", "popup" },
                                          { "response_type", "code" } // make it code and not access token for security reasons.
                                      };

            loginParameters["scope"] = ExtendedPermissions;

            var oauthClient = new FacebookOAuthClient { AppId = AppId, RedirectUri = new Uri(SilverlightFacebookCallback) };

            var loginUrl = oauthClient.GetLoginUrl(loginParameters);

            // don't make the response_type = token
            // coz it will be saved in the browser's history.
            // so others might hack it.
            // rather call ExchangeCodeForAccessToken to get access token in server side.
            // we need to this in server side and not in this Silverlight app
            // so that the app secret doesn't get exposed to the client in case someone
            // reverse engineers this Silverlight app.
            HtmlPage.Window.Eval(string.Format("fbLogin('{0}')", loginUrl));
        }
        #endregion

       
        #region GnsClientAsyncCompletedCallBacks

        private void SetCallBacks()
        {
            _gnsClient.OpenCompleted                    +=      GnsClientOnOpenCompleted;
            _gnsClient.AuthenticateCompleted            +=      GnsClientOnAuthenticateCompleted;
            _gnsClient.AddGameNickCompleted             +=      GnsClientOnAddGameNickCompleted;
            _gnsClient.GetFriendsCompleted              +=      GnsClientOnGetFriendsCompleted;
            _gnsClient.GetGameNicksCompleted            +=      GnsClientOnGetGameNicksCompleted;
            _gnsClient.GetEventsForUserCompleted        +=      GnsClientOnGetEventsForUserCompleted;
            _gnsClient.GetStatusCompleted               +=      GnsClientOnGetStatusCompleted;
            _gnsClient.GetPlatformsCompleted            +=      GnsClientOnGetPlatformsCompleted;
            _gnsClient.GetEventsForGameCompleted        +=      GnsClientOnGetEventsForGameCompleted;
            _gnsClient.CreateEventCompleted             +=      GnsClientOnCreateEventCompleted;
            _gnsClient.UpdateStatusCompleted            +=      GnsClientOnUpdateStatusCompleted;
            _gnsClient.RemoveGameNickCompleted          +=      GnsClientOnRemoveGameNickCompleted;
            _gnsClient.GetGamesCompleted                +=      GnsClientOnGetGamesCompleted;
            _gnsClient.GetFriendsWithGameCompleted      +=      GnsClientGetFriendsWithGameCompleted;
            _gnsClient.GetStatusWithGameCompleted       +=      GnsClientGetStatusWithGameCompleted;
        }

        private void GnsClientGetStatusWithGameCompleted(object sender, GetStatusWithGameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dataGridBrowseStatus.ItemsSource = e.Result;
            }
        }

        private void GnsClientGetFriendsWithGameCompleted(object sender, GetFriendsWithGameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                dataGridBrowseFriends.ItemsSource = e.Result;
            }
        }

        private void GnsClientOnGetGamesCompleted(object sender, GetGamesCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                listBoxBrowseGames.ItemsSource = e.Result;
            }
        }

        private void GnsClientOnRemoveGameNickCompleted(object sender, RemoveGameNickCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                UserMe = e.Result;
            }
        }

        private void GnsClientOnAddGameNickCompleted(object sender, AddGameNickCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                UserMe = e.Result;

            }
        }

        private void GnsClientOnUpdateStatusCompleted(object sender, UpdateStatusCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                UserMe = e.Result;
            }
        }


        private void GnsClientOnCreateEventCompleted(object sender, CreateEventCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                ChildWindowEventCreated childWindow = new ChildWindowEventCreated();
                childWindow.hyperlinkButtonViewEvent.NavigateUri = new Uri("http://facebook.com/events/" + e.Result.ToString());
                childWindow.Show();
            }
        }

        private void GnsClientOnGetPlatformsCompleted(object sender, GetPlatformsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Platforms = e.Result;
            }
        }

        private void GnsClientOnGetEventsForGameCompleted(object sender, GetEventsForGameCompletedEventArgs e)
        {
            if (e.Result != null && e.Error == null)
            {
                dataGridBrowseEvents.ItemsSource = e.Result;
            }
        }

        private void GnsClientOnOpenCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {

                _gnsClient.AuthenticateAsync(FacebookAccessToken);
            }
            else
            {
                throw e.Error;
            }
        }

        private void GnsClientOnAuthenticateCompleted(object sender, AuthenticateCompletedEventArgs e)
        {
            if (e.Result != null && e.Error == null)
            {
                UserMe = e.Result;
                MainGrid.Visibility = Visibility.Visible;
                WelcomeGrid.Visibility = Visibility.Collapsed;
                _childWindow = new ChildWindowAddGameNick( _gnsClient, UserMe);
                LoadTabFriends();
            }
        }

        private void GnsClientOnGetFriendsCompleted(object sender, GetFriendsCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                Friends = e.Result;
            }
        }

        private void GnsClientOnGetEventsForUserCompleted(object sender, GetEventsForUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                dataGridFriendsEvents.ItemsSource = e.Result;
            }
        }

        private void GnsClientOnGetGameNicksCompleted(object sender, GetGameNicksCompletedEventArgs e)
        {
            if (e.Result != null && e.Error == null)
            {
                dataGridFriendsGameNicks.ItemsSource = e.Result;
                dataGridMeGameNicks.ItemsSource = e.Result;
            }
        }

        private void GnsClientOnGetStatusCompleted(object sender, GetStatusCompletedEventArgs e)
        {
            if (e.Result != null && e.Error == null)
            {
                    dataGridFriendsStatus.ItemsSource = e.Result;
                    dataGridMePastStatus.ItemsSource = e.Result;
            }
        }
        #endregion

        #region Events
        
        private void ChildWindowAddGameNickOnClosed(object sender, EventArgs e)
        {
            ChildWindowAddGameNick addGameNick = (ChildWindowAddGameNick) sender;
            if (addGameNick.DialogResult == true && addGameNick.textBoxName.Text != string.Empty && addGameNick.listBoxGame.SelectedItem != null && addGameNick.listBoxPlatform.SelectedItem != null)
            {
                GameNick gn = new GameNick()
                                  {
                                      GameID = ((Game) addGameNick.listBoxGame.SelectedItem).ID,
                                      UserID = UserMe.ID,
                                      Name = addGameNick.textBoxName.Text
                                  };
                UserMe.GameNicks.Add(gn);
                _gnsClient.AddGameNickAsync(UserMe, gn);
            }
        }

        private void ListBoxBrowseGamesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxBrowseGames.SelectedItem != null)
            {
                // Get Game ID
                int selectedGameId = ((Game) listBoxBrowseGames.SelectedItem).ID;
                // Events
                dataGridBrowseEvents.ItemsSource = new List<string>() { "Feature coming soon."}; // set when the async returns 
                // Friends
                _gnsClient.GetFriendsWithGameAsync(GameNickAccessToken, selectedGameId);
                // Status
                _gnsClient.GetStatusWithGameAsync(GameNickAccessToken, selectedGameId);
            }
        }

        private void ListBoxBrowsePlatformsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxBrowsePlatforms.SelectedItem != null)
            {
                int platformId = ((Platform) listBoxBrowsePlatforms.SelectedItem).ID;
                _gnsClient.GetGamesAsync(GameNickAccessToken, platformId);
            }
        }

        private void ListBoxEventsCreateGameSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxEventsCreateGame.SelectedItem != null)
            {
                Game game = (Game)listBoxEventsCreateGame.SelectedItem;
                List<User> eventFriends = new List<User>();
                foreach (User friend in Friends)
                {
                    var result = friend.GameNicks.Where(gamenick => gamenick.GameID == game.ID);
                    if(result.Count() > 0)
                        eventFriends.Add(result.First().User);
                }
                dataGridEventsCreateFriends.ItemsSource = eventFriends;
            }
        }

        private void ListBoxEventsCreatePlatformSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxEventsCreatePlatform.SelectedItem != null)
            {
                listBoxEventsCreateGame.ItemsSource = ((Platform) listBoxEventsCreatePlatform.SelectedItem).Games;
            }
        }

        private void ListBoxFriendsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(listBoxFriendsFriends.SelectedItem != null)
            {
                // Update the entire friend view grid
                User selectedUser = (User) listBoxFriendsFriends.SelectedItem;
                _gnsClient.GetStatusAsync(GameNickAccessToken, selectedUser.ID);
                // status
                //dataGridFriendsStatus.ItemsSource = selectedUser.Status;
                // events
                // the events are actually in the user!
                //List<GamingEvent> userEvents = new List<GamingEvent>();
                //FacebookClient client = new FacebookClient(UserMe.FacebookAccessToken);
                //foreach (GameNick gn in selectedUser.GameNicks)
                //{
                //    if (gn.Game.Events != null)
                //    {
                //        foreach (Event eEvent in gn.Game.Events)
                //        {
                //            long fbId = eEvent.FacebookID;
                //            client.GetCompleted +=new EventHandler<FacebookApiEventArgs>(client_GetCompleted);
                //            client.GetAsync(fbId.ToString(), null, eEvent);
                //        }
                //    }
                //}
                //_gnsClient.GetEventsForUserAsync(UserMe, selectedUser);
                dataGridFriendsEvents.ItemsSource = new List<string>() {"Feature coming soon."};
                // gamenicks
                _gnsClient.GetGameNicksAsync(GameNickAccessToken, selectedUser.ID);
                hyperLinkFriend.NavigateUri = new Uri("http://facebook.com/profile.php?id="+selectedUser.FacebookID.ToString());
            }
        }

        private void client_GetCompleted(object sender, FacebookApiEventArgs e)
        {
            dynamic items = dataGridFriendsEvents.ItemsSource ?? new List<GamingEvent>();
            dynamic result = e.GetResultData();
            Event eEvent = (Event)e.UserState;
            GamingEvent gEvent  = new GamingEvent();
            gEvent.Event = eEvent;
            if(result.description != null)
                gEvent.Description = result.description.ToString();
            if(result.end_time != null)
                gEvent.EndTime = DateTime.Parse(result.end_time.ToString());
            if (result.start_time != null)
                gEvent.StartTime = DateTime.Parse(result.start_time.ToString());
            if (result.name != null)
                gEvent.Name = result.name.ToString();
            if (result.location != null)
                gEvent.Location = result.location.ToString();
            items.Add(gEvent);
            dataGridFriendsEvents.ItemsSource = items;
        }

        private void MainTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl == null)
                return;
            int selectedTab = MainTabControl.SelectedIndex;
            switch (selectedTab)
            {
                    // Friend tab
                case 0:
                    LoadTabFriends();
                    break;
                    // My profile tab
                case 1:
                    LoadTabMe();
                    break;
                    // Events tab
                case 2:
                    LoadTabEvents();
                    break;
                case 3:
                    LoadTabBrowse();
                    break;
            }
        }
        #endregion

        #region Loading

        private void LoadTabBrowse()
        {
            _gnsClient.GetPlatformsAsync(GameNickAccessToken);
        }

        private void LoadTabEvents()
        {
            // Tab Create
            _gnsClient.GetPlatformsAsync(GameNickAccessToken);
            textBoxEventsCreateEventDetails.Text = "Details";
            textBoxEventsCreateEventName.Text = "Name";
        }

        private void LoadTabFriends()
        {
            _gnsClient.GetFriendsAsync(UserMe);
            _gnsClient.GetPlatformsAsync(GameNickAccessToken);
        }

        private void LoadTabMe()
        {
            _gnsClient.GetGameNicksAsync(GameNickAccessToken, UserMe.ID);
            _gnsClient.GetStatusAsync(GameNickAccessToken, UserMe.ID);
        }

        private void LoadPage()
        {
            _gnsClient = new GameNickServiceClient();
            // debug only;
            //_gnsClient.Endpoint.Binding.SendTimeout = new TimeSpan(0, 10, 0);
            //_gnsClient.Endpoint.Binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            // end debug only
            SetCallBacks();
            _gnsClient.OpenAsync();
            TextBlockLoadingStatus.Text += "Authenticating";
        }        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            // Do not load your data at design time.
            // if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            // {
            // 	//Load your data here and assign the result to the CollectionViewSource.
            // 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
            // 	myCollectionViewSource.Source = your data
            // }
        }
#endregion
        #region Clicks
        private void ButtonEventsCreateSubmitNewEventClick(object sender, RoutedEventArgs e)
        {
            if ((listBoxEventsCreatePlatform.SelectedItem != null) && (listBoxEventsCreateGame.SelectedItem != null) &&
                (comboBoxEventsCreateStartTime.SelectedItem != null) &&
                (comboBoxEventsCreateEndTime.SelectedItem != null) &&
                (textBoxEventsCreateEventName.Text != string.Empty) &&
                (textBoxEventsCreateEventDetails.Text != string.Empty))
            {
                GamingEvent gamingEvent = new GamingEvent();
                if (eventsCalendar.SelectedDate != null)
                {
                    DateTime selectedDate = (DateTime) eventsCalendar.SelectedDate;
                    DateTime startTime = DateTime.Parse(comboBoxEventsCreateStartTime.SelectionBoxItem.ToString());
                    selectedDate = selectedDate.AddHours(startTime.Hour);
                    startTime = selectedDate.AddMinutes(startTime.Minute);
                    gamingEvent.StartTime = startTime;

                    DateTime endTime = DateTime.Parse(comboBoxEventsCreateEndTime.SelectionBoxItem.ToString());
                    selectedDate = (DateTime) eventsCalendar.SelectedDate;
                    selectedDate = selectedDate.AddHours(endTime.Hour);
                    endTime = selectedDate.AddMinutes(endTime.Minute);
                    gamingEvent.EndTime = endTime;
                }
                gamingEvent.Description = textBoxEventsCreateEventDetails.Text;
                Event gEvent = new Event();
                gEvent.GameID = ((Game) listBoxEventsCreateGame.SelectedItem).ID;
                gEvent.Game = (Game) listBoxEventsCreateGame.SelectedItem;
                gamingEvent.Event = gEvent;
                gamingEvent.Location = string.Empty;
                gamingEvent.Name = textBoxEventsCreateEventName.Text;
                _gnsClient.CreateEventAsync(UserMe, gamingEvent, null);
            }
        }

        private void ButtonMeAddGameNickClick(object sender, RoutedEventArgs e)
        {
            _childWindow.Closed += ChildWindowAddGameNickOnClosed;
            _childWindow.Show();
        }

        private void ButtonMeRemoveGameNickClick(object sender, RoutedEventArgs e)
        {
            if (dataGridMeGameNicks.SelectedItem != null)
            {
                _gnsClient.RemoveGameNickAsync(UserMe, (GameNick)dataGridMeGameNicks.SelectedItem);
            }
        }
        private void ButtonMeUpdateStatusClick(object sender, RoutedEventArgs e)
        {
            if (textBoxMeStatusUpdate.Text != string.Empty)
            {
                string updateText = textBoxMeStatusUpdate.Text;
                if (dataGridMeGameNicks.SelectedItem != null)
                {
                    GameNick uGameNick = (GameNick) dataGridMeGameNicks.SelectedItem;
                    _gnsClient.UpdateStatusAsync(UserMe, uGameNick.ID, updateText);
                }
            }
        }

        private void ButtonViewFacebookEventClick(object sender, RoutedEventArgs e)
        {
            GamingEvent item = (GamingEvent)dataGridBrowseEvents.SelectedItem;
            HtmlPage.Window.Navigate(new Uri("http://www.facebook.com/" + item.Event.FacebookID.ToString()), "_blank");
        }
        #endregion


    }
}
