using System.Collections.Generic;
using System.Linq;

namespace Cryptofolio.Api.Commands
{
    /// <summary>
    /// Provides a result wrapper for <see cref="CommandBase"/> implementations.
    /// </summary>
    public class CommandResult
    {
        private static readonly CommandResult SuccessCommandResult = new(Enumerable.Empty<string>());

        /// <summary>
        /// The error list.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Defines if the command succeeded.
        /// </summary>
        public bool Succeeded { get; }

        protected CommandResult(IEnumerable<string> errors)
        {
            Errors = errors.ToList().AsReadOnly();
            Succeeded = !Errors.Any();
        }

        /// <summary>
        /// Creates a succeeded <see cref="CommandResult"/>.
        /// </summary>
        /// <returns>The <see cref="CommandResult"/> instance.</returns>
        public static CommandResult Success() => SuccessCommandResult;

        /// <summary>
        /// Creates a failed <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="errors">The error list.</param>
        /// <returns>The <see cref="CommandResult"/> instance.</returns>
        public static CommandResult Failed(IEnumerable<string> errors) => new(errors);

        /// <summary>
        /// Creates a failed <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="errors">The error list.</param>
        /// <returns>The <see cref="CommandResult"/> instance.</returns>
        public static CommandResult Failed(params string[] errors) => new(errors.AsEnumerable());
    }

    /// <summary>
    /// Provides a wrapper for <see cref="CommandBase"/> implementations that returns data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CommandResult<T> : CommandResult
    {
        /// <summary>
        /// The data.
        /// </summary>
        public T Data { get; }

        protected CommandResult(T data) : base(Enumerable.Empty<string>())
        {
            Data = data;
        }

        protected CommandResult(IEnumerable<string> errors) : base(errors)
        {
        }

        /// <summary>
        /// Creates a succeeded <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The <see cref="CommandResult{T}"/> instance.</returns>
        public static CommandResult<T> Success(T data) => new(data);

        /// <summary>
        /// Creates a failed <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="errors">The error list.</param>
        /// <returns>The <see cref="CommandResult{T}"/> instance.</returns>
        public static new CommandResult<T> Failed(IEnumerable<string> errors) => new(errors);

        /// <summary>
        /// Creates a failed <see cref="CommandResult"/>.
        /// </summary>
        /// <param name="errors">The error list.</param>
        /// <returns>The <see cref="CommandResult{T}"/> instance.</returns>
        public static new CommandResult<T> Failed(params string[] errors) => new(errors.AsEnumerable());
    }
}
