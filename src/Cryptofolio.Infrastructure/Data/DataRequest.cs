using MediatR;
using System;

namespace Cryptofolio.Infrastructure.Data
{
    public abstract class DataRequest : IRequest
    {
        public string TraceIdentifier { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
