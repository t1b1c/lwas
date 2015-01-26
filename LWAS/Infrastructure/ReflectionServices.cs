/*
 * Copyright 2006-2015 TIBIC SOLUTIONS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Diagnostics;

namespace LWAS.Infrastructure
{
	public static class ReflectionServices
	{
		public static Control FindControlEx(string id, Control parent)
		{
			Control ret = parent.FindControl(id);
			if (null == ret)
			{
				foreach (Control child in parent.Controls)
				{
					ret = ReflectionServices.FindControlEx(id, child);
					if (null != ret)
					{
						break;
					}
				}
			}
			return ret;
		}
		public static object CreateInstance(string typeAssemblyQualifiedName)
		{
			if (string.IsNullOrEmpty(typeAssemblyQualifiedName))
			{
				throw new ArgumentNullException("typeAssemblyQualifiedName");
			}
			Type type = Type.GetType(typeAssemblyQualifiedName);
			if (null == type)
			{
				throw new ArgumentException("Can't find type " + typeAssemblyQualifiedName);
			}
			return Activator.CreateInstance(type);
		}
		public static object StaticInstance(string typeAssemblyQualifiedName, string staticMember)
		{
			if (string.IsNullOrEmpty(typeAssemblyQualifiedName))
			{
				throw new ArgumentNullException("typeAssemblyQualifiedName");
			}
			if (string.IsNullOrEmpty(staticMember))
			{
				throw new ArgumentNullException("staticMember");
			}
			Type type = Type.GetType(typeAssemblyQualifiedName);
			if (null == type)
			{
				throw new ArgumentException("Can't find type " + typeAssemblyQualifiedName);
			}
			FieldInfo fi = type.GetField(staticMember);
			if (null == fi)
			{
				throw new ArgumentException(string.Format("'{0}' is not a static field", staticMember));
			}
			return fi.GetValue(null);
		}
		public static Dictionary<string, object> ToDictionary(object target)
		{
			Dictionary<string, object> result2;
			if (null == target)
			{
				result2 = null;
			}
			else
			{
				if (target is Dictionary<string, object>)
				{
					result2 = (target as Dictionary<string, object>);
				}
				else
				{
					if (target is KeyValuePair<string, object>)
					{
						KeyValuePair<string, object> kvp = (KeyValuePair<string, object>)target;
						result2 = new Dictionary<string, object>
						{

							{
								kvp.Key, 
								kvp.Value
							}
						};
					}
					else
					{
						if (target is DataRowView)
						{
							result2 = ReflectionServices.ToDictionary((DataRowView)target);
						}
						else
						{
							if (target is DataRow)
							{
								result2 = ReflectionServices.ToDictionary((DataRow)target);
							}
							else
							{
								if (target is IList)
								{
									result2 = ReflectionServices.ToDictionary((IList)target);
								}
								else
								{
									if (target is IDictionary)
									{
										result2 = ReflectionServices.ToDictionary((IDictionary)target);
									}
									else
									{
                                        if (target is IDataRecord)
                                        {
                                            result2 = ReflectionServices.ToDictionary((IDataRecord)target);
                                        }
                                        else
                                        {
                                            if (target is IHierarchyData)
                                                result2 = ToDictionary((IHierarchyData)target);
                                            else
                                            {
                                                Dictionary<string, object> result3 = new Dictionary<string, object>();
                                                PropertyInfo[] properties = target.GetType().GetProperties();
                                                for (int i = 0; i < properties.Length; i++)
                                                {
                                                    PropertyInfo propertyInfo = properties[i];
                                                    if (0 == propertyInfo.GetIndexParameters().Length)
                                                    {
                                                        result3.Add(propertyInfo.Name, propertyInfo.GetValue(target, null));
                                                    }
                                                }
                                                result2 = result3;
                                            }
                                        }
									}
								}
							}
						}
					}
				}
			}
			return result2;
		}
        public static Dictionary<string, object> ToDictionary(DataTable target)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (DataColumn column in target.Columns)
            {
                if (result.ContainsKey(column.ColumnName)) throw new InvalidOperationException(String.Format("ToDictionary failed. Data containes duplicate column '{0}'", column.ColumnName));
                result.Add(column.ColumnName, null);
            }
            if (target.Rows.Count > 0)
            {
                foreach (DataColumn column in target.Columns)
                    result[column.ColumnName] = target.Rows[0][column];
            }
            return result;
        }
		public static Dictionary<string, object> ToDictionary(DataRowView target)
		{
			return ReflectionServices.ToDictionary(target.Row);
		}
		public static Dictionary<string, object> ToDictionary(DataRow target)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			foreach (DataColumn column in target.Table.Columns)
			{
                if (result.ContainsKey(column.ColumnName)) throw new InvalidOperationException(String.Format("ToDictionary failed. Data containes duplicate column '{0}'", column.ColumnName));
                result.Add(column.ColumnName, target[column]);
			}
			return result;
		}
		public static Dictionary<string, object> ToDictionary(IList target)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			for (int index = 0; index < target.Count; index++)
			{
				result.Add(index.ToString(), target[index]);
			}
			return result;
		}
		public static Dictionary<string, object> ToDictionary(IDictionary target)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			foreach (DictionaryEntry entry in target)
			{
                if (result.ContainsKey(entry.Key.ToString())) throw new InvalidOperationException(String.Format("ToDictionary failed. Data containes duplicate column '{0}'", entry.Key.ToString()));
                result.Add(entry.Key.ToString(), entry.Value);
			}
			return result;
		}
		public static Dictionary<string, object> ToDictionary(IDataRecord target)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			for (int i = 0; i < target.FieldCount; i++)
			{
                if (result.ContainsKey(target.GetName(i))) throw new InvalidOperationException(String.Format("ToDictionary failed. Data containes duplicate column '{0}'", target.GetName(i)));
                result.Add(target.GetName(i), target.GetValue(i));
			}
			return result;
		}
        public static Dictionary<string, object> ToDictionary(IHierarchyData data)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (null != data)
            {
                result.Add("Type", data.Type);
                result.Add("Path", data.Path);
                if (data.Item is XmlNode)
                {
                    XmlNode nodeItem = data.Item as XmlNode;
                    foreach (XmlAttribute attribute in nodeItem.Attributes)
                    {
                        if ("Type" == attribute.Name) throw new ArgumentException("Invalid attribute name 'Type'. That name has a different meaning in hierachical data object");
                        if ("Path" == attribute.Name) throw new ArgumentException("Invalid attribute name 'Path'. That name has a different meaning in hierachical data object");
                        result.Add(attribute.Name, attribute.Value);
                    }
                }
            }
            return result;
        }
		public static object ExtractValue(object source, string propertyName)
		{
			return ReflectionServices.ExtractValue(source, propertyName, propertyName);
		}
		public static object ExtractValue(object source, string propertyName, string originalPropertyName)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source object");
			}
			if (string.IsNullOrEmpty(propertyName))
			{
				throw new ArgumentNullException("source property name");
			}
			string childProperty = null;
			if (propertyName.StartsWith("'"))
			{
				propertyName = propertyName.Substring(1);
				childProperty = propertyName.Substring(propertyName.IndexOf("'") + 1);
				if (childProperty.StartsWith("."))
				{
					childProperty = childProperty.Substring(1);
				}
				propertyName = propertyName.Substring(0, propertyName.IndexOf("'"));
			}
			else
			{
				if (propertyName.Contains("."))
				{
					childProperty = propertyName.Substring(propertyName.IndexOf(".") + 1);
					propertyName = propertyName.Substring(0, propertyName.IndexOf("."));
				}
			}
			object val = null;
			if (source is DataRowView)
			{
				val = ReflectionServices.ExtractValue((DataRowView)source, propertyName);
			}
			else
			{
				if (source is DataRow)
				{
					val = ReflectionServices.ExtractValue((DataRow)source, propertyName);
				}
				else
				{
					if (source is ParameterCollection)
					{
						val = ReflectionServices.ExtractValue((ParameterCollection)source, propertyName);
					}
                    else if (source is SqlParameterCollection)
                    {
                        string paramName = propertyName;
                        if (!propertyName.StartsWith("@"))
                            paramName = "@" + propertyName;
                        val = ((SqlParameterCollection)source)[paramName];
                    }
                    else
                    {
                        if (source is IDataRecord)
                        {
                            val = ReflectionServices.ExtractValue((IDataRecord)source, propertyName);
                        }
                        else
                        {
                            if (source is IList)
                            {
                                val = ((IList)source)[int.Parse(propertyName)];
                            }
                            else
                            {
                                if (source is IDictionary)
                                {
                                    val = ((IDictionary)source)[propertyName];
                                }
                                else
                                {
                                    if (source is ControlCollection)
                                    {
                                        val = ((ControlCollection)source)[int.Parse(propertyName)];
                                    }
                                    else
                                    {
                                        if (source is WebPartCollection)
                                        {
                                            val = ((WebPartCollection)source)[propertyName];
                                        }
                                        else
                                        {
                                            if (source is DataRowCollection)
                                            {
                                                val = ((DataRowCollection)source)[int.Parse(propertyName)];
                                            }
                                            else
                                            {
                                                if (source is IHierarchyData)
                                                {
                                                    val = ExtractValue((IHierarchyData)source, propertyName);
                                                }
                                                else
                                                {
                                                    Type sourceType = source.GetType();
                                                    PropertyInfo sourcePropertyInfo = null;
                                                    try
                                                    {
                                                        sourcePropertyInfo = sourceType.GetProperty(propertyName);
                                                    }
                                                    catch (AmbiguousMatchException ex)
                                                    {
                                                        sourcePropertyInfo = sourceType.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                                                    }

                                                    bool isIndexer = false;
                                                    if (null == sourcePropertyInfo)
                                                    {
                                                        // try to find an indexer with one string param
                                                        foreach (PropertyInfo pi in sourceType.GetProperties())
                                                        {
                                                            ParameterInfo[] parami = pi.GetIndexParameters();
                                                            if (parami.Length == 1 && parami[0].ParameterType == typeof(string))
                                                            {
                                                                sourcePropertyInfo = pi;
                                                                isIndexer = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (null == sourcePropertyInfo)
                                                        throw new InvalidOperationException(string.Format("Could not find property '{0}' from '{1}'. Remaining property loop is '{2}'", propertyName, originalPropertyName, childProperty));

                                                    if (isIndexer)
                                                        val = sourcePropertyInfo.GetValue(source, new object[] { propertyName });
                                                    else
                                                        val = sourcePropertyInfo.GetValue(source, null);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
				}
			}
			object result;
			if (string.IsNullOrEmpty(childProperty))
			{
				result = val;
			}
			else
			{
				if (null == val)
				{
					throw new InvalidOperationException(string.Format("Found empty property '{0}' from '{1}'. Remaining property loop is '{2}'", propertyName, originalPropertyName, childProperty));
				}
				result = ReflectionServices.ExtractValue(val, childProperty, originalPropertyName);
			}
			return result;
		}
		public static object ExtractValue(DataRowView source, string columnName)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(columnName))
			{
				throw new ArgumentException("columnName is required");
			}
			return ReflectionServices.ExtractValue(source.Row, columnName);
		}
		public static object ExtractValue(DataRow source, string columnName)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(columnName))
			{
				throw new ArgumentException("columnName is required");
			}
			object result;
			if (source.Table.Columns.Contains(columnName))
			{
				result = source[columnName];
			}
			else
			{
				result = null;
			}
			return result;
		}
		public static object ExtractValue(ParameterCollection source, string paramName)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(paramName))
			{
				throw new ArgumentException("paramName is required");
			}
			return source[paramName].DefaultValue;
		}
		public static object ExtractValue(IDataRecord source, string columnName)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(columnName))
			{
				throw new ArgumentException("columnName is required");
			}
			int index = source.GetOrdinal(columnName);
			return source.GetValue(index);
		}
        public static object ExtractValue(IHierarchyData data, string propertyName)
        {
            if ("Type" == propertyName)
                return data.Type;
            else if ("Path" == propertyName)
                return data.Path;
            else
                return ExtractValue(data.Item, propertyName);
        }
		public static object StrongTypeValue(object value, string typeAssemblyQualifiedName)
		{
			if (string.IsNullOrEmpty(typeAssemblyQualifiedName))
			{
				throw new ArgumentNullException("typeAssemblyQualifiedName");
			}
			Type type = Type.GetType(typeAssemblyQualifiedName);
			object result;
			if (null != type)
			{
				result = ReflectionServices.StrongTypeValue(value, type);
			}
			else
			{
				result = value;
			}
			return result;
		}
		public static object StrongTypeValue(object value, Type type)
		{
			if (null == type)
			{
				throw new ArgumentNullException("type");
			}
			object result;
			if (null == value)
			{
				result = null;
			}
			else
			{
				if (type.IsEnum)
				{
					result = Enum.Parse(type, value.ToString());
				}
				else
				{
					if (type == typeof(FontUnit))
					{
						if (value is FontUnit)
						{
							result = value;
						}
						else
						{
							if (value is string && (string.IsNullOrEmpty(value.ToString()) || "NotSet" == value.ToString()))
							{
								result = default(FontUnit);
							}
							else
							{
								result = new FontUnit(value.ToString());
							}
						}
					}
					else
					{
						if (type == typeof(Color))
						{
							if (value is Color)
							{
								result = value;
							}
							else
							{
								if (value is string && string.IsNullOrEmpty(value.ToString()))
								{
									result = default(Color);
								}
								else
								{
									result = Color.FromName(value.ToString());
								}
							}
						}
						else
						{
							if (type == typeof(Unit))
							{
								result = Unit.Parse(value.ToString());
							}
							else
							{
								if (type.IsArray && value is string)
								{
									string[] values = value.ToString().Split(new char[]
									{
										','
									});
									for (int i = 0; i < values.Length; i++)
									{
										values[i] = values[i].Trim();
									}
									Array ret = Array.CreateInstance(type.GetElementType(), values.Length);
									values.CopyTo(ret, 0);
									result = ret;
								}
								else
								{
									if (value is IConvertible && (!type.IsGenericType || !type.Name.Contains("Nullable")))
									{
										if (value is DateTime && type == typeof(string))
										{
											result = ((DateTime)value).ToString();
										}
										else
										{
											result = Convert.ChangeType(value, type);
										}
									}
									else
									{
										result = value;
									}
								}
							}
						}
					}
				}
			}
			return result;
		}
		public static void SetValue(object target, string propertyName, object value)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target object");
			}
			if (string.IsNullOrEmpty(propertyName))
			{
				throw new ArgumentNullException("propertyName");
			}
			ReflectionServices.SetValue(target, propertyName, value, false);
		}
		public static bool SetValue(object target, string propertyName, object value, bool onlyIfExists)
		{
			if (null == target) throw new ArgumentNullException("target object");
			if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");

			Type targetType = target.GetType();
			string childProperty = null;
			if (propertyName.StartsWith("'"))
			{
				propertyName = propertyName.Substring(1);
				childProperty = propertyName.Substring(propertyName.IndexOf("'") + 1);
				if (childProperty.StartsWith("."))
					childProperty = childProperty.Substring(1);

                propertyName = propertyName.Substring(0, propertyName.IndexOf("'"));
			}
			else
			{
				if (propertyName.Contains("."))
				{
					childProperty = propertyName.Substring(propertyName.IndexOf(".") + 1);
					propertyName = propertyName.Substring(0, propertyName.IndexOf("."));
				}
			}

            if (string.IsNullOrEmpty(childProperty))
			{
				if (target is IDictionary<string, object>)
					return ReflectionServices.SetValue((IDictionary<string, object>)target, propertyName, value, onlyIfExists);

                if (target is IDictionary)
					((IDictionary)target)[propertyName] = value;
				else
				{
					if (target is DataRowView)
						return ReflectionServices.SetValue((DataRowView)target, propertyName, value, onlyIfExists);
					if (target is DataRow)
						return ReflectionServices.SetValue((DataRow)target, propertyName, value, onlyIfExists);
					if (target is ParameterCollection)
						return ReflectionServices.SetValue((ParameterCollection)target, propertyName, value, onlyIfExists);
					if (target is SqlParameterCollection)
						return ReflectionServices.SetValue((SqlParameterCollection)target, propertyName, value, onlyIfExists);
					if (target is SqlParameter && "Value" == propertyName)
						return ReflectionServices.SetValue((SqlParameter)target, value);

                    PropertyInfo targetPropertyInfo = targetType.GetProperty(propertyName);

                    bool isIndexer = false;
                    if (null == targetPropertyInfo)
                    {
                        // try to find an indexer with one string param
                        foreach (PropertyInfo pi in targetType.GetProperties())
                        {
                            ParameterInfo[] parami = pi.GetIndexParameters();
                            if (parami.Length == 1 && parami[0].ParameterType == typeof(string))
                            {
                                targetPropertyInfo = pi;
                                isIndexer = true;
                                break;
                            }
                        }
                    }

					if (null == targetPropertyInfo)
					{
                        if (onlyIfExists)
                            return false;

						throw new InvalidOperationException("Can't find property " + propertyName);
					}
					else
					{
                        if (!targetPropertyInfo.CanWrite)
                            return false;

						if (DBNull.Value == value)
							value = null;
						if (value is string && targetPropertyInfo.PropertyType != typeof(string) && string.IsNullOrEmpty((string)value))
							value = null;

                        object convertedValue = ReflectionServices.StrongTypeValue(value, targetPropertyInfo.PropertyType);
                        if (isIndexer)
                            targetPropertyInfo.SetValue(target, convertedValue, new object[] { propertyName });
                        else
                            targetPropertyInfo.SetValue(target, convertedValue, null);
					}
				}
			}
			else
				ReflectionServices.SetValue(ReflectionServices.ExtractValue(target, "'" + propertyName + "'"), childProperty, value);

            return true;
		}
		public static bool SetValue(IDictionary<string, object> target, string key, object value)
		{
			return ReflectionServices.SetValue(target, key, value, false);
		}
		public static bool SetValue(IDictionary<string, object> target, string key, object value, bool onlyIfExists)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			bool result;
			if (!target.ContainsKey(key))
			{
				if (!onlyIfExists)
				{
					throw new InvalidOperationException("Can't find key " + key);
				}
				result = false;
			}
			else
			{
				if (target.ContainsKey(key))
				{
					target[key] = value;
				}
				result = true;
			}
			return result;
		}
		public static void SetValue(DataRowView target, string columnName, object value)
		{
			ReflectionServices.SetValue(target, columnName, value, false);
		}
		public static bool SetValue(DataRowView target, string columnName, object value, bool onlyIfExists)
		{
			return ReflectionServices.SetValue(target.Row, columnName, value, onlyIfExists);
		}
		public static void SetValue(DataRow target, string columnName, object value)
		{
			ReflectionServices.SetValue(target, columnName, value, false);
		}
		public static bool SetValue(DataRow target, string columnName, object value, bool onlyIfExists)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			if (string.IsNullOrEmpty(columnName))
			{
				throw new ArgumentNullException("columnName");
			}
			bool result;
			if (!target.Table.Columns.Contains(columnName))
			{
				if (!onlyIfExists)
				{
					throw new InvalidOperationException("Can't find column " + columnName);
				}
				result = false;
			}
			else
			{
				if (target.Table.Columns.Contains(columnName))
				{
					target[columnName] = value;
				}
				result = true;
			}
			return result;
		}
		public static bool SetValue(ParameterCollection target, string paramName, object value, bool onlyIfExists)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			if (string.IsNullOrEmpty(paramName))
			{
				throw new ArgumentNullException("paramName");
			}
			Parameter param = target[paramName];
			if (null == param)
			{
				throw new InvalidOperationException(string.Format("Can't find parameter '{0}'", paramName));
			}
			param.DefaultValue = ((value != null) ? value.ToString() : null);
			return true;
		}
		public static bool SetValue(SqlParameterCollection target, string paramName, object value, bool onlyIfExists)
		{
			if (null == target)
			{
				throw new ArgumentNullException("target");
			}
			if (string.IsNullOrEmpty(paramName))
			{
				throw new ArgumentNullException("paramName");
			}
			if (!paramName.StartsWith("@"))
			{
				paramName = "@" + paramName;
			}
			SqlParameter param = target[paramName];
			return ReflectionServices.SetValue(param, value);
		}
		public static bool SetValue(SqlParameter param, object value)
		{
			if (null == param)
			{
				throw new ArgumentNullException("param");
			}
			if (null == value)
			{
				param.Value = DBNull.Value;
			}
			else
			{
				if (string.IsNullOrEmpty(value.ToString()))
				{
					param.Value = DBNull.Value;
				}
				else
				{
					param.Value = value;
				}
			}
			return true;
		}
		public static string[] SplitWithEscape(string input, char separator, char beginEscape, char endEscape)
		{
			List<string> output = new List<string>();
			StringBuilder segment = new StringBuilder();
			bool isEscaping = false;
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] == separator && !isEscaping)
				{
					output.Add(segment.ToString());
					segment = new StringBuilder();
				}
				else
				{
					if (input[i] == beginEscape && !isEscaping)
					{
						isEscaping = true;
					}
					else
					{
						if (input[i] == endEscape)
						{
							isEscaping = false;
						}
						else
						{
							segment.Append(input[i]);
						}
					}
				}
			}
			if (segment.Length > 0)
			{
				output.Add(segment.ToString());
			}
			return output.ToArray();
		}
		public static string ToString(object value)
		{
			string result;
			if (value is Color)
			{
				Color color = (Color)value;
				if (color.IsEmpty)
				{
					result = "";
				}
				else
				{
					result = color.Name;
				}
			}
			else
			{
				result = value.ToString();
			}
			return result;
		}

        public static string Version()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            return fvi.ProductVersion;
        }
	}
}
