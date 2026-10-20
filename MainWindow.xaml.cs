using System.Windows;
using System.Windows.Controls;
using Cursavayaup1.Pages;

namespace Cursavayaup1
{
    public partial class MainWindow : Window
    {
        public Users CurrentUser { get; private set; }

        public MainWindow(Users user)
        {
            InitializeComponent();
            CurrentUser = user;

            txtUserName.Text = user.FullName;
            txtUserRole.Text = user.Role;

            // Показываем кнопку админа только администратору
            if (user.Role == "Admin")
            {
                btnAdmin.Visibility = Visibility.Visible;
            }

            // При открытии сразу показываем список кампаний
            MainFrame.Navigate(new CampaignsPage(this));
        }

        private void BtnCampaigns_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CampaignsPage(this));
        }

        private void BtnCreateCampaign_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CreateCampaignPage(this));
        }

        private void BtnMyDonations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MyDonationsPage(this));
        }

        private void BtnMyCampaigns_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MyCampaignsPage(this));
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage(this));
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage(this));
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            Helpers.ThemeManager.ToggleTheme();
            btnTheme.Content = Helpers.ThemeManager.IsDark ? "Светлая тема" : "Тёмная тема";
        }
        
    }
}