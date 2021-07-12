namespace Cryptofolio.Infrastructure.Entities
{
    /// <summary>
    /// Models a setting.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }
    }
}
