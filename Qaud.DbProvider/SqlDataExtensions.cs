using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qaud.DbProvider
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

        internal static void PrepareParams(this DbParameterCollection parameters)
        {

            foreach (DbParameter param in parameters)
            {
                if (param.Value == null) param.Value = DBNull.Value;
                if (param.DbType.ToString().ToLower().Contains("string"))
                    param.Size = (param.Size == 0) ? ((param.Value == DBNull.Value || param.Value.ToString().Length == 0) ? 1 : param.Value.ToString().Length) : param.Size;
            }
        }
    }
}
