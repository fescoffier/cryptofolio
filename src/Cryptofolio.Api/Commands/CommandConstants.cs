using Cryptofolio.Infrastructure;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Provides constants for the command layer.
    /// </summary>
    public static class CommandConstants
    {
        /// <summary>
        /// Wallet commands related constants.
        /// </summary>
        public static class Wallet
        {
            /// <summary>
            /// Error constants.
            /// </summary>
            public static class Errors
            {
                public const string CreateError = "An error has occured while creating the wallet.";

                public const string UpdateError = "An error has occured while updating the wallet.";

                public const string DeleteError = "An error has occured while deleting the wallet.";

                public const string DeleteSelectedError = "The selected wallet can't be deleted.";
            }
        }

        /// <summary>
        /// Transaction commands related constants.
        /// </summary>
        public static class Transaction
        {
            /// <summary>
            /// Transaction types.
            /// </summary>
            public static class Types
            {
                public const string Buy = InfrastructureConstants.Transactions.Types.Buy;

                public const string Sell = InfrastructureConstants.Transactions.Types.Sell;

                public const string Transfer = InfrastructureConstants.Transactions.Types.Transfer;
            }

            /// <summary>
            /// Error constants.
            /// </summary>
            public static class Errors
            {
                public const string CreateError = "An error has occured while creating the transaction.";

                public const string CreateInvalidWalletError = "An error has occured while creating the transaction, the selected wallet is invalid.";

                public const string UpdateError = "An error has occured while updating the transaction.";

                public const string DeleteError = "An error has occured while deleting the transaction.";
            }
        }
    }
}
