using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Qaud.Test;

namespace Qaud.EntityFramework.Test
{
    public class TestEFDataStoreGenerator
    {
        public class TestEFDataStoreContext : DbContext
        {
            public DbSet<FooModel> FooModels { get; set; }

            public void SetupDB()
            {
                if (Database.Exists()) Database.Delete();
                Database.Create();
            }
        }
        public EFDataStore<FooModel> Create()
        {
            var context = new TestEFDataStoreContext();
            context.SetupDB();
            var dstore = new EFDataStore<FooModel>(context.FooModels, context);
            return dstore;
        }
    }
}
