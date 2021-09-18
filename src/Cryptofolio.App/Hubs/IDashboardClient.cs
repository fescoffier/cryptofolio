using Cryptofolio.Infrastructure.Entities;
using System.Threading.Tasks;

namespace Cryptofolio.App.Hubs
{
    /// <summary>
    /// Provides an abstraction that represent a dashboard client.
    /// </summary>
    public interface IDashboardClient
    {
        /// <summary>
        /// Notifies the client that its wallet balance changed.
        /// </summary>
        /// <param name="wallet">The wallet.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task WalletBalanceChanged(Wallet wallet);
    }
}
