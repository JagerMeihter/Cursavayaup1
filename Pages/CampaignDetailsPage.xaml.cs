using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cursavayaup1.Pages
{
    public partial class CampaignDetailsPage : Page
    {
        private MainWindow _mainWindow;
        private int _campaignId;
        private vw_CampaignsFull _campaign;

        public CampaignDetailsPage(MainWindow mainWindow, int campaignId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _campaignId = campaignId;
            LoadCampaign();
        }

        private void LoadCampaign()
        {
            using (var db = new up1Entities1())
            {
                _campaign = db.vw_CampaignsFull.FirstOrDefault(c => c.CampaignId == _campaignId);

                if (_campaign == null)
                {
                    MessageBox.Show("Кампания не найдена");
                    return;
                }

                txtTitle.Text = _campaign.Title;
                txtCreator.Text = "Автор: " + _campaign.CreatorName;
                txtCategory.Text = _campaign.CategoryName ?? "Без категории";
                txtDescription.Text = _campaign.Description;

                txtCollected.Text = $"{_campaign.CurrentAmount:N0} ₽";
                txtGoal.Text = $"{_campaign.GoalAmount:N0} ₽";
                txtPercent.Text = $"Собрано {_campaign.ProgressPercent}% • {_campaign.DonationsCount} пожертвований";

                progressBar.Value = (double)(_campaign.ProgressPercent ?? 0);

                // Статус
                if (_campaign.Status == "Active")
                {
                    txtStatus.Text = "Активна";
                    txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                }
                else if (_campaign.Status == "Completed")
                {
                    txtStatus.Text = "Завершена";
                    txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                    donationPanel.Visibility = Visibility.Collapsed; // нельзя больше донатить
                }
                else
                {
                    txtStatus.Text = _campaign.Status;
                    txtStatus.Foreground = Brushes.Gray;
                    donationPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BtnDonate_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text.Replace(" ", ""), out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    var donation = new Donations
                    {
                        CampaignId = _campaignId,
                        UserId = _mainWindow.CurrentUser.UserId,
                        Amount = amount,
                        Message = string.IsNullOrWhiteSpace(txtMessage.Text) ? null : txtMessage.Text.Trim(),
                        DonationDate = DateTime.Now
                    };

                    db.Donations.Add(donation);
                    db.SaveChanges(); // триггер сам обновит CurrentAmount

                    MessageBox.Show($"Спасибо! Вы пожертвовали {amount:N0} ₽", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем данные на странице
                    LoadCampaign();
                    txtAmount.Clear();
                    txtMessage.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при пожертвовании:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new CampaignsPage(_mainWindow));
        }
    }
}