using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Cryptofolio.App
{
    /// <summary>
    /// Configuration options for the <see cref="IdentityUserService"/> service.
    /// </summary>
    public class IdentityUserServiceOptions
    {
        /// <summary>
        /// The default identity users.
        /// </summary>
        public IEnumerable<IdentityUser> Users { get; set; }
    }
}
