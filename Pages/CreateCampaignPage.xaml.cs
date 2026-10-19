using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cursavayaup1.Pages
{
    public partial class CreateCampaignPage : Page
    {
        private MainWindow _mainWindow;

        public CreateCampaignPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadCategories();
        }

        private void LoadCategories()
        {
            using (var db = new up1Entities1())
            {
                cmbCategory.ItemsSource = db.Categories.ToList();
                cmbCategory.DisplayMemberPath = "CategoryName";
                cmbCategory.SelectedValuePath = "CategoryId";
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string shortDesc = txtShortDescription.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Заполните название и описание", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtGoalAmount.Text.Replace(" ", ""), out decimal goal) || goal <= 0)
            {
                MessageBox.Show("Введите корректную целевую сумму", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    var campaign = new Campaigns
                    {
                        Title = title,
                        ShortDescription = string.IsNullOrEmpty(shortDesc) ? null : shortDesc,
                        Description = description,
                        GoalAmount = goal,
                        CurrentAmount = 0,
                        CreatorId = _mainWindow.CurrentUser.UserId,
                        Status = "Active",
                        CreatedAt = DateTime.Now,
                        EndDate = dpEndDate.SelectedDate,
                        CategoryId = cmbCategory.SelectedValue as int?
                    };

                    db.Campaigns.Add(campaign);
                    db.SaveChanges();

                    MessageBox.Show("Кампания успешно создана!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переходим к списку
                    _mainWindow.MainFrame.Navigate(new CampaignsPage(_mainWindow));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при создании:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}