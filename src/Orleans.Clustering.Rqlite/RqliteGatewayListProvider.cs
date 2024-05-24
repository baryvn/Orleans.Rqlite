using Orleans.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using RqliteNet;
using Orleans.Clustering.Rqlite;
using System.Text.Json;

namespace Orleans.Runtime.Membership
{
    public class RqliteGatewayListProvider : IGatewayListProvider
    {
        private readonly ILogger logger;
        private readonly string Uri;
        private readonly string ClusterId;
        private readonly TimeSpan _maxStaleness;

        public RqliteGatewayListProvider(
            ILogger<RqliteGatewayListProvider> logger,
            IOptions<RqliteGatewayListProviderOptions> options,
            IOptions<GatewayOptions> gatewayOptions,
            IOptions<ClusterOptions> clusterOptions)
        {
            this.logger = logger;
            ClusterId = clusterOptions.Value.ClusterId;
            Uri = options.Value.Uri;
            _maxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
        }

        /// <summary>
        /// Initializes the Rqlite based gateway provider
        /// </summary>
        public Task InitializeGatewayListProvider() => Task.CompletedTask;

        /// <summary>
        /// Returns the list of gateways (silos) that can be used by a client to connect to Orleans cluster.
        /// The Uri is in the form of: "gwy.tcp://IP:port/Generation". See Utils.ToGatewayUri and Utils.ToSiloAddress for more details about Uri format.
        /// </summary>
        public async Task<IList<Uri>> GetGateways()
        {
            List<Uri> dataRs = new List<Uri>();
            using (var _rqlite = new RqliteNetClient(this.Uri))
            {
                var result = await _rqlite.Query<MemberModel>("SELECT * FROM " + ClusterId + "_Members");
                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        var data = JsonSerializer.Deserialize<MembershipEntry>(item.Data);
                        if (data != null && data.Status == SiloStatus.Active && data.ProxyPort > 0)
                        {
                            data.SiloAddress.Endpoint.Port = data.ProxyPort;
                            dataRs.Add(data.SiloAddress.ToGatewayUri());
                        }
                    }
                }
            }
            return dataRs;
        }

        /// <summary>
        /// Specifies how often this IGatewayListProvider is refreshed, to have a bound on max staleness of its returned information.
        /// </summary>
        public TimeSpan MaxStaleness => _maxStaleness;

        /// <summary>
        /// Specifies whether this IGatewayListProvider ever refreshes its returned information, or always returns the same gw list.
        /// (currently only the static config based StaticGatewayListProvider is not updatable. All others are.)
        /// </summary>
        public bool IsUpdatable => true;
    }
}
//tôi muốn tạo một lớp triển khai interface trên thì cần luu ý điều gì