using System;
using System.Linq;
using System.Windows;

namespace Cursavayaup1
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private int _failedAttempts = 0;
        private const int MaxAttempts = 3;

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если слишком много попыток — показываем капчу
            if (_failedAttempts >= MaxAttempts)
            {
                // Простая капча
                string captcha = GenerateCaptcha();
                string userAnswer = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Введите код с картинки:\n\n{captcha}", "Проверка", "");

                if (userAnswer != captcha)
                {
                    MessageBox.Show("Неверная капча. Попробуйте ещё раз.", "Ошибка");
                    return;
                }
            }

            try
            {
                using (var db = new up1Entities1())
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == username);

                    if (user == null || !(user.IsActive ?? true))
                    {
                        _failedAttempts++;
                        MessageBox.Show("Пользователь не найден или заблокирован", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Проверяем хэш
                    bool passwordCorrect = Helpers.PasswordHelper.VerifyPassword(password, user.PasswordHash);

                    // Для старых пользователей (у кого пароль ещё не захэширован) — временная поддержка
                    if (!passwordCorrect && user.PasswordHash == password)
                    {
                        // Обновляем старый пароль на хэш
                        user.PasswordHash = Helpers.PasswordHelper.HashPassword(password);
                        passwordCorrect = true;
                    }

                    if (!passwordCorrect)
                    {
                        _failedAttempts++;

                        // Записываем неудачную попытку
                        db.LoginHistory.Add(new LoginHistory
                        {
                            UserId = user.UserId,
                            LoginDate = DateTime.Now,
                            Success = false
                        });
                        db.SaveChanges();

                        MessageBox.Show($"Неверный пароль. Осталось попыток: {MaxAttempts - _failedAttempts}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Успешный вход
                    _failedAttempts = 0;
                    user.LastLogin = DateTime.Now;

                    db.LoginHistory.Add(new LoginHistory
                    {
                        UserId = user.UserId,
                        LoginDate = DateTime.Now,
                        Success = true
                    });

                    db.SaveChanges();

                    MainWindow main = new MainWindow(user);
                    main.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при входе:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Простая текстовая капча
        private string GenerateCaptcha()
        {
            Random rnd = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] captcha = new char[5];
            for (int i = 0; i < 5; i++)
                captcha[i] = chars[rnd.Next(chars.Length)];
            return new string(captcha);
        }

        // ← Этот метод обязательно должен быть!
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.Show();
            this.Close();
        }
    }
}