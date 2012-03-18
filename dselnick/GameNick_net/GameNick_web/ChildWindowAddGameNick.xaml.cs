using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GameNick_Web.GameNickService;

namespace GameNick_Web
{
    public partial class ChildWindowAddGameNick : ChildWindow
    {
        private GameNickServiceClient _gameNickServiceClient;
        private User _userMe;

        public ChildWindowAddGameNick(GameNickServiceClient gameNickServiceClient, User me)
        {
            InitializeComponent();
            _gameNickServiceClient = gameNickServiceClient;
            _userMe = me;
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ListBoxPlatformSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(listBoxPlatform.SelectedItem != null)
            {
                listBoxGame.ItemsSource = ((Platform) listBoxPlatform.SelectedItem).Games;
            }
        }
    }
}

