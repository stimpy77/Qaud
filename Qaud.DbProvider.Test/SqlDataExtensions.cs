using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qaud.DbProvider.Test
{
    public static class SqlDataExtensions
    {
        public static Dictionary<string, object> RowAsDictionary(this DbDataReader dataReader)
        {
            var columns = new List<string>();
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                columns.Add(dataReader.GetName(i));
            }
            var dic = new Dictionary<string, object>();
            foreach (var column in columns)
            {
                dic[column] = dataReader[column];
            }

            return dic;
        }
    }
}
