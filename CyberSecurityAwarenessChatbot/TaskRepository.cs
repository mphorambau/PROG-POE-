using Microsoft.Data.Sqlite;

namespace CyberSecurityAwarenessChatbot;

/// <summary>
/// Stores the task assistant data in a local SQLite file. No server, password,
/// or environment-variable setup is required.
/// </summary>
public sealed class TaskRepository
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public TaskRepository()
    {
        _databasePath = Path.Combine(AppContext.BaseDirectory, "Data", "cyber_awareness.db");
        _connectionString = new SqliteConnectionStringBuilder { DataSource = _databasePath, ForeignKeys = true }.ToString();
    }

    public string DatabasePath => _databasePath;

    private async Task<SqliteConnection> OpenAsync()
    {
        await EnsureInitializedAsync();
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;
        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized) return;
            Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
            await using var db = new SqliteConnection(_connectionString);
            await db.OpenAsync();
            await using var command = db.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS tasks (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    title TEXT NOT NULL CHECK(length(title) <= 160),
    description TEXT NOT NULL DEFAULT '',
    reminder_at TEXT NULL,
    is_completed INTEGER NOT NULL DEFAULT 0 CHECK(is_completed IN (0, 1)),
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);";
            await command.ExecuteNonQueryAsync();
            _initialized = true;
        }
        finally { _initializationLock.Release(); }
    }

    public async Task<int> AddAsync(TaskItem task)
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = @"INSERT INTO tasks(title, description, reminder_at, is_completed)
VALUES($title, $description, $reminder, $completed);
SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$title", task.Title);
        cmd.Parameters.AddWithValue("$description", task.Description);
        cmd.Parameters.AddWithValue("$reminder", ToDatabaseValue(task.ReminderAt));
        cmd.Parameters.AddWithValue("$completed", task.IsCompleted ? 1 : 0);
        return Convert.ToInt32((long)(await cmd.ExecuteScalarAsync() ?? 0L));
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = @"SELECT id, title, description, reminder_at, is_completed FROM tasks
ORDER BY is_completed, CASE WHEN reminder_at IS NULL THEN 1 ELSE 0 END, reminder_at, id;";
        return await ReadTasksAsync(cmd);
    }

    public async Task<List<TaskItem>> GetDueRemindersAsync(DateTime now)
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = @"SELECT id, title, description, reminder_at, is_completed FROM tasks
WHERE is_completed = 0 AND reminder_at IS NOT NULL AND reminder_at <= $now
ORDER BY reminder_at, id;";
        cmd.Parameters.AddWithValue("$now", now.ToString("O"));
        return await ReadTasksAsync(cmd);
    }

    public async Task SetReminderAsync(int id, DateTime? reminderAt)
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE tasks SET reminder_at = $reminder WHERE id = $id;";
        cmd.Parameters.AddWithValue("$reminder", ToDatabaseValue(reminderAt));
        cmd.Parameters.AddWithValue("$id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = "DELETE FROM tasks WHERE id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SetCompletedAsync(int id, bool completed)
    {
        await using var db = await OpenAsync();
        await using var cmd = db.CreateCommand();
        cmd.CommandText = "UPDATE tasks SET is_completed = $completed WHERE id = $id;";
        cmd.Parameters.AddWithValue("$completed", completed ? 1 : 0);
        cmd.Parameters.AddWithValue("$id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static object ToDatabaseValue(DateTime? dateTime) => (object?)dateTime?.ToString("O") ?? DBNull.Value;

    private static async Task<List<TaskItem>> ReadTasksAsync(SqliteCommand cmd)
    {
        var tasks = new List<TaskItem>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tasks.Add(new TaskItem
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.GetString(2),
                ReminderAt = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind),
                IsCompleted = reader.GetInt64(4) != 0
            });
        return tasks;
    }
}
