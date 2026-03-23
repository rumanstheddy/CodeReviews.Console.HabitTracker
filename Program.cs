using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
// TODO: This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
// TODO: Users need to be able to input the date of the occurrence of the habit
// TODO: The application should store and retrieve data from a real database
// When the application starts, it should create a sqlite database, if one isn’t present.
// It should also create a table in the database, where the habit will be logged.
// TODO: The users should be able to [insert], delete, update and view their logged habit.
// TODO: You should handle all possible errors so that the application never crashes.
// TODO: You can only interact with the database using ADO.NET.
// TODO: Your project needs to contain a Read Me file where you'll explain how your app works and tell a little bit about your thought progress.
// ? https://www.thecsharpacademy.com/project/12/habit-logger
// ? https://reintech.io/blog/mastering-parameterized-queries-ado-net


// TODO: read about repository pattern: https://medium.com/@chandrashekharsingh25/understanding-the-repository-pattern-in-c-net-with-examples-51f02c4074ba

//TODO: work on better variable naming

class Program
{
    static void Main()
    {
        // TODO: make connection string safer - store it in env or json file
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

        int logId = -1;
        int habitId = -1;
        string selectedHabit = "";
        int unitId = -1;
        string selectedUnit = "";
        string? dateLogged = "";
        double quantity = 0.0;

        Console.WriteLine("Choose an action:");
        Console.WriteLine("\tc - Create habit log");
        Console.WriteLine("\tv - View logged habits");
        Console.WriteLine("\tu - Update logged habit");
        Console.WriteLine("\td - Delete logged habit");

        Console.Write("Your option? ");
        string? op = Console.ReadLine();

        // Validate input is not null, and matches the pattern
        while (op == null || !Regex.IsMatch(op, "[c|v|u|d]"))
        {
            Console.WriteLine("Error: Unrecognized input. Please try again.");
            op = Console.ReadLine();
        }

        if (op == "c")
        {
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

                    Console.Write($"Enter the quantity of {selectedUnit}:");
                    string? quantityInput = Console.ReadLine();
                    bool isQuantityDouble = double.TryParse(quantityInput, out quantity);
                    while (!isQuantityDouble || quantity < 0)
                    {
                        if (!isQuantityDouble)
                        {
                            Console.WriteLine("This is not valid input. Please enter a numeric value: ");
                        }
                        else if (quantity < 0)
                        {
                            Console.WriteLine("Error: Quantity should be a positive value.");
                        }
                        quantityInput = Console.ReadLine();
                    }
                }
            }

            // Console.Write("Enter the quantity: ");
            // if (op == "r" && cleanNum1 < 0)
            // {
            //     Console.WriteLine("Error: Cannot calculate square root of a negative number. Please enter a positive number: ");
            //     numInput1 = Console.ReadLine();

            //     while (!double.TryParse(numInput1, out cleanNum1) || cleanNum1 < 0)
            //     {
            //         if (!double.TryParse(numInput1, out cleanNum1))
            //         {
            //             Console.WriteLine("This is not valid input. Please enter a numeric value: ");
            //         }
            //         else if (cleanNum1 < 0)
            //         {
            //             Console.WriteLine("Error: Cannot calculate square root of a negative number. Please enter a positive number: ");
            //         }
            //         numInput1 = Console.ReadLine();
            //     }
            // }

            Console.Write("Press 'c' to enter a custom date to log, or press any other key and Enter to proceed with current date: ");
            if (Console.ReadLine() == "c")
            {
                Console.Write("Press enter a custom date in YYYY-MM-DD format:");
                dateLogged = Console.ReadLine();

                // TODO: find out what the warning is about
                while (!Regex.IsMatch(dateLogged, @"^(\d{4})-(\d{2})-(\d{2})$"))
                {
                    Console.Write("This is not valid input. Please enter a date in YYYY-MM-DD format: ");
                    dateLogged = Console.ReadLine();
                }
            }

            else
            {
                dateLogged = DateTime.Today.ToString("yyyy-MM-dd");
            }

            // Console.Write($"You've entered: {dateLogged}");

            string createHabitLogCommand = @"INSERT INTO habit_logs (date, habit_id, quantity, unit_id) VALUES (@DateLogged, @HabitId, @Quantity, @UnitId);";

            using (var command = new SqliteCommand(createHabitLogCommand, connection))
            {
                command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                command.Parameters.Add("@HabitId", SqliteType.Integer).Value = habitId;
                command.Parameters.Add("@Quantity", SqliteType.Real).Value = quantity;
                command.Parameters.Add("@UnitId", SqliteType.Integer).Value = unitId;

                command.ExecuteNonQuery();
            }

