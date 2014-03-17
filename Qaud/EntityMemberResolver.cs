using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Qaud
{
    /// <summary>
    /// Just a reflection utility that resolves and hydrates the properties of an object using reflection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntityMemberResolver<T>
    {
        private IEnumerable<PropertyInfo> _keyMembers;
        private IEnumerable<PropertyInfo> _publicProperties;

        public IEnumerable<PropertyInfo> KeyPropertyMembers
        {
            get
            {
                if (_keyMembers == null)
                {
                    var properties = typeof (T).GetProperties();
                    _keyMembers =
                        from member in properties
                        where
                            member.GetCustomAttributes(typeof (System.ComponentModel.DataAnnotations.KeyAttribute), true)
                                .Any()
                        select member;
                    if (!_keyMembers.Any())
                    {
                        _keyMembers = properties.Where(p => p.Name.ToUpper() == "ID" || p.Name.ToUpper() == typeof(T).Name.ToUpper() + "ID");
                    }
                }
                return _keyMembers;
            }
        }

        /// <summary>
        /// Grabs the key column propert(ies) of an item mappable to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IEnumerable<object> GetKeyPropertyValues(object item)
        {
            var itemType = item.GetType();
            return KeyPropertyMembers.Select(p => itemType.GetProperty(p.Name).GetValue(item, new object[] {}));
        }

        /// <summary>
        /// Returns all properties as a dictionary.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, object> ConvertToDictionary(object item)
        {
            var datarow = item as DbDataReader;
            
            if (datarow == null)
                return item.GetType().GetProperties().ToDictionary(property
                    => property.Name, property => property.GetValue(item, new object[] {}));

            var dictionary = new Dictionary<string, object>();
            var columns = datarow.GetSchemaTable().Columns;
            foreach (DataColumn column in columns)
            {
                dictionary[column.ColumnName] = datarow[column.ColumnName];
            }
            return dictionary;
        }

        /// <summary>
        /// Mutates the given <paramref name="current"/> model with any properties that are different in the <paramref name="modified"/> model.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="modified"></param>
        public void ApplyChanges(T current, T modified)
        {
            var vals = ConvertToDictionary(modified);
            var props = _publicProperties ?? (_publicProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance));
            foreach (var prop in props.Where(prop => prop.GetValue(current, new object[] {}) != vals[prop.Name]))
            {
                prop.SetValue(current, vals[prop.Name], new object[] {});
            }
        }

        public void ApplyPartial(T target, object changes)
        {
            IDictionary<string, object> dic;
            if (!(changes is IDictionary<string, object>))
            {
                dic = ConvertToDictionary(changes);
            }
            else dic = (Dictionary<string, object>)changes;
            foreach (var kvp in dic)
            {
                typeof(T).GetProperty(kvp.Key).SetValue(target, kvp.Value, new object[] {});
            }
        }

        public System.Type TypeOfProperty(string property)
        {
            return typeof (T).GetProperty(property).PropertyType;
        }

        /// <summary>
        /// Returns <value>true</value> if the specified member has <see cref="DatabaseGeneratedOption.Identity"/> attribute.
        /// </summary>
        /// <param name="membername"></param>
        /// <returns></returns>
        public bool IsAutoIdentity(string membername)
        {
            var property = typeof (T).GetProperty(membername);
            return property.CustomAttributes.Any(
                a => a.NamedArguments != null && a.NamedArguments.Any(
                    r => r.MemberName == "DatabaseGeneratedOption" && r.TypedValue.Value.ToString() == "Identity"));
        }

        /// <summary>
        /// Populates the given object with the values from the given dictionary.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dic"></param>
        public void HydrateFromDictionary(T obj, Dictionary<string, object> dic)
        {
            foreach (var kvp in dic)
            {
                var prop = typeof (T).GetProperty(kvp.Key);
                var value = kvp.Value;
                if (value == DBNull.Value) value = null;
                prop.SetValue(obj, value);
            }
        }
    }
}
