using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LinqToDB.EntityFrameworkCore.SQLite.Tests.Models.Identity
{
    public sealed class EfCoreSqliteInMemoryDbFactory : IDisposable, IAsyncDisposable
    {
        private readonly string _connectionString;
        private SqliteConnection? _connection;

        public EfCoreSqliteInMemoryDbFactory()
        {
            _connectionString = "DataSource=:memory:";
        }

        public EfCoreSqliteInMemoryDbFactory(string connectionString)
        {
            _connectionString = connectionString;
        }


        public async ValueTask DisposeAsync()
        {
            await Task.Run(Dispose);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public T CreateDbContext<T>(bool addVersionGeneratorTrigger = true, Action<string> logger = null)
            where T : DbContext
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>().UseSqlite(_connection);

            if (logger != null)
                optionsBuilder.LogTo(logger);

            var context = (T) Activator.CreateInstance(typeof(T), optionsBuilder.Options);

            context?.Database.EnsureCreated();

            if (!addVersionGeneratorTrigger)
                return context;

            AddVersionTrigger(context);

            return context;
        }

        private void AddVersionTrigger<T>(T context) where T : DbContext
        {
            var tables = context.Model.GetEntityTypes();

            foreach (var table in tables)
            {
                var props = table.GetProperties().Where(p =>
                    p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate &&
                    p.IsConcurrencyToken);

                var tableName = table.GetTableName();

                foreach (var field in props)
                {
                    string[] sqlStrings =
                    {
                        $@"CREATE TRIGGER Set{tableName}_{field.Name}OnUpdate
                AFTER UPDATE ON {tableName}
                BEGIN
                    UPDATE {tableName}
                    SET {field.Name} = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
                ",
                        $@"CREATE TRIGGER Set{tableName}_{field.Name}OnInsert
                AFTER INSERT ON {tableName}
                BEGIN
                    UPDATE {tableName}
                    SET {field.Name} = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
                "
                    };

                    foreach (var sql in sqlStrings)
                    {
                        using var command = _connection.CreateCommand();
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void ReleaseUnmanagedResources()
        {
            _connection.Dispose();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();

            if (disposing)
            {
            }
        }

        ~EfCoreSqliteInMemoryDbFactory()
        {
            Dispose(false);
        }
    }

}
