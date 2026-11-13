using MySqlConnector;

namespace CyberSecurityAwarenessChatbot;

public sealed class TaskRepository
{
    private readonly string _connectionString;
    public TaskRepository(string connectionString) => _connectionString = connectionString;

    private async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<int> AddAsync(TaskItem task)
    {
        await using var db = await OpenAsync();
        await using var cmd = new MySqlCommand(@"INSERT INTO tasks(title, description, reminder_at, is_completed)
VALUES(@title,@description,@reminder,@completed); SELECT LAST_INSERT_ID();", db);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@description", task.Description);
        cmd.Parameters.AddWithValue("@reminder", (object?)task.ReminderAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@completed", task.IsCompleted);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<TaskItem>> GetAllAsync()
    {
        var tasks = new List<TaskItem>();
        await using var db = await OpenAsync();
        await using var cmd = new MySqlCommand("SELECT id,title,description,reminder_at,is_completed FROM tasks ORDER BY is_completed, reminder_at, id", db);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tasks.Add(new TaskItem { Id=reader.GetInt32(0), Title=reader.GetString(1), Description=reader.GetString(2),
                ReminderAt=reader.IsDBNull(3) ? null : reader.GetDateTime(3), IsCompleted=reader.GetBoolean(4) });
        return tasks;
    }

    public async Task SetReminderAsync(int id, DateTime? reminderAt)
    {
        await using var db = await OpenAsync();
        await using var cmd = new MySqlCommand("UPDATE tasks SET reminder_at=@reminder WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@reminder", (object?)reminderAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await OpenAsync();
        await using var cmd = new MySqlCommand("DELETE FROM tasks WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SetCompletedAsync(int id, bool completed)
    {
        await using var db = await OpenAsync();
        await using var cmd = new MySqlCommand("UPDATE tasks SET is_completed=@done WHERE id=@id", db);
        cmd.Parameters.AddWithValue("@done", completed);
        cmd.Parameters.AddWithValue("@id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}