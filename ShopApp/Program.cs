using System;
using Microsoft.Data.Sqlite;

namespace ShopApp
{
    class Program
    {
        static string connectionString = "Data Source=shop.db;";

        static void Main(string[] args)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqliteCommand pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                    {
                        pragmaCmd.ExecuteNonQuery();
                    }

                    bool exit = false;
                    while (!exit)
                    {
                        Console.WriteLine("\n--- ГОЛОВНЕ МЕНЮ ---");
                        Console.WriteLine("1. Додати дані (Категорії та Товари)");
                        Console.WriteLine("2. Змінити ID категорії");
                        Console.WriteLine("3. Видалити категорію");
                        Console.WriteLine("4. Показати всі таблиці");
                        Console.WriteLine("5. Вихід");
                        Console.Write("Оберіть дію (1-5): ");

                        string choice = Console.ReadLine() ?? "";
                        Console.WriteLine("\n");

                        switch (choice)
                        {
                            case "1":
                                InsertData(connection);
                                break;
                            case "2":
                                UpdateCategory(connection);
                                break;
                            case "3":
                                DeleteCategory(connection);
                                break;
                            case "4":
                                SelectData(connection);
                                break;
                            case "5":
                                exit = true;
                                Console.WriteLine("Роботу завершено.");
                                break;
                            default:
                                Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка підключення до БД: {ex.Message}");
                }
            }
        }

        static void InsertData(SqliteConnection connection)
        {
            Console.WriteLine("\n[ЩО ДОДАЄМО?]");
            Console.WriteLine("1. Нову категорію");
            Console.WriteLine("2. Новий товар (до існуючої категорії)");
            Console.Write("Ваш вибір: ");
            string choice = Console.ReadLine() ?? "";

            try
            {
                if (choice == "1")
                {
                    Console.Write("Введіть ID нової категорії: ");
                    int catId = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("Введіть назву категорії: ");
                    string catName = Console.ReadLine() ?? "";
                    Console.Write("Введіть опис: ");
                    string catDesc = Console.ReadLine() ?? "";

                    string sql = $"INSERT INTO Category (ID, Name, Description) VALUES ({catId}, '{catName}', '{catDesc}')";
                    using (SqliteCommand cmd = new SqliteCommand(sql, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Категорію успішно додано!");
                    }
                }
                else if (choice == "2")
                {
                    Console.Write("Введіть ID товару: ");
                    int prodId = int.Parse(Console.ReadLine() ?? "0");
                    Console.Write("Введіть назву товару: ");
                    string prodName = Console.ReadLine() ?? "";
                    Console.Write("Введіть ціну товару: ");
                    string price = Console.ReadLine() ?? "0";
                    Console.Write("Введіть одиниці вимірювання (наприклад, шт, кг): ");
                    string unit = Console.ReadLine() ?? "";
                    Console.Write("Введіть ID категорії для цього товару: ");
                    int prodCatId = int.Parse(Console.ReadLine() ?? "0");

                    string sql = $"INSERT INTO Product (ID, Name, Price, Unit, Category_ID) VALUES ({prodId}, '{prodName}', {price.Replace(",", ".")}, '{unit}', {prodCatId})";
                    using (SqliteCommand cmd = new SqliteCommand(sql, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Товар успішно додано!");
                    }
                }
                else
                {
                    Console.WriteLine("Невірний вибір.");
                }
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"\n[ПОМИЛКА БД]: Порушення цілісності! {ex.Message}");
                Console.WriteLine("Ймовірно, ви намагаєтесь додати товар до неіснуючої категорії або такий ID вже є.");
            }
            catch (FormatException)
            {
                Console.WriteLine("\n[ПОМИЛКА ВВОДУ]: Будь ласка, вводьте коректні числа для ID.");
            }
        }

        static void UpdateCategory(SqliteConnection connection)
        {
            try
            {
                Console.Write("Введіть старий ID категорії, який хочете змінити: ");
                int oldId = int.Parse(Console.ReadLine() ?? "0");
                Console.Write("Введіть новий ID для цієї категорії: ");
                int newId = int.Parse(Console.ReadLine() ?? "0");

                string updateSql = $"UPDATE Category SET ID = {newId} WHERE ID = {oldId}";
                using (SqliteCommand cmd = new SqliteCommand(updateSql, connection))
                {
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        Console.WriteLine("Категорію оновлено! Перевірте товари (пункт 4).");
                    else
                        Console.WriteLine("Категорію з таким ID не знайдено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static void DeleteCategory(SqliteConnection connection)
        {
            try
            {
                Console.Write("Введіть ID категорії, яку потрібно видалити: ");
                int delId = int.Parse(Console.ReadLine() ?? "0");

                string deleteSql = $"DELETE FROM Category WHERE ID = {delId}";
                using (SqliteCommand cmd = new SqliteCommand(deleteSql, connection))
                {
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        Console.WriteLine("Категорію видалено! Перевірте товари (пункт 4).");
                    else
                        Console.WriteLine("Категорію з таким ID не знайдено.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }

        static void SelectData(SqliteConnection connection)
        {
            Console.WriteLine("\nКатегорії");
            string selectCatSql = "SELECT ID, Name, Description FROM Category";
            using (SqliteCommand cmd = new SqliteCommand(selectCatSql, connection))
            using (SqliteDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine(new string('-', 55));
                Console.WriteLine(String.Format("{0,-5} | {1,-15} | {2,-25}", "ID", "Назва", "Опис"));
                Console.WriteLine(new string('-', 55));

                bool hasCatData = false;
                while (reader.Read())
                {
                    hasCatData = true;
                    Console.WriteLine(String.Format("{0,-5} | {1,-15} | {2,-25}", 
                        reader["ID"], reader["Name"], reader["Description"]));
                }

                if (!hasCatData) Console.WriteLine("Таблиця категорій порожня.");
                Console.WriteLine(new string('-', 55));
            }

            Console.WriteLine("\nТовари");
            string selectProdSql = "SELECT ID, Name, Price, Unit, Category_ID FROM Product";
                
            using (SqliteCommand cmd = new SqliteCommand(selectProdSql, connection))
            using (SqliteDataReader reader = cmd.ExecuteReader())
            {
                Console.WriteLine(new string('-', 65));
                Console.WriteLine(String.Format("{0,-5} | {1,-15} | {2,-7} | {3,-10} | {4,-10}", 
                    "ID", "Назва", "Ціна", "Од. вим.", "ID Кат."));
                Console.WriteLine(new string('-', 65));

                bool hasProdData = false;
                while (reader.Read())
                {
                    hasProdData = true;
                    string catId = reader["Category_ID"] != DBNull.Value ? reader["Category_ID"].ToString()! : "NULL";
                    
                    Console.WriteLine(String.Format("{0,-5} | {1,-15} | {2,-7} | {3,-10} | {4,-10}", 
                        reader["ID"], reader["Name"], reader["Price"], reader["Unit"], catId));
                }

                if (!hasProdData) Console.WriteLine("Таблиця товарів порожня.");
                Console.WriteLine(new string('-', 65));
            }
        }
    }
}