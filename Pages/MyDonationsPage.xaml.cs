using System.Linq;
using System.Windows.Controls;

namespace Cursavayaup1.Pages
{
    public partial class MyDonationsPage : Page
    {
        private MainWindow _mainWindow;

        public MyDonationsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadDonations();
        }

        private void LoadDonations()
        {
            using (var db = new up1Entities1())
            {
                var donations = (from d in db.Donations
                                 join c in db.Campaigns on d.CampaignId equals c.CampaignId
                                 where d.UserId == _mainWindow.CurrentUser.UserId
                                 orderby d.DonationDate descending
                                 select new
                                 {
                                     d.DonationDate,
                                     CampaignTitle = c.Title,
                                     d.Amount,
                                     d.Message
                                 }).ToList();

                DonationsGrid.ItemsSource = donations;
            }
        }
    }
}