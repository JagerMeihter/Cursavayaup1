using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cursavayaup1.Pages
{
    public partial class MyCampaignsPage : Page
    {
        private MainWindow _mainWindow;

        public MyCampaignsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadMyCampaigns();
        }

        private void LoadMyCampaigns()
        {
            using (var db = new up1Entities1())
            {
                var campaigns = db.Campaigns
                    .Where(c => c.CreatorId == _mainWindow.CurrentUser.UserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                CampaignsGrid.ItemsSource = campaigns;
            }
        }

        private void CampaignsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var campaign = CampaignsGrid.SelectedItem as Campaigns;
            if (campaign != null)
            {
                _mainWindow.MainFrame.Navigate(new CampaignDetailsPage(_mainWindow, campaign.CampaignId));
            }
        }
    }
}