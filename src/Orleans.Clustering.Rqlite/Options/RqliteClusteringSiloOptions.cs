namespace Orleans.Configuration
{
    /// <summary>
    /// Option to configure RqliteMembership
    /// </summary>
    public class RqliteClusteringSiloOptions
    {
        /// <summary>
        /// Connection string for Rqlite storage
        /// </summary>
        public string Uri { get; set; } = "http://localhost:4001";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
