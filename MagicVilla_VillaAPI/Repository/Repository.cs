using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T>:IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet; //in constructor we are passing _db.Set<T> where T is the villas means we are getting _db.Villas 
        // above code is similar to _db.Villas but it is generic now 
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, int pageSize = 3, int pageNumber = 1)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (pageSize > 0)
            {
                if (pageSize > 100)
                {
                    pageSize = 100;
                }
                //pagination
                /*
                 --> pageSize: how many records want to display on the page.
                 suppose page size==5 and we want data on page number which means first 10 record we to skip
                 query=query.Skip(5*(3-1)).Take(5) -->skip first 10 records and display next 5 records on page 3                 
                 */

                query=query.Skip(pageSize*(pageNumber-1)).Take(pageSize);
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> ?filter = null, bool tracked = true)
        {
            IQueryable<T> query = dbSet;
            if (!tracked)
            {
                query = query.AsNoTracking();
            }
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(T entity)
        {
           dbSet.Remove(entity);
            await SaveAsync();

        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
