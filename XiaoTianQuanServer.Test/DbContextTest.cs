using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using XiaoTianQuanServer.Data;
using Xunit;

namespace XiaoTianQuanServer.Test
{
    public abstract class DbContextTest : IAsyncLifetime
    {
        protected ApplicationDbContext DbContext;
        protected SqliteConnection SqliteConnection;

        private async Task InitContext()
        {
            SqliteConnection = new SqliteConnection("DataSource=:memory:");
            SqliteConnection.Open();

            var builder =
                new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(SqliteConnection);

            var context = new ApplicationDbContext(builder.Options);

            await context.Database.EnsureCreatedAsync();

            CreateDatabaseItems(DbContext);

            await context.SaveChangesAsync();
            DbContext = context;
        }

        protected abstract void CreateDatabaseItems(ApplicationDbContext context);

        public virtual Task InitializeAsync()
        {
            return InitContext();
        }

        public virtual Task DisposeAsync()
        {
            SqliteConnection.Close();
            return Task.CompletedTask;
        }
    }
}
