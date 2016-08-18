using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConcurrentcyApp.Model
{
    public class TestConcurrentcyDbContext : DbContext
    {
        static TestConcurrentcyDbContext()
        {
            Database.SetInitializer<TestConcurrentcyDbContext>(new DropCreateDatabaseIfModelChanges<TestConcurrentcyDbContext>());
        }

        public TestConcurrentcyDbContext() : base("TestConcurrentcyConnectionString")
        {
            this.Database.Log = Console.WriteLine;
        }

        public TestConcurrentcyDbContext(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
            this.Database.Log = Console.WriteLine;
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<OutputAccount> OutputAccounts { get; set; }
        public DbSet<InputAccount> InputAccounts { get; set; }
        public DbSet<TestCreateScript> TestCreateScripts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Student>().Property(x => x.RowVersion).IsRowVersion();
            //modelBuilder.Entity<Student>().Property(x => x.FirstName).IsConcurrencyToken();

            // 禁用默认表名复数形式
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // 禁用一对多级联删除
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            // 禁用多对多级联删除
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
        }
    }
}
