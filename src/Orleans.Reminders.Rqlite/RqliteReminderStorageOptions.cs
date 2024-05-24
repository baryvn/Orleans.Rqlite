namespace Orleans.Reminders.Rqlite
{
    public class RqliteReminderStorageOptions
    {
        public required string Uri { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}