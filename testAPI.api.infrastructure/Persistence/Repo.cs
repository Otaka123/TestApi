using System.Linq.Expressions;
using testAPI.api.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static testAPI.api.infrastructure.Persistence.Interface.IRepo;

namespace testAPI.api.infrastructure.Persistence
{
    public class Repo
    {
        public class Repository<T> : IRepo<T> where T : class
        {
            private readonly AppDbContext _db;
            internal DbSet<T> dbSet;

            public Repository(AppDbContext db)
            {
                _db = db ?? throw new ArgumentNullException(nameof(db));
                this.dbSet = _db.Set<T>();
            }

            public async Task<List<T>> GetAllAsync(
                Expression<Func<T, bool>>? filter = null,
                string? includeProperties = null,
                bool isTracking = false,
                Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                int? take = null,
                CancellationToken cancellationToken = default)
            {
                IQueryable<T> query = isTracking ? dbSet : dbSet.AsNoTracking();

                if (filter != null)
                    query = query.Where(filter);

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        query = query.Include(includeProperty.Trim());
                }

                if (orderBy != null)
                    query = orderBy(query);

                if (take.HasValue && take.Value > 0)
                    query = query.Take(take.Value);

                return await query.ToListAsync(cancellationToken);
            }

            public IQueryable<T> Query(bool isTracking = false)
            {
                return isTracking ? dbSet : dbSet.AsNoTracking();
            }

            public async Task<T?> GetAsync(
                Expression<Func<T, bool>> filter,
                string? includeProperties = null,
                bool isTracking = false,
                CancellationToken cancellationToken = default)
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                IQueryable<T> query = isTracking ? dbSet : dbSet.AsNoTracking();
                query = query.Where(filter);

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        query = query.Include(includeProperty.Trim());
                }

                return await query.FirstOrDefaultAsync(cancellationToken);
            }

            public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            {
                if (predicate == null)
                    throw new ArgumentNullException(nameof(predicate));

                return await dbSet.AsNoTracking().AnyAsync(predicate, cancellationToken);
            }

            public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                await dbSet.AddAsync(entity, cancellationToken);
                await SaveAsync(cancellationToken);
            }

            public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                dbSet.Remove(entity);
                await SaveAsync(cancellationToken);
            }

            public async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                var entitiesList = entities.ToList();
                if (!entitiesList.Any())
                    return;

                dbSet.RemoveRange(entitiesList);
                await SaveAsync(cancellationToken);
            }

            public async Task SaveAsync(CancellationToken cancellationToken = default)
            {
                await _db.SaveChangesAsync(cancellationToken);
            }

            public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                var entitiesList = entities.ToList();
                if (!entitiesList.Any())
                    return;

                await dbSet.AddRangeAsync(entitiesList, cancellationToken);
                await SaveAsync(cancellationToken);
            }

            public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
            {
                if (predicate != null)
                    return await dbSet.AsNoTracking().CountAsync(predicate, cancellationToken);

                return await dbSet.AsNoTracking().CountAsync(cancellationToken);
            }

            public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                dbSet.Update(entity);
                await SaveAsync(cancellationToken);
            }

            public async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                var entitiesList = entities.ToList();
                if (!entitiesList.Any())
                    return;

                dbSet.UpdateRange(entitiesList);
                await SaveAsync(cancellationToken);
            }

            public async Task<T?> GetByIdAsync(int id, bool isTracking = false, CancellationToken cancellationToken = default)
            {
                if (isTracking)
                {
                    return await dbSet.FindAsync(new object[] { id }, cancellationToken);
                }
                else
                {
                    var entity = await dbSet.AsNoTracking().FirstOrDefaultAsync(
                        e => EF.Property<int>(e, "Id") == id, cancellationToken);
                    return entity;
                }
            }
        }
    }
}
