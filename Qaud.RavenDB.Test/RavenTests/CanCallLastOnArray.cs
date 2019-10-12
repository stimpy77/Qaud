using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Indexes;
using Raven.Tests.Helpers;
using Xunit;
using Assert = Xunit.Assert;

namespace Qaud.RavenDB.Test.RavenTests
{
    [TestClass]
	public class CanCallLastOnArray : RavenTestBase
	{
		private class Student
		{
			public string Email { get; set; }
		}

		private class Students_ByEmailDomain : AbstractIndexCreationTask<Student, Students_ByEmailDomain.Result>
		{
			public class Result
			{
				public string EmailDomain { get; set; }
				public int Count { get; set; }
			}

			public Students_ByEmailDomain()
			{
				Map = students => from student in students
				                  select new
				                  {
					                  EmailDomain = student.Email.Split('@').Last(),
					                  Count = 1,
				                  };

				Reduce = results => from result in results
				                    group result by result.EmailDomain
				                    into g
				                    select new
				                    {
					                    EmailDomain = g.Key,
					                    Count = g.Sum(r => r.Count),
				                    };
			}
		}

        [Fact, TestMethod]
		public void WillSupportLast()
		{
			using (var store = NewDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new Student {Email = "support@hibernatingrhinos.com"});
					session.SaveChanges();
				}

				new Students_ByEmailDomain().Execute(store);

				using (var session = store.OpenSession())
				{
					var results = session.Query<Students_ByEmailDomain.Result, Students_ByEmailDomain>()
					                     .Customize(customization => customization.WaitForNonStaleResults())
					                     .ToList();

					Assert.Empty(store.DatabaseCommands.GetStatistics().Errors);
					Assert.Equal(1, results.Count);
				}
			}
		}
	}
}