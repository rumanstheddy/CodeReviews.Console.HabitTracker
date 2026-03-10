using Microsoft.Data.Sqlite;
// TODO: This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
// TODO: Users need to be able to input the date of the occurrence of the habit
// TODO: The application should store and retrieve data from a real database
// When the application starts, it should create a sqlite database, if one isn’t present.
// It should also create a table in the database, where the habit will be logged.
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
                                    habit_name TEXT NOT NULL UNIQUE CHECK(length(habit_name) <= 100) COLLATE NOCASE
                                );";

        string unitsTableCommand = @"CREATE TABLE IF NOT EXISTS units(
                                    unit_id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    unit_name TEXT NOT NULL UNIQUE CHECK(length(unit_name) <= 30) COLLATE NOCASE
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


        // string insertHabitCommand = @"INSERT INTO habits (habit_name) VALUES ('Drink water');";
        // string insertUnitCommand = @"INSERT INTO units (unit_name) VALUES ('Glasses');";
        // new SqliteCommand(insertHabitCommand + insertUnitCommand, connection).ExecuteNonQuery();

        int habitId = -1;
        string selectedHabit = "";
        int unitId = -1;
        string selectedUnit = "";
        string dateLogged = "";

        Console.WriteLine("Choose a habit to log: ");
        string selectAllHabitsCommand = @"SELECT * FROM habits;";
        using var habitReader = new SqliteCommand(selectAllHabitsCommand, connection).ExecuteReader();
        while (habitReader.Read())
        {
            Console.WriteLine($"{habitReader["habit_id"]} : {habitReader["habit_name"]}");
        }

        string selectHabitForIdCommand = @"SELECT COUNT(*) FROM habits WHERE habit_id = @HabitId;";

        string? habitIdInput = Console.ReadLine();
        while (string.IsNullOrEmpty(habitIdInput) || !int.TryParse(habitIdInput, out habitId) || habitId < 0)
        {
            Console.WriteLine("This is not a valid ID. Please try again: ");
            habitIdInput = Console.ReadLine();
        }

        int habitCount = 0;
        while (habitCount != 1)
        {
            using (var command = new SqliteCommand(selectHabitForIdCommand, connection))
            {
                command.Parameters.Add("@HabitId", SqliteType.Integer).Value = habitId;
                habitCount = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        string selectHabitNameForId = @"SELECT habit_name FROM habits WHERE habit_id = @HabitId";
        using (var command = new SqliteCommand(selectHabitNameForId, connection))
        {
            command.Parameters.Add("@HabitId", SqliteType.Integer).Value = habitId;
            // habitCount = Convert.ToInt32(command.ExecuteScalar());
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {

                Console.WriteLine($"You've chosen: {reader["habit_name"]}");
                selectedHabit = reader["habit_name"].ToString();
            }
            // command.ExecuteNonQuery();
        }


        Console.WriteLine("Choose a unit for the selected habit: ");
        string selectAllUnitsCommand = @"SELECT * FROM units;";
        using var unitReader = new SqliteCommand(selectAllUnitsCommand, connection).ExecuteReader();
        while (unitReader.Read())
        {
            Console.WriteLine($"{unitReader["unit_id"]} : {unitReader["unit_name"]}");
        }

        string selectUnitForIdCommand = @"SELECT COUNT(*) FROM units WHERE unit_id = @UnitId;";

        string? unitIdInput = Console.ReadLine();
        while (string.IsNullOrEmpty(unitIdInput) || !int.TryParse(unitIdInput, out unitId) || unitId < 0)
        {
            Console.WriteLine("This is not a valid ID. Please try again: ");
            unitIdInput = Console.ReadLine();
        }

        int unitCount = 0;
        while (unitCount != 1)
        {
            using (var command = new SqliteCommand(selectUnitForIdCommand, connection))
            {
                command.Parameters.Add("@UnitId", SqliteType.Integer).Value = unitId;
                unitCount = Convert.ToInt32(command.ExecuteScalar());
            }
        }

        string selectUnitNameForId = @"SELECT unit_name FROM units WHERE unit_id = @UnitId";
        using (var command = new SqliteCommand(selectUnitNameForId, connection))
        {
            command.Parameters.Add("@UnitId", SqliteType.Integer).Value = unitId;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"You've chosen: {reader["unit_name"]}");
                selectedUnit = reader["unit_name"].ToString();
            }
        }

        // if (Console.ReadLine() == "p")
        // {
        //     Console.WriteLine("Select which results you would like to use: \n");
        //     for (int i = 0; i < calculator.ResultHistory.Count; i++)
        //     {
        //         Console.WriteLine($"\t{i + 1} - {calculator.ResultHistory[i]}");
        //     }
        //     Console.WriteLine();
        //     Console.Write("Your option? ");
        //     string? selectedResult = Console.ReadLine();
        //     while (selectedResult == null || !int.TryParse(selectedResult, out int idx) || idx < 1 || idx > calculator.ResultHistory.Count)
        //     {
        //         Console.Write("This is not valid input. Please select an appropriate option: ");
        //         selectedResult = Console.ReadLine();
        //     }
        //     cleanNum1 = calculator.ResultHistory[int.Parse(selectedResult) - 1];
        //     usePreviousResult = true;
        // }
    }
}