using Microsoft.AspNetCore.SignalR;

namespace Cryptofolio.App.Hubs
{
    /// <summary>
    /// Provides a hub for the connected clients on the dashboard.
    /// </summary>
    public class DashboardHub : Hub<IDashboardClient>
    {
        public const string Endpoint = "/dashboardHub";
    }
}
