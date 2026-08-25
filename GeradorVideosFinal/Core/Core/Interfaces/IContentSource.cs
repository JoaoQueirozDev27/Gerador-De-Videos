using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IContentSource
    {
        string Name { get; }
        Type RequestType { get; }
        Type ContentType { get; }
        Task<List<IContent>> GetContent(object request);
    }

    public interface IContentSource<TRequest> : IContentSource
    {
        Task<List<IContent>> GetContent(TRequest request);
    }
}
