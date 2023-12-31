﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SQLite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SqlitePlainStorage<TEntity> : SqlitePlainStorage<TEntity, string>,
        IPlainStorage<TEntity>
        where TEntity : class, IPlainStorageEntity, new()
    {
        public SqlitePlainStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings)
            : base(logger, fileSystemAccessor, settings)
        {
        }

        public SqlitePlainStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage,
            logger)
        {
        }
    }

    public class SqlitePlainStorage<TEntity, TKey> : IPlainStorage<TEntity, TKey>
        where TEntity : class, IPlainStorageEntity<TKey>, new()
    {
        private SQLiteConnectionWithLock connection;
        protected readonly ILogger logger;
        protected readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly SqliteSettings settings;

        public SqlitePlainStorage(ILogger logger,
            IFileSystemAccessor fileSystemAccessor,
            SqliteSettings settings)
        {
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.settings = settings;
        }

        protected virtual SQLiteConnectionWithLock GetConnection()
        {
            if (this.connection != null)
                return this.connection;

            var pathToDatabase = GetPathToDatabase();
            this.connection = CreateConnection(pathToDatabase);
            return this.connection;
        }

        protected virtual SQLiteConnectionWithLock CreateConnection()
        {
            var pathToDatabase = GetPathToDatabase();
            return CreateConnection(pathToDatabase);
        }

        protected SQLiteConnectionWithLock CreateConnection(string pathToDatabase)
        {
            var dbDirectory = fileSystemAccessor.GetDirectory(pathToDatabase);
            if (!fileSystemAccessor.IsDirectoryExists(dbDirectory))
                fileSystemAccessor.CreateDirectory(dbDirectory);

            var sqliteConnectionString = new SQLiteConnectionString(pathToDatabase,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true);
            var newConnection = new SQLiteConnectionWithLock(sqliteConnectionString);

            CreateTable(newConnection);
            
            return newConnection;
        }


        protected virtual void CreateTable(SQLiteConnectionWithLock connect)
        {
            connect.CreateTable<TEntity>();
        }

        protected virtual string GetPathToDatabase()
            => fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, typeof(TEntity).Name + "-data.sqlite3");

        public SqlitePlainStorage(SQLiteConnectionWithLock storage, ILogger logger)
        {
            this.connection = storage;
            this.logger = logger;
            this.connection.CreateTable<TEntity>();
        }

        public virtual TEntity GetById(TKey id)
        {
            return RunInTransaction(table => table.Connection.Find<TEntity>(id));
        }

        public virtual void Remove(TKey id)
        {
            RunInTransaction(table =>
            {
                table.Connection.Delete<TEntity>(id);
                OnRemove(table, id);
            });
        }

        public virtual void Remove(IEnumerable<TEntity> entities)
        {
            try
            {
                RunInTransaction(table =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                    {
                        table.Connection.Delete(entity);
                        OnRemove(table, entity.Id);
                    }
                });

            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to remove {entities.Count()} entities as batch", ex);
                throw;
            }
        }

        public virtual void Store(TEntity entity)
        {
            this.Store(new[] { entity });
        }

        public virtual void Store(IEnumerable<TEntity> entities)
        {
            try
            {
                RunInTransaction(table =>
                {
                    foreach (var entity in entities.Where(entity => entity != null))
                    {
                        table.Connection.InsertOrReplace(entity);
                        OnStore(table, entity);
                    }
                });
            }
            catch (SQLiteException ex)
            {
                this.logger.Fatal($"Failed to persist {entities.Count()} entities {typeof(TEntity).Name} as batch", ex);
                throw;
            }
        }

        public virtual void OnStore(TableQuery<TEntity> query, TEntity entity) { }
        public virtual void OnRemove(TableQuery<TEntity> query, TKey entityId) { }

        public IReadOnlyCollection<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => ToModifiedCollection(table.Where(predicate)).ToReadOnlyCollection());

        public IReadOnlyCollection<TResult> WhereSelect<TResult>(Expression<Func<TEntity, bool>> wherePredicate,
            Expression<Func<TEntity, TResult>> selectPredicate) where TResult : class
        {
            return this.RunInTransaction(
                table => table
                    .Where(wherePredicate).AsQueryable()
                    .Select(selectPredicate)
                    .ToReadOnlyCollection());
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => table.Count(predicate));

        public int Count()
            => this.RunInTransaction(table => table.Count());

        public TEntity FirstOrDefault() => this.RunInTransaction(table => ToModifiedEntity(table.FirstOrDefault()));

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
            => this.RunInTransaction(table => table.FirstOrDefault(predicate));

        public virtual IReadOnlyCollection<TEntity> LoadAll()
        {
            return this.RunInTransaction(table => ToModifiedCollection(table.Connection.Table<TEntity>()).ToReadOnlyCollection());
        }

        public IReadOnlyCollection<TEntity> FixedQuery(Expression<Func<TEntity, bool>> wherePredicate, 
            Expression<Func<TEntity, int>> orderPredicate, int takeCount, int skip = 0)
            => this.RunInTransaction(
                table => ToModifiedCollection(table.Where(wherePredicate)
                        .OrderBy(orderPredicate)
                        .Skip(skip)
                        .Take(takeCount))
                    .ToReadOnlyCollection());

        public IReadOnlyCollection<TResult> FixedQueryWithSelection<TResult>(
            Expression<Func<TEntity, bool>> wherePredicate, 
            Expression<Func<TEntity, int>> orderPredicate,
            Expression<Func<TEntity, TResult>> selectPredicate,
            int takeCount, int skip = 0) where TResult : class
            => this.RunInTransaction(
                table => table.Where(wherePredicate)
                    .OrderBy(orderPredicate)
                    .Skip(skip)
                    .Take(takeCount)
                    .AsQueryable()
                    .Select(selectPredicate)
                    .ToReadOnlyCollection()
            );

        public virtual void RemoveAll()
        {
            RunInTransaction(table => table.Connection.DeleteAll<TEntity>());
        }

        public virtual void Dispose()
        {
            connection?.Dispose();
        }

        protected TResult RunInTransaction<TResult>(Func<TableQuery<TEntity>, TResult> function)
        {
            var connect = GetConnection();
            return this.RunInTransaction(connect, function);
        }
        
        protected TResult RunInTransaction<TResult>(SQLiteConnectionWithLock connect, Func<TableQuery<TEntity>, TResult> function)
        {
            TResult result = default(TResult);
            using (connect.Lock())
                connect.RunInTransaction(() => result = function.Invoke(connect.Table<TEntity>()));
            return result;
        }

        protected void RunInTransaction(Action<TableQuery<TEntity>> function)
        {
            var connect = GetConnection();
            using (connect.Lock())
            {
                connect.RunInTransaction(
                    () => function.Invoke(connect.Table<TEntity>()));
            }
        }

        private IEnumerable<TEntity> ToModifiedCollection(IEnumerable<TEntity> entity)
            => entity.Select(ToModifiedEntity);

        protected virtual TEntity ToModifiedEntity(TEntity entity) => entity;
    }
}