            string selectHabitLogsForDate = @"SELECT * FROM habit_logs WHERE date = @DateLogged;";
            using (var command = new SqliteCommand(selectHabitLogsForDate, connection))
            {
                command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"log_id: {reader["log_id"]}");
                    Console.WriteLine($"date: {reader["date"]}");
                    Console.WriteLine($"habit_id: {reader["habit_id"]}");
                    Console.WriteLine($"quantity: {reader["quantity"]}");
                    Console.WriteLine($"unit_id: {reader["unit_id"]}");
                }
            }
        }

        else if (op == "v")
        {
            string selectHabitLogsForDate = @"SELECT * FROM habit_logs;";
            using (var command = new SqliteCommand(selectHabitLogsForDate, connection))
            {
                command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"log_id: {reader["log_id"]} | date: {reader["date"]} | habit_id: {reader["habit_id"]} | quantity: {reader["quantity"]} | unit_id: {reader["unit_id"]}");
                    Console.WriteLine("\n");
                }
            }
        }

        else if (op == "u")
        {
            Console.WriteLine("Choose a habit log to update: ");
            string selectHabitLogs = @"SELECT * FROM habit_logs;";
            using (var command = new SqliteCommand(selectHabitLogs, connection))
            {
                command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["log_id"]} : date: {reader["date"]} | habit_id: {reader["habit_id"]} | quantity: {reader["quantity"]} | unit_id: {reader["unit_id"]}");
                    // Console.WriteLine($"date: {reader["date"]}");
                    // Console.WriteLine($"habit_id: {reader["habit_id"]}");
                    // Console.WriteLine($"quantity: {reader["quantity"]}");
                    // Console.WriteLine($"unit_id: {reader["unit_id"]}");
                }
            }

            string selectLogCountForIdCommand = @"SELECT COUNT(*) FROM habit_logs WHERE log_id = @LogId;";

            string? logIdInput = Console.ReadLine();
            while (string.IsNullOrEmpty(logIdInput) || !int.TryParse(logIdInput, out logId) || logId < 0)
            {
                Console.WriteLine("This is not a valid ID. Please try again: ");
                logIdInput = Console.ReadLine();
            }

            int logCount = 0;
            while (logCount != 1)
            {
                using (var command = new SqliteCommand(selectLogCountForIdCommand, connection))
                {
                    command.Parameters.Add("@LogId", SqliteType.Integer).Value = logId;
                    logCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            // int logId = -1;
            // int habitId = -1;
            // string selectedHabit = "";
            // int unitId = -1;
            // string selectedUnit = "";
            // string? dateLogged = "";
            // double quantity = 0.0;

            string selectLogForIdCommand = @"SELECT * FROM habit_logs WHERE log_id = @LogId;";

            using (var command = new SqliteCommand(selectLogForIdCommand, connection))
            {
                command.Parameters.Add("@LogId", SqliteType.Text).Value = logId;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    habitId = Convert.ToInt32(reader["habit_id"]);
                    unitId = Convert.ToInt32(reader["unit_id"]);
                    dateLogged = reader["date"].ToString();
                    quantity = Convert.ToDouble(reader["quantity"]);
                    // Console.WriteLine($"{reader["log_id"]} : date: {reader["date"]} | habit_id: {reader["habit_id"]} | quantity: {reader["quantity"]} | unit_id: {reader["unit_id"]}");
                    // Console.WriteLine($"date: {reader["date"]}");
                    // Console.WriteLine($"habit_id: {reader["habit_id"]}");
                    // Console.WriteLine($"quantity: {reader["quantity"]}");
                    // Console.WriteLine($"unit_id: {reader["unit_id"]}");
                }
            }

            Console.Write("Press 'c' to change the habit, or press any other key and Enter to continue: ");
            if (Console.ReadLine() == "c")
            {
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
            }

            Console.Write("Press 'c' to change the unit, or press any other key and Enter to continue: ");
            if (Console.ReadLine() == "c")
            {
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

                Console.Write($"Enter the quantity of {selectedUnit}:");
                string? quantityInput = Console.ReadLine();
                bool isQuantityDouble = double.TryParse(quantityInput, out quantity);
                while (!isQuantityDouble || quantity < 0)
                {
                    if (!isQuantityDouble)
                    {
                        Console.WriteLine("This is not valid input. Please enter a numeric value: ");
                    }
                    else if (quantity < 0)
                    {
                        Console.WriteLine("Error: Quantity should be a positive value.");
                    }
                    quantityInput = Console.ReadLine();
                }
            }

            // Console.Write("Enter the quantity: ");
            // if (op == "r" && cleanNum1 < 0)
            // {
            //     Console.WriteLine("Error: Cannot calculate square root of a negative number. Please enter a positive number: ");
            //     numInput1 = Console.ReadLine();

            //     while (!double.TryParse(numInput1, out cleanNum1) || cleanNum1 < 0)
            //     {
            //         if (!double.TryParse(numInput1, out cleanNum1))
            //         {
            //             Console.WriteLine("This is not valid input. Please enter a numeric value: ");
            //         }
            //         else if (cleanNum1 < 0)
            //         {
            //             Console.WriteLine("Error: Cannot calculate square root of a negative number. Please enter a positive number: ");
            //         }
            //         numInput1 = Console.ReadLine();
            //     }
            // }

            Console.Write("Press 'c' to change th logged date, or press any other key and Enter to continue: ");
            if (Console.ReadLine() == "c")
            {
                Console.Write("Press 'c' to enter a custom date to log, or press any other key and Enter to proceed with current date: ");
                if (Console.ReadLine() == "c")
                {
                    Console.Write("Press enter a custom date in YYYY-MM-DD format:");
                    dateLogged = Console.ReadLine();

                    // TODO: find out what the warning is about
                    while (!Regex.IsMatch(dateLogged, @"^(\d{4})-(\d{2})-(\d{2})$"))
                    {
                        Console.Write("This is not valid input. Please enter a date in YYYY-MM-DD format: ");
                        dateLogged = Console.ReadLine();
                    }
                }

                else
                {
                    dateLogged = DateTime.Today.ToString("yyyy-MM-dd");
                }

                // Console.Write($"You've entered: {dateLogged}");

                string createHabitLogCommand = @"INSERT INTO habit_logs (date, habit_id, quantity, unit_id) VALUES (@DateLogged, @HabitId, @Quantity, @UnitId);";

                using (var command = new SqliteCommand(createHabitLogCommand, connection))
                {
                    command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                    command.Parameters.Add("@HabitId", SqliteType.Integer).Value = habitId;
                    command.Parameters.Add("@Quantity", SqliteType.Real).Value = quantity;
                    command.Parameters.Add("@UnitId", SqliteType.Integer).Value = unitId;

                    command.ExecuteNonQuery();
                }

                string selectHabitLogsForDate = @"SELECT * FROM habit_logs WHERE date = @DateLogged;";
                using (var command = new SqliteCommand(selectHabitLogsForDate, connection))
                {
                    command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Console.WriteLine($"log_id: {reader["log_id"]} | date: {reader["date"]} | habit_id: {reader["habit_id"]} | quantity: {reader["quantity"]} | unit_id: {reader["unit_id"]}");
                    }
                }
            }
        }

        else if (op == "d")
        {
            Console.WriteLine("Choose a habit log to delete: ");
            string selectHabitLogs = @"SELECT * FROM habit_logs;";
            using (var command = new SqliteCommand(selectHabitLogs, connection))
            {
                command.Parameters.Add("@DateLogged", SqliteType.Text).Value = dateLogged;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["log_id"]} : date: {reader["date"]} | habit_id: {reader["habit_id"]} | quantity: {reader["quantity"]} | unit_id: {reader["unit_id"]}");
                    // Console.WriteLine($"date: {reader["date"]}");
                    // Console.WriteLine($"habit_id: {reader["habit_id"]}");
                    // Console.WriteLine($"quantity: {reader["quantity"]}");
                    // Console.WriteLine($"unit_id: {reader["unit_id"]}");
                }
            }

            string selectLogCountForIdCommand = @"SELECT COUNT(*) FROM habit_logs WHERE log_id = @LogId;";

            string? logIdInput = Console.ReadLine();
            while (string.IsNullOrEmpty(logIdInput) || !int.TryParse(logIdInput, out logId) || logId < 0)
            {
                Console.WriteLine("This is not a valid ID. Please try again: ");
                logIdInput = Console.ReadLine();
            }

            int logCount = 0;
            while (logCount != 1)
            {
                using (var command = new SqliteCommand(selectLogCountForIdCommand, connection))
                {
                    command.Parameters.Add("@LogId", SqliteType.Integer).Value = logId;
                    logCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            string deleteLogForId = @"DELETE FROM habit_logs WHERE log_id = @LogId;";
            using (var command = new SqliteCommand(deleteLogForId, connection))
            {
                command.Parameters.Add("@LogId", SqliteType.Integer).Value = logId;
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine($"Habit log with ID {logId} has been deleted. Rows affected: {rowsAffected}");
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