using Microsoft.Data.Sqlite;
// TODO: This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
// TODO: Users need to be able to input the date of the occurrence of the habit
// TODO: The application should store and retrieve data from a real database
// When the application starts, it should create a sqlite database, if one isn’t present.
// TODO: It should also create a table in the database, where the habit will be logged.
// TODO: The users should be able to insert, delete, update and view their logged habit.
// TODO: You should handle all possible errors so that the application never crashes.
// TODO: You can only interact with the database using ADO.NET.
// TODO: Your project needs to contain a Read Me file where you'll explain how your app works and tell a little bit about your thought progress.
// ? https://www.thecsharpacademy.com/project/12/habit-logger
// ? https://reintech.io/blog/mastering-parameterized-queries-ado-net

class Program
{
    static void Main()
    {
        string connectionString = "Data Source=habit_tracker.db";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string habitsTableCommand = @"CREATE TABLE IF NOT EXISTS habits(
                                    habit_id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    habit_name TEXT NOT NULL CHECK(length(habit_name) <= 100)
                                );";

        string unitsTableCommand = @"CREATE TABLE IF NOT EXISTS units(
                                    unit_id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    unit_name TEXT NOT NULL CHECK(length(unit_name) <= 30)
                                );";

        string habitLogsTableCommand = @"CREATE TABLE IF NOT EXISTS habit_logs(
                                    log_id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    date TEXT NOT NULL CHECK(date(date) IS NOT NULL),
                                    habit_id INTEGER NOT NULL,
                                    quantity INTEGER NOT NULL,
                                    unit_id INTEGER NOT NULL,
                                    FOREIGN KEY (habit_id) REFERENCES habits(habit_id),
                                    FOREIGN KEY (unit_id) REFERENCES units(unit_id)                                    
                                );";

        using var createTable = new SqliteCommand(habitsTableCommand + unitsTableCommand + habitLogsTableCommand, connection);
        createTable.ExecuteNonQuery();
    }
}