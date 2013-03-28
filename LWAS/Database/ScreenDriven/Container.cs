/*
 * Copyright 2006-2012 TIBIC SOLUTIONS
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
using System.Xml;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.WebParts.Templating;

namespace LWAS.Database.ScreenDriven
{
    public class Container : IEnumerable<Field>
    {
        Dictionary<string, Field> fields = new Dictionary<string, Field>();

        IConfiguration _configuration;
        public IConfiguration Configuration
        {
            get { return _configuration; }
        }

        Screen _screen;
        public Screen Screen
        {
            get { return _screen; }
        }

        string _name;
        public string Name
        {
            get { return _name; }
        }

        string _alias;
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        XmlNode extra = null;

        public Field this[string field]
        {
            get { return fields[field]; }
        }

        public Container(IConfiguration configuration, Screen screen, string name)
        {
            if (null == configuration) throw new ArgumentNullException("configuration");
            if (null == screen) throw new ArgumentNullException("screen");
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            _configuration = configuration;
            _screen = screen;
            _name = name;

            LoadFields();
        }

        public Container(IConfiguration configuration, Screen screen, string name, XmlNode anExtra)
            : this(configuration, screen, name)
        {
            extra = anExtra;

            LoadFields();

            if (null != extra.Attributes["alias"])
                _alias = extra.Attributes["alias"].Value;
        }

        protected virtual void LoadFields()
        {
            fields.Clear();

            if (_configuration.Sections.ContainsKey("Template"))
            {
                foreach(IConfigurationElement chapter in _configuration.GetConfigurationSectionReference("Template").Elements.Values)
                {
                    if ("selectors" == chapter.ConfigKey ||
                        "commanders" == chapter.ConfigKey ||
                        "filter" == chapter.ConfigKey ||
                        "header" == chapter.ConfigKey ||
                        "footer" == chapter.ConfigKey ||
                        "grouping" == chapter.ConfigKey ||
                        "totals" == chapter.ConfigKey)
                        continue;

                    foreach (IConfigurationElement fieldElement in chapter.Elements.Values)
                    {
                        string fieldType = null;

                        if (fieldElement.Attributes.ContainsKey("type"))
                            fieldType = fieldElement.GetAttributeReference("type").Value.ToString();

                        foreach (IConfigurationElement propertyElement in fieldElement.Elements.Values)
                        {
                            string type = null;
                            string name = null;
                            string member = null;
                            string linkedScreen = null;
                            string linkedContainer = null;
                            string linkedField = null;
                            bool isReference = false;
                            bool isExcluded = false;
                            bool isUnique = false;
                            XmlNode fieldExtra = null;

                            if (propertyElement.Attributes.ContainsKey("member"))
                                member = propertyElement.GetAttributeReference("member").Value.ToString();
                            if (propertyElement.Attributes.ContainsKey("push"))
                                name = propertyElement.GetAttributeReference("push").Value.ToString();
                            else if (propertyElement.Attributes.ContainsKey("pull"))
                                name = propertyElement.GetAttributeReference("pull").Value.ToString();

                            if ((fieldType == "Date" || fieldType == "Number") && member == "Text")
                                continue;

                            if (!String.IsNullOrEmpty(name) && name.StartsWith("ID"))
                                type = "Identifier";
                            else if (!String.IsNullOrEmpty(fieldType))
                            {
                                ControlFactory.ControlDescriptor knownType = null;
                                if (ControlFactory.Instance.KnownTypes.ContainsKey(fieldType))
                                    knownType = ControlFactory.Instance.KnownTypes[fieldType];
                                if (null != knownType && knownType.EditableProperties.ContainsKey(member))
                                    type = knownType.EditableProperties[member].ValueType;
                            }

                            if (null != extra && null != name && !String.IsNullOrEmpty(name.Trim()))
                                fieldExtra = extra.SelectSingleNode(name);

                            if (null != fieldExtra)
                            {
                                if (null != fieldExtra.Attributes["linkedScreen"])
                                    linkedScreen = fieldExtra.Attributes["linkedScreen"].Value;
                                if (null != fieldExtra.Attributes["linkedContainer"])
                                    linkedContainer = fieldExtra.Attributes["linkedContainer"].Value;
                                if (null != fieldExtra.Attributes["linkedField"])
                                    linkedField = fieldExtra.Attributes["linkedField"].Value;
                                if (null != fieldExtra.Attributes["isReference"])
                                    bool.TryParse(fieldExtra.Attributes["isReference"].Value, out isReference);
                                if (null != fieldExtra.Attributes["isExcluded"])
                                    bool.TryParse(fieldExtra.Attributes["isExcluded"].Value, out isExcluded);
                                if (null != fieldExtra.Attributes["isUnique"])
                                    bool.TryParse(fieldExtra.Attributes["isUnique"].Value, out isUnique);
                                if (null != fieldExtra.Attributes["Type"])
                                    type = fieldExtra.Attributes["Type"].Value;
                            }

                            if (!String.IsNullOrEmpty(name) && !fields.ContainsKey(name))
                                fields.Add(name, new Field(name, type, fieldType, member, linkedScreen, linkedContainer, linkedField, isReference, isExcluded, isUnique));
                        }
                    }
                }
            }
        }

        public void Save()
        {
            if (null == extra.Attributes["alias"])
                extra.Attributes.Append(extra.OwnerDocument.CreateAttribute("alias"));
            extra.Attributes["alias"].Value = _alias;
        }

        public void Save(Field field)
        {
            XmlNode fieldExtra = extra.SelectSingleNode(field.Name);
            if (null == fieldExtra)
            {
                fieldExtra = extra.OwnerDocument.CreateNode(XmlNodeType.Element, field.Name, null);
                extra.AppendChild(fieldExtra);
            }

            if (null == fieldExtra.Attributes["linkedScreen"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("linkedScreen"));
            if (null == fieldExtra.Attributes["linkedContainer"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("linkedContainer"));
            if (null == fieldExtra.Attributes["linkedField"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("linkedField"));
            if (null == fieldExtra.Attributes["isReference"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("isReference"));
            if (null == fieldExtra.Attributes["isExcluded"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("isExcluded"));
            if (null == fieldExtra.Attributes["isUnique"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("isUnique"));
            if (null == fieldExtra.Attributes["Type"])
                fieldExtra.Attributes.Append(fieldExtra.OwnerDocument.CreateAttribute("Type"));

            fieldExtra.Attributes["linkedScreen"].Value = field.LinkedScreen;
            fieldExtra.Attributes["linkedContainer"].Value = field.LinkedContainer;
            fieldExtra.Attributes["linkedField"].Value = field.LinkedField;
            fieldExtra.Attributes["isReference"].Value = field.IsReference.ToString();
            fieldExtra.Attributes["isExcluded"].Value = field.IsExcluded.ToString();
            fieldExtra.Attributes["isUnique"].Value = field.IsUnique.ToString();
            fieldExtra.Attributes["Type"].Value = field.Type;
        }

        public string FindLinkedIdentifier(Container linkedContainer)
        {
            if (null == linkedContainer) throw new ArgumentNullException("linkedContainer");
            if (!this.Screen.ListLinkedContainers(this).Contains(linkedContainer)) throw new ArgumentException("Container not linked");

            foreach (Field field in linkedContainer)
                if (!field.IsExcluded &&
                        ((!String.IsNullOrEmpty(_alias) && _alias == field.LinkedScreen) ||
                        (_screen.Key == field.LinkedScreen && _name == field.LinkedContainer)) &&
                    "ID" == field.LinkedField)
                    return field.Name;

            return null;
        }

        #region IEnumerable<Field> Members

        public IEnumerator<Field> GetEnumerator()
        {
            foreach (Field field in fields.Values)
                yield return field;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public string ToSql(int flag)
        {
            string sql = null;
            string sql_table, sql_insert, sql_update, sql_delete, sql_load, sql_loadBy, sql_search;
            string sql_insert_cols = "";
            string sql_insert_vals = "";
            string sql_update_sets = "";
            string tableName = "";
            string rn = "\r\n";
            Field externalKeyField = null;
            string sql_ref_fields = "";
            string sql_joins = "";
            string sql_search_criteria = "";
            string sql_search_criteria_refs = "";
            string sql_search_prep = "";
            string sql_check_unique = "";
            string sql_check_linked = "";

            if (!String.IsNullOrEmpty(_alias))
                tableName = _alias;
            else
                tableName = _screen.Key + "_" + _name;

            if (fields.Count > 0)
            {
                sql_table = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "') AND type in (N'U'))";
                sql_table += rn + "drop table dbo." + tableName;
                sql_table += rn + "GO";
                sql_table += rn + "create table dbo." + tableName;
                sql_table += rn + " (";
                sql_table += rn + "    " + "ID int";
                if (1 == flag)
                    sql_table += " identity(1,1)";
                sql_table += " not null";

                foreach (Container container in this.Screen.ListLinkedContainers(this))
                {
                    string linkedIdentifier = FindLinkedIdentifier(container);
                    if (!String.IsNullOrEmpty(linkedIdentifier))
                    {
                        string containerTableName = container.Alias;
                        if (String.IsNullOrEmpty(containerTableName))
                            containerTableName = container.Screen.Key + "_" + container.Name;

                        sql_check_linked += String.IsNullOrEmpty(sql_check_linked) ? "   " : rn + "   else ";
                        sql_check_linked += "if exists (select * from " + containerTableName + " where " + linkedIdentifier + " = @ID)" + rn;
                        sql_check_linked += "        raiserror('" + tableName + "_" + containerTableName + "', 16, 1)";   
                    }
                }

                sql_insert = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "_insert" + "') AND type in (N'P', N'PC'))";
                sql_insert += rn + "drop procedure dbo." + tableName + "_insert";
                sql_insert += rn + "GO";
                sql_insert += rn + "create procedure dbo." + tableName + "_insert";
                sql_insert += rn + " (";
                sql_insert += rn + "    @ID int";
                if (1 == flag)
                    sql_insert += " output";

                sql_update = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "_update" + "') AND type in (N'P', N'PC'))";
                sql_update += rn + "drop procedure dbo." + tableName + "_update";
                sql_update += rn + "GO";
                sql_update += rn + "create procedure dbo." + tableName + "_update";
                sql_update += rn + " (";
                sql_update += rn + "    @ID int";

                sql_delete = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "_delete" + "') AND type in (N'P', N'PC'))";
                sql_delete += rn + "drop procedure dbo." + tableName + "_delete";
                sql_delete += rn + "GO";
                sql_delete += rn + "create procedure dbo." + tableName + "_delete";
                sql_delete += rn + " (";
                sql_delete += rn + "    @ID int";
                sql_delete += rn + " )";
                sql_delete += rn + " AS";

                sql_load = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "_load" + "') AND type in (N'P', N'PC'))";
                sql_load += rn + "drop procedure dbo." + tableName + "_load";
                sql_load += rn + "GO";
                sql_load += rn + "create procedure dbo." + tableName + "_load";
                sql_load += rn + " (";
                sql_load += rn + "    @ID int";
                sql_load += rn + " )";
                sql_load += rn + " AS";

                sql_search = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + tableName + "_search" + "') AND type in (N'P', N'PC'))";
                sql_search += rn + "drop procedure dbo." + tableName + "_search";
                sql_search += rn + "GO";
                sql_search += rn + "create procedure dbo." + tableName + "_search";
                sql_search += rn + " (";
                sql_search += rn + "    @ID int = NULL";

                foreach (Field field in this)
                {
                    string outerTableName = "";
                    if (null == externalKeyField && field.IsReference)
                        externalKeyField = field;
                    if (!String.IsNullOrEmpty(field.LinkedScreen) && !String.IsNullOrEmpty(field.LinkedField))
                    {
                        if (String.IsNullOrEmpty(field.LinkedContainer))
                            outerTableName = field.LinkedScreen;
                        else
                            outerTableName = field.LinkedScreen + "_" + field.LinkedContainer;

                        if (field.Type != "Identifier" && !field.LinkedField.Contains("."))
                        {
                            sql_ref_fields += "," + rn + "          ";
                            sql_ref_fields += outerTableName + "." + field.LinkedField + " AS " + field.Name;
                        }
						else if (field.LinkedField.Contains("."))
                        {
                            sql_ref_fields += "," + rn + "          ";
                            sql_ref_fields += field.LinkedScreen + "." + field.Name;
                        }

                        if (!field.IsExcluded)
                        {
                            sql_joins += rn + " LEFT JOIN ";
                            sql_joins += outerTableName + " ON " + tableName + "." + field.Name + "=" + outerTableName + "." + field.LinkedField;
                        }
                        else if (field.LinkedField.Contains("."))
                        {
                            string thirdTableName = field.LinkedField.Substring(0, field.LinkedField.IndexOf("."));
                            sql_joins += rn + " LEFT JOIN ";
                            sql_joins += thirdTableName + " ON " + outerTableName + "." + field.Name + "=" + field.LinkedField;
                        }
                    }

                    if (field.IsExcluded && (field.Type == "Identifier" || !field.LinkedField.Contains(".")))
                    {
                        sql_search += ",";
                        sql_search += rn + "    @" + field.ToSql();

                        if ("Text" == field.Type)
                        {
                            sql_search_prep += String.IsNullOrEmpty(sql_search_prep) ? "" : rn;
                            sql_search_prep += "    IF @" + field.Name + " = '' SET @" + field.Name + " = NULL";
                        }

                        sql_search_criteria_refs += rn + "    OR ";
                        sql_search_criteria_refs += outerTableName;
                        if (field.Type != "Identifier")
                            sql_search_criteria_refs += "." + field.LinkedField;
                        else if (field.Type == "Identifier")
                            sql_search_criteria_refs += "." + field.Name;

                        if ("Text" == field.Type)
                            sql_search_criteria_refs += " LIKE '%' + ";
                        else
                            sql_search_criteria_refs += " = ";
                        sql_search_criteria_refs += "@" + field.Name;
                        if ("Text" == field.Type)
                            sql_search_criteria_refs += " + '%'";

                        continue;
                    }
                    else if (field.IsUnique)
                    {
                        sql_check_unique += String.IsNullOrEmpty(sql_check_unique) ? "   " : rn + "   else ";
                        sql_check_unique += "if exists (select * from " + tableName + " where " + field.Name + " = @" + field.Name + "{0})" + rn;
                        sql_check_unique += "       raiserror('" + _screen.Application + "_" + _screen.Key + "_" + _name + "_" + field.Name + "_Unique', 16, 1)";
                    }

                    if (field.Name.ToUpper() == "ID")
                    {
                        if (2 == flag)
                        {
                            sql_insert_cols += String.IsNullOrEmpty(sql_insert_cols) ? "" : ",";
                            sql_insert_cols += rn + "    " + field.Name;

                            sql_insert_vals += String.IsNullOrEmpty(sql_insert_vals) ? "" : ",";
                            sql_insert_vals += rn + "    @" + field.Name;
                        }
                        continue;
                    }

                    if (!String.IsNullOrEmpty(field.LinkedField) && field.LinkedField.Contains("."))
                        continue;

                    sql_table += ",";
                    sql_table += rn + "    " + field.ToSql();

                    sql_insert += ",";
                    sql_insert += rn + "    @" + field.ToSql();

                    sql_update += ",";
                    sql_update += rn + "    @" + field.ToSql();

                    sql_search += ",";
                    sql_search += rn + "    @" + field.ToSql();

                    sql_insert_cols += String.IsNullOrEmpty(sql_insert_cols) ? "" : ",";
                    sql_insert_cols += rn + "    " + field.Name;

                    sql_insert_vals += String.IsNullOrEmpty(sql_insert_vals) ? "" : ",";
                    sql_insert_vals += rn + "    @" + field.Name;

                    sql_update_sets += String.IsNullOrEmpty(sql_update_sets) ? "" : ",";
                    sql_update_sets += rn + "    " + field.Name + " = @" + field.Name;

                    if ("Text" == field.Type)
                    {
                        sql_search_prep += String.IsNullOrEmpty(sql_search_prep) ? "" : rn;
                        sql_search_prep += "    IF @" + field.Name + " = '' SET @" + field.Name + " = NULL";
                    }

                    sql_search_criteria += rn + "    OR ";
                    if (!String.IsNullOrEmpty(outerTableName))
                        sql_search_criteria += outerTableName + "." + field.LinkedField;
                    else
                        sql_search_criteria += tableName + "." + field.Name;
                    if ("Text" == field.Type)
                        sql_search_criteria += " LIKE '%' + ";
                    else
                        sql_search_criteria += " = ";
                    sql_search_criteria += "@" + field.Name;
                    if ("Text" == field.Type)
                        sql_search_criteria += " + '%'";
                }

                sql_load += rn + "    SELECT " + tableName + ".*";
                sql_load += sql_ref_fields;
                sql_load += rn + " FROM " + tableName;
                sql_load += sql_joins;
                sql_load += rn + " WHERE " + tableName + ".ID=@ID";

                sql_loadBy = "";
                if (null != externalKeyField)
                {
                    string whereField = null;
                    if (externalKeyField.LinkedField.Contains("."))
                        whereField = externalKeyField.LinkedField;
                    string sql_ref_name = tableName + "_loadBy" + externalKeyField.Name;
                    sql_loadBy = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo." + sql_ref_name + "') AND type in (N'P', N'PC'))";
                    sql_loadBy += rn + "drop procedure dbo." + sql_ref_name;
                    sql_loadBy += rn + "GO";
                    sql_loadBy += rn + "create procedure dbo." + sql_ref_name;
                    sql_loadBy += rn + " (";
                    sql_loadBy += rn + "    @" + externalKeyField.ToSql();
                    sql_loadBy += rn + " )";
                    sql_loadBy += rn + " AS";
                    sql_loadBy += rn + "    SELECT " + tableName + ".*";
                    sql_loadBy += sql_ref_fields;
                    sql_loadBy += rn + " FROM " + tableName;
                    sql_loadBy += sql_joins;
                    sql_loadBy += rn + " WHERE " + (whereField ?? tableName + "." + externalKeyField.Name) + "=@" + externalKeyField.Name;
                }

                sql_table += rn + "constraint PK_" + tableName + " PRIMARY KEY CLUSTERED"; 
                sql_table += rn + "    (";
	            sql_table += rn + "    [ID] ASC";
                sql_table += rn + "    )";
                sql_table += rn + ")";

                sql_insert += rn + ")";
                sql_insert += rn + " AS";
                if (!String.IsNullOrEmpty(sql_check_unique))
                {
                    sql_insert += rn + String.Format(sql_check_unique, "");
                    sql_insert += rn + "    else";
                    sql_insert += rn + "    begin";
                }
                sql_insert += rn + "    INSERT INTO " + tableName;
                sql_insert += rn + "(";
                sql_insert += sql_insert_cols;
                sql_insert += rn + ")";
                sql_insert += rn + " VALUES";
                sql_insert += rn + "(";
                sql_insert += sql_insert_vals;
                sql_insert += rn + ")";
                if (1 == flag)
                    sql_insert += rn + "SET @ID = SCOPE_IDENTITY()";
                if (!String.IsNullOrEmpty(sql_check_unique))
                    sql_insert += rn + "    end";

                sql_update += rn + ")";
                sql_update += rn + " AS";
                if (!String.IsNullOrEmpty(sql_check_unique))
                {
                    sql_update += rn + String.Format(sql_check_unique, " and ID <> @ID");
                    sql_update += rn + "    else";
                    sql_update += rn + "    begin";
                }
                sql_update += rn + "    UPDATE " + tableName + " SET";
                sql_update += sql_update_sets;
                sql_update += rn + "    WHERE ID = @ID";
                if (!String.IsNullOrEmpty(sql_check_unique))
                    sql_update += rn + "    end";

                if (!String.IsNullOrEmpty(sql_check_linked))
                {
                    sql_delete += rn + sql_check_linked;
                    sql_delete += rn + "    else";
                    sql_delete += rn + "    begin";
                }
                sql_delete += rn + "    DELETE FROM " + tableName + " WHERE ID=@ID";
                if (!String.IsNullOrEmpty(sql_check_linked))
                    sql_delete += rn + "    end";

                sql_search += rn + ")";
                sql_search += rn + " AS";
                sql_search += rn + sql_search_prep + rn;
                sql_search += rn + " SELECT " + tableName + ".*";
                sql_search += sql_ref_fields;
                sql_search += rn + " FROM " + tableName;
                sql_search += sql_joins;
                sql_search += rn + " WHERE 1=0";
                sql_search += sql_search_criteria;
                sql_search += sql_search_criteria_refs;

                if (flag == 5)
                {
                    sql = sql_insert + rn + "GO";
                    sql += rn + sql_update + rn + "GO";
                    sql += rn + sql_delete + rn + "GO";
                    sql += rn + sql_load + rn + "GO";
                    sql += rn + sql_search + rn + "GO";
                }
                else
                {
                    if (flag != 3 && flag != 4)
                    {
                        sql = sql_table + rn + "GO";
                        sql += rn + sql_insert + rn + "GO";
                        sql += rn + sql_update + rn + "GO";
                        sql += rn + sql_delete + rn + "GO";
                        sql += rn + sql_load + rn + "GO";
                    }
                    if (flag != 4)
                        sql += rn + sql_loadBy + rn + "GO";

                    if (flag != 2 && flag != 3)
                        sql += rn + sql_search + rn + "GO";
                }
            }

            return sql;
        }
    }
}
