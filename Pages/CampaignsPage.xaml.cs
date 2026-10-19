using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cursavayaup1.Pages
{
    public partial class CampaignsPage : Page
    {
        private MainWindow _mainWindow;

        public CampaignsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadCampaigns();
        }

        private void LoadCampaigns(string search = "")
        {
            using (var db = new up1Entities1())
            {
                var query = db.vw_CampaignsFull.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(c => c.Title.Contains(search));
                }

                // Показываем только активные и завершённые
                var campaigns = query
                    .Where(c => c.Status == "Active" || c.Status == "Completed")
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                CampaignsList.ItemsSource = campaigns;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtSearchPlaceholder.Visibility =
                string.IsNullOrEmpty(txtSearch.Text) ? Visibility.Visible : Visibility.Collapsed;

            LoadCampaigns(txtSearch.Text.Trim());
        }

        private void CampaignCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var campaign = border.DataContext as vw_CampaignsFull;

            if (campaign != null)
            {
                _mainWindow.MainFrame.Navigate(new CampaignDetailsPage(_mainWindow, campaign.CampaignId));
            }
        }
    }
}