using System;
using System.Linq;
using System.Windows;

namespace Cursavayaup1
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Password;
            string passwordConfirm = txtPasswordConfirm.Password;

            // Валидация
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все обязательные поля (*)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != passwordConfirm)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен быть не короче 4 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    // Проверка уникальности логина и email
                    if (db.Users.Any(u => u.Username == username))
                    {
                        MessageBox.Show("Такой логин уже занят", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (db.Users.Any(u => u.Email == email))
                    {
                        MessageBox.Show("Такой email уже зарегистрирован", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newUser = new Users
                    {
                        Username = username,
                        PasswordHash = Helpers.PasswordHelper.HashPassword(password),   // ← хэшируем
                        Email = email,
                        FullName = fullName,
                        Phone = string.IsNullOrEmpty(phone) ? null : phone,
                        Role = "User",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    };

                    db.Users.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Возвращаемся на окно входа
                    LoginWindow login = new LoginWindow();
                    login.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}