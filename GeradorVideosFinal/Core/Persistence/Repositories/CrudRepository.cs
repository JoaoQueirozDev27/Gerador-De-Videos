using Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.context;

namespace Persistence.Repositories
{
    public class CrudRepository<T> : ICrudRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _entityDbTable;
        public CrudRepository(AppDbContext context)
        {
            _context = context;
            _entityDbTable = _context.Set<T>();
        }

        public async Task Add(T obj)
        {
            _entityDbTable.Add(obj);
            await _context.SaveChangesAsync();
            return;
        }

        public async Task Delete(int id)
        {
            T _entity = await GetById(id);
            _entityDbTable.Remove(_entity);
            await _context.SaveChangesAsync();
            return;
        }

        public Task<List<T>> GetAll()
        {
            return _entityDbTable.ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            var entity = await _entityDbTable.FindAsync(id);
            return entity;
        }

        public async Task Update(T obj)
        {
            _entityDbTable.Update(obj);
            await _context.SaveChangesAsync();
            return;
        }
    }
}
