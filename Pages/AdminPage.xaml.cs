using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace Cursavayaup1.Pages
{
    public partial class AdminPage : Page
    {
        private MainWindow _mainWindow;

        public AdminPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            LoadUsers();
            LoadCampaigns();
            LoadDonations();
            InitFilters();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            using (var db = new up1Entities1())
            {
                // Карточки
                txtStatUsers.Text = db.Users.Count().ToString();
                txtStatCampaigns.Text = db.Campaigns.Count().ToString();
                txtStatDonations.Text = db.Donations.Count().ToString();

                var totalAmount = db.Donations.Sum(d => (decimal?)d.Amount) ?? 0;
                txtStatTotalAmount.Text = $"{totalAmount:N0} ₽";

                // Топ кампаний
                var top = (from c in db.Campaigns
                           select new
                           {
                               c.Title,
                               Total = c.CurrentAmount,
                               Goal = c.GoalAmount,
                               Progress = c.GoalAmount > 0
                                   ? Math.Round((double)(c.CurrentAmount / c.GoalAmount) * 100, 1)
                                   : 0
                           })
                           .OrderByDescending(x => x.Total)
                           .Take(10)
                           .ToList();

                TopCampaignsGrid.ItemsSource = top;
            }
        }

        // ==================== ИНИЦИАЛИЗАЦИЯ ФИЛЬТРОВ ====================
        private void InitFilters()
        {
            cmbUserRoleFilter.Items.Add("Все роли");
            cmbUserRoleFilter.Items.Add("Admin");
            cmbUserRoleFilter.Items.Add("User");
            cmbUserRoleFilter.Items.Add("Moderator");
            cmbUserRoleFilter.SelectedIndex = 0;

            cmbCampaignStatusFilter.Items.Add("Все статусы");
            cmbCampaignStatusFilter.Items.Add("Active");
            cmbCampaignStatusFilter.Items.Add("Completed");
            cmbCampaignStatusFilter.Items.Add("Cancelled");
            cmbCampaignStatusFilter.Items.Add("Draft");
            cmbCampaignStatusFilter.SelectedIndex = 0;
        }

        // ==================== ПОЛЬЗОВАТЕЛИ ====================
        private void LoadUsers(string search = "", string roleFilter = "Все роли")
        {
            using (var db = new up1Entities1())
            {
                var query = db.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.Username.Contains(search) ||
                        u.FullName.Contains(search) ||
                        u.Email.Contains(search));
                }

                if (roleFilter != "Все роли")
                {
                    query = query.Where(u => u.Role == roleFilter);
                }

                UsersGrid.ItemsSource = query.OrderBy(u => u.UserId).ToList();
            }
        }

        private void TxtUserSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtUserSearchPlaceholder.Visibility =
                string.IsNullOrEmpty(txtUserSearch.Text) ? Visibility.Visible : Visibility.Collapsed;

            string role = cmbUserRoleFilter.SelectedItem?.ToString() ?? "Все роли";
            LoadUsers(txtUserSearch.Text.Trim(), role);
        }

        private void CmbUserRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUserRoleFilter.SelectedItem == null) return;

            string role = cmbUserRoleFilter.SelectedItem.ToString();
            LoadUsers(txtUserSearch.Text.Trim(), role);
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void BtnToggleUserActive_Click(object sender, RoutedEventArgs e)
        {
            var user = UsersGrid.SelectedItem as Users;
            if (user == null)
            {
                MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (user.UserId == _mainWindow.CurrentUser.UserId)
            {
                MessageBox.Show("Нельзя заблокировать самого себя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    var dbUser = db.Users.Find(user.UserId);
                    if (dbUser != null)
                    {
                        bool current = dbUser.IsActive ?? true;
                        dbUser.IsActive = !current;
                        db.SaveChanges();

                        MessageBox.Show((dbUser.IsActive ?? false) ? "Пользователь разблокирован" : "Пользователь заблокирован", "Успех");
                        LoadUsers(txtUserSearch.Text.Trim(), cmbUserRoleFilter.SelectedItem?.ToString() ?? "Все роли");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = UsersGrid.SelectedItem as Users;
            if (user == null)
            {
                MessageBox.Show("Выберите пользователя", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (user.UserId == _mainWindow.CurrentUser.UserId)
            {
                MessageBox.Show("Нельзя удалить самого себя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить пользователя «{user.FullName}»?\nЭто действие нельзя отменить.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new up1Entities1())
                {
                    foreach (var d in db.Donations.Where(x => x.UserId == user.UserId).ToList())
                        db.Donations.Remove(d);

                    foreach (var f in db.Favorites.Where(x => x.UserId == user.UserId).ToList())
                        db.Favorites.Remove(f);

                    foreach (var c in db.Comments.Where(x => x.UserId == user.UserId).ToList())
                        db.Comments.Remove(c);

                    foreach (var n in db.Notifications.Where(x => x.UserId == user.UserId).ToList())
                        db.Notifications.Remove(n);

                    foreach (var r in db.Reports.Where(x => x.ReporterId == user.UserId).ToList())
                        db.Reports.Remove(r);

                    foreach (var l in db.LoginHistory.Where(x => x.UserId == user.UserId).ToList())
                        db.LoginHistory.Remove(l);

                    var campaigns = db.Campaigns.Where(c => c.CreatorId == user.UserId).ToList();
                    foreach (var camp in campaigns)
                    {
                        foreach (var d in db.Donations.Where(x => x.CampaignId == camp.CampaignId).ToList())
                            db.Donations.Remove(d);

                        foreach (var c in db.Comments.Where(x => x.CampaignId == camp.CampaignId).ToList())
                            db.Comments.Remove(c);

                        foreach (var f in db.Favorites.Where(x => x.CampaignId == camp.CampaignId).ToList())
                            db.Favorites.Remove(f);

                        foreach (var u in db.CampaignUpdates.Where(x => x.CampaignId == camp.CampaignId).ToList())
                            db.CampaignUpdates.Remove(u);

                        foreach (var r in db.Reports.Where(x => x.CampaignId == camp.CampaignId).ToList())
                            db.Reports.Remove(r);

                        db.Campaigns.Remove(camp);
                    }

                    var dbUser = db.Users.Find(user.UserId);
                    if (dbUser != null)
                    {
                        db.Users.Remove(dbUser);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Пользователь удалён", "Успех");
                    LoadUsers(txtUserSearch.Text.Trim(), cmbUserRoleFilter.SelectedItem?.ToString() ?? "Все роли");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении:\n" + ex.Message);
            }
        }

        // ==================== КАМПАНИИ ====================
        private void LoadCampaigns(string search = "", string statusFilter = "Все статусы")
        {
            using (var db = new up1Entities1())
            {
                var query = db.Campaigns.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(c => c.Title.Contains(search));

                if (statusFilter != "Все статусы")
                    query = query.Where(c => c.Status == statusFilter);

                CampaignsGrid.ItemsSource = query.OrderByDescending(c => c.CreatedAt).ToList();
            }
        }

        private void TxtCampaignSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string status = cmbCampaignStatusFilter.SelectedItem?.ToString() ?? "Все статусы";
            LoadCampaigns(txtCampaignSearch.Text.Trim(), status);
        }

        private void CmbCampaignStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCampaignStatusFilter.SelectedItem == null) return;
            string status = cmbCampaignStatusFilter.SelectedItem.ToString();
            LoadCampaigns(txtCampaignSearch.Text.Trim(), status);
        }

        private void BtnDeleteCampaign_Click(object sender, RoutedEventArgs e)
        {
            var campaign = CampaignsGrid.SelectedItem as Campaigns;
            if (campaign == null)
            {
                MessageBox.Show("Выберите кампанию", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить кампанию «{campaign.Title}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new up1Entities1())
                {
                    foreach (var d in db.Donations.Where(x => x.CampaignId == campaign.CampaignId).ToList())
                        db.Donations.Remove(d);

                    foreach (var c in db.Comments.Where(x => x.CampaignId == campaign.CampaignId).ToList())
                        db.Comments.Remove(c);

                    foreach (var f in db.Favorites.Where(x => x.CampaignId == campaign.CampaignId).ToList())
                        db.Favorites.Remove(f);

                    foreach (var u in db.CampaignUpdates.Where(x => x.CampaignId == campaign.CampaignId).ToList())
                        db.CampaignUpdates.Remove(u);

                    foreach (var r in db.Reports.Where(x => x.CampaignId == campaign.CampaignId).ToList())
                        db.Reports.Remove(r);

                    var dbCampaign = db.Campaigns.Find(campaign.CampaignId);
                    if (dbCampaign != null)
                    {
                        db.Campaigns.Remove(dbCampaign);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Кампания удалена", "Успех");
                    LoadCampaigns(txtCampaignSearch.Text.Trim(), cmbCampaignStatusFilter.SelectedItem?.ToString() ?? "Все статусы");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // ==================== ПОЖЕРТВОВАНИЯ ====================
        private void LoadDonations()
        {
            using (var db = new up1Entities1())
            {
                DonationsGrid.ItemsSource = db.Donations.OrderByDescending(d => d.DonationDate).ToList();
            }
        }

        // ==================== ЭКСПОРТ В EXCEL ====================
        private void BtnExportUsersExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new up1Entities1())
                {
                    var users = db.Users.OrderBy(u => u.UserId).ToList();

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Пользователи");

                        ws.Cell(1, 1).Value = "ID";
                        ws.Cell(1, 2).Value = "Логин";
                        ws.Cell(1, 3).Value = "ФИО";
                        ws.Cell(1, 4).Value = "Email";
                        ws.Cell(1, 5).Value = "Телефон";
                        ws.Cell(1, 6).Value = "Роль";
                        ws.Cell(1, 7).Value = "Активен";
                        ws.Cell(1, 8).Value = "Дата регистрации";

                        int row = 2;
                        foreach (var u in users)
                        {
                            ws.Cell(row, 1).Value = u.UserId;
                            ws.Cell(row, 2).Value = u.Username;
                            ws.Cell(row, 3).Value = u.FullName;
                            ws.Cell(row, 4).Value = u.Email;
                            ws.Cell(row, 5).Value = u.Phone ?? "";
                            ws.Cell(row, 6).Value = u.Role;
                            ws.Cell(row, 7).Value = (u.IsActive ?? false) ? "Да" : "Нет";
                            ws.Cell(row, 8).Value = u.CreatedAt;
                            row++;
                        }

                        ws.Columns().AdjustToContents();

                        var dialog = new SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx",
                            FileName = $"Users_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            workbook.SaveAs(dialog.FileName);
                            MessageBox.Show("Файл успешно сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message);
            }
        }

        private void BtnExportCampaignsExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new up1Entities1())
                {
                    var campaigns = db.Campaigns.OrderByDescending(c => c.CreatedAt).ToList();

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Кампании");

                        ws.Cell(1, 1).Value = "ID";
                        ws.Cell(1, 2).Value = "Название";
                        ws.Cell(1, 3).Value = "Цель";
                        ws.Cell(1, 4).Value = "Собрано";
                        ws.Cell(1, 5).Value = "Статус";
                        ws.Cell(1, 6).Value = "Автор (ID)";
                        ws.Cell(1, 7).Value = "Дата создания";
                        ws.Cell(1, 8).Value = "Дата окончания";

                        int row = 2;
                        foreach (var c in campaigns)
                        {
                            ws.Cell(row, 1).Value = c.CampaignId;
                            ws.Cell(row, 2).Value = c.Title;
                            ws.Cell(row, 3).Value = c.GoalAmount;
                            ws.Cell(row, 4).Value = c.CurrentAmount;
                            ws.Cell(row, 5).Value = c.Status;
                            ws.Cell(row, 6).Value = c.CreatorId;
                            ws.Cell(row, 7).Value = c.CreatedAt;
                            ws.Cell(row, 8).Value = c.EndDate;
                            row++;
                        }

                        ws.Columns().AdjustToContents();

                        var dialog = new SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx",
                            FileName = $"Campaigns_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            workbook.SaveAs(dialog.FileName);
                            MessageBox.Show("Файл успешно сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message);
            }
        }

        private void BtnExportDonationsExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new up1Entities1())
                {
                    var donations = db.Donations.OrderByDescending(d => d.DonationDate).ToList();

                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Пожертвования");

                        ws.Cell(1, 1).Value = "ID";
                        ws.Cell(1, 2).Value = "Кампания (ID)";
                        ws.Cell(1, 3).Value = "Пользователь (ID)";
                        ws.Cell(1, 4).Value = "Сумма";
                        ws.Cell(1, 5).Value = "Сообщение";
                        ws.Cell(1, 6).Value = "Дата";

                        int row = 2;
                        foreach (var d in donations)
                        {
                            ws.Cell(row, 1).Value = d.DonationId;
                            ws.Cell(row, 2).Value = d.CampaignId;
                            ws.Cell(row, 3).Value = d.UserId;
                            ws.Cell(row, 4).Value = d.Amount;
                            ws.Cell(row, 5).Value = d.Message ?? "";
                            ws.Cell(row, 6).Value = d.DonationDate;
                            row++;
                        }

                        ws.Columns().AdjustToContents();

                        var dialog = new SaveFileDialog
                        {
                            Filter = "Excel files (*.xlsx)|*.xlsx",
                            FileName = $"Donations_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
                        };

                        if (dialog.ShowDialog() == true)
                        {
                            workbook.SaveAs(dialog.FileName);
                            MessageBox.Show("Файл успешно сохранён!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message);
            }

        }
    }
}