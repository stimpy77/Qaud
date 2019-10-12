using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        /// <summary>
        /// Returns the property members that are not attributed with <see cref="KeyAttribute"/>
        /// </summary>
        public virtual IEnumerable<PropertyInfo> NonKeyPropertyMembers
        {
            get
            {
                return typeof(T).GetProperties(BindingFlags.Instance|BindingFlags.Public)
                    .Where(p => !KeyPropertyMembers.Contains(p));
            }
        }

        /// <summary>
        /// Returns the property members that are attributed with <see cref="KeyAttribute"/>
        /// </summary>
        public virtual IEnumerable<PropertyInfo> KeyPropertyMembers
        {
            get
            {
                if (_keyMembers == null)
                {
                    var properties = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    _keyMembers =
                        from member in properties
                        where
                            member.GetCustomAttributes(typeof (KeyAttribute), true)
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
        public virtual IEnumerable<object> GetKeyPropertyValues(object item)
        {
            var itemType = item.GetType();
            return KeyPropertyMembers.Select(p => itemType.GetProperty(p.Name).GetValue(item));
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
                return item.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance)
                    .ToDictionary(property
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

        /// <summary>
        /// Maps the values of the given <paramref name="changes"/> to the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="changes"></param>
        public void ApplyPartial(T target, IDictionary<string, object> changes)
        {
            foreach (var kvp in changes)
            {
                typeof(T).GetProperty(kvp.Key).SetValue(target, kvp.Value, new object[] { });
            }
        }

        /// <summary>
        /// Maps the values of the given <paramref name="changes"/> to the given <paramref name="target"/>.
        /// The <paramref name="changes"/> parameter must be an
        /// object that can be converted to a dictionary by reflecting its properties.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="changes"></param>
        public void ApplyPartial<TChanges>(T target, TChanges changes)
        {
            if (changes is IDictionary<string, object> dic)
            {
                ApplyPartial(target, dic);
                return;
            }
            ApplyPartial(target, ConvertToDictionary(changes));
        }

        /// <summary>
        /// Returns the PropertyType of the member of <typeparamref name="T"/> having the given property name.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public Type PropertyType(string property)
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
