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
            public TestEFDataStoreContext() : base("TestDb")
            {

            }

            public DbSet<FooModel> FooModels { get; set; }

            public void SetupDB()
            {
                if (Database.Exists() && !DBCreated) Database.Delete();
                else if (!Database.Exists()) Database.Create();
                DBCreated = true;
            }
        }

        internal static bool DBCreated;
        public EFDataSetDataStore<FooModel> Create()
        {
            var context = new TestEFDataStoreContext();
            context.SetupDB();
            var dstore = new EFDataSetDataStore<FooModel>(context.FooModels, context);
            return dstore;
        }
    }
}
