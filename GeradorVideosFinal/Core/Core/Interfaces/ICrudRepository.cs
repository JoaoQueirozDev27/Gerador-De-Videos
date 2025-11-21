using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface ICrudRepository<T> where T : class
    {        
        Task Add(T obj);
        Task<T> GetById(int id);
        Task<List<T>> GetAll();
        Task Update(T obj);
        Task Delete(int id);
    }
}
