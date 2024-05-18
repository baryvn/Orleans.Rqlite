namespace Orleans.Clustering.Rqlite
{
    public class RqliteClusteringOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 3000;
        public string Namespace { get; set; } = "dev";
        public string SetName { get; set; } = "membershipTable";
        public string Username { get; set; }
        public string Password { get; set; }
        public bool CleanupOnInit { get; set; }
        public bool VerifyEtagGenerations { get; set; } = true;
    }
}
