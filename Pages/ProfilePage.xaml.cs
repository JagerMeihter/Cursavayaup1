using System;
using System.Windows;
using System.Windows.Controls;

namespace Cursavayaup1.Pages
{
    public partial class ProfilePage : Page
    {
        private MainWindow _mainWindow;

        public ProfilePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            LoadProfile();
        }

        private void LoadProfile()
        {
            var user = _mainWindow.CurrentUser;

            txtFullName.Text = user.FullName;
            txtUsername.Text = user.Username;
            txtEmail.Text = user.Email;
            txtPhone.Text = string.IsNullOrEmpty(user.Phone) ? "—" : user.Phone;
            txtRole.Text = user.Role;

            // Безопасное форматирование даты
            txtCreatedAt.Text = user.CreatedAt != null
                ? Convert.ToDateTime(user.CreatedAt).ToString("dd.MM.yyyy HH:mm")
                : "—";

            if (user.LastLogin != null)
            {
                txtLastLogin.Text = Convert.ToDateTime(user.LastLogin).ToString("dd.MM.yyyy HH:mm");
            }
            else
            {
                txtLastLogin.Text = "—";
            }
        }
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string current = txtCurrentPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirm = txtNewPasswordConfirm.Password;

            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("Новые пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 4)
            {
                MessageBox.Show("Новый пароль должен быть не короче 4 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    var user = db.Users.Find(_mainWindow.CurrentUser.UserId);
                    if (user == null) return;

                    // Проверяем текущий пароль
                    bool correct = Helpers.PasswordHelper.VerifyPassword(current, user.PasswordHash);
                    if (!correct && user.PasswordHash != current) // поддержка старых паролей
                    {
                        MessageBox.Show("Неверный текущий пароль", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Меняем
                    user.PasswordHash = Helpers.PasswordHelper.HashPassword(newPass);
                    db.SaveChanges();

                    // Обновляем текущего пользователя в памяти
                    _mainWindow.CurrentUser.PasswordHash = user.PasswordHash;

                    MessageBox.Show("Пароль успешно изменён!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    txtCurrentPassword.Clear();
                    txtNewPassword.Clear();
                    txtNewPasswordConfirm.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}