using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;

namespace SupportChatbot.API.Repositories
{
    public class Repository<K, T> : IRepository<K, T> where T : class
    {
        private readonly SupportChatbotContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(SupportChatbotContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(K id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null");
            }
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found");
            }
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(K id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null");
            }
            return await _dbSet.FindAsync(id);
        }

        public Task<bool> SaveChangesAsync()
        {
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }

        public async Task<T> UpdateAsync(K key, T entity)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null");
            }
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            var existing = await GetByIdAsync(key);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Entity with ID {key} not found");
            }
            _context.Entry(existing).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
            return existing;
        }
        
        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}