using Microsoft.Data.Sqlite;
// TODO: This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
// TODO: Users need to be able to input the date of the occurrence of the habit
// TODO: The application should store and retrieve data from a real database
// TODO: When the application starts, it should create a sqlite database, if one isn’t present.
// TODO: It should also create a table in the database, where the habit will be logged.
// TODO: The users should be able to insert, delete, update and view their logged habit.
// TODO: You should handle all possible errors so that the application never crashes.
// TODO: You can only interact with the database using ADO.NET.
// TODO: Your project needs to contain a Read Me file where you'll explain how your app works and tell a little bit about your thought progress.

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=habits.db";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string tableCommand = @"CREATE TABLE IF NOT EXISTS Habits(
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Date TEXT NOT NULL,
                                    Quantity INTEGER NOT NULL
                                );";
        using var createTable = new SqliteCommand(tableCommand, connection);
        createTable.ExecuteNonQuery();
    }
}