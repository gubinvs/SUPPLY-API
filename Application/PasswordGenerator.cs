using System;
using System.Text;

public class PasswordGenerator
{
    private static readonly string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}|;:,.<>?";

    public static string GeneratePassword(int length)
    {
        Random rand = new Random();
        StringBuilder password = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            // Выбираем случайный символ из допустимых символов
            password.Append(ValidChars[rand.Next(ValidChars.Length)]);
        }

        return password.ToString();
    }

    // Реализация в контроллере
    // public static void Main()
    // {
    //     int passwordLength = 12; // Указываем длину пароля
    //     string password = GeneratePassword(passwordLength);
    //     //Console.WriteLine("Generated Password: " + password);
    // }
}
