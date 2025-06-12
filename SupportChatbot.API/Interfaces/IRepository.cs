namespace SupportChatbot.API.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        public Task<IEnumerable<T>> GetAllAsync();
        public Task<T?> GetByIdAsync(K id);
        public Task<T> AddAsync(T entity);
        public Task<T> UpdateAsync(K key, T entity);
        public Task<T> DeleteAsync(K id);
        public Task<bool> SaveChangesAsync();
    }
}