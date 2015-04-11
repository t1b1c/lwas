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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

using LWAS.Extensible.Interfaces.Expressions;

using LWAS.Expressions.Extensions;
using LWAS.Database.Expressions;
using LWAS.Database.Expressions.Aggregate;

namespace LWAS.Database
{
    public class View
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Table Source { get; set; }
        public RelationsCollection Relationship { get; set; }
        public FiltersCollection Filters { get; set; }
        public FieldsCollection<TableField> Fields { get; set; }
        public FieldsCollection<ComputedField> ComputedFields { get; set; }
        public IEnumerable<Field> AllFields
        {
            get { return this.Fields.Cast<Field>().Union(this.ComputedFields.Cast<Field>()); }
        }
        public Dictionary<Field, string> Aliases { get; set; }
        public ParametersCollection OwnParameters { get; set; }
        public ParametersCollection Parameters
        {
            get
            {
                return new ParametersCollection(this.OwnParameters).Append(this.OrphanParameters);
            }
        }
        public IEnumerable<string> OrphanParameters
        {
            get
            {
                return this.FlattenSubviews().SelectMany(v => v.Parameters
                                                               .Where(p => !this.Subviews.ContainsKey(v) ||
                                                                           !this.Subviews[v].ContainsKey(p) ||
                                                                           this.Subviews[v][p] == null));
            }
        }
        public ParametersCollection UpdateParameters { get; set; }
        public ViewsManager Manager { get; set; }
        public ViewSorting Sorting { get; private set; }
        public Dictionary<View, Dictionary<string, Field>> Subviews { get; set; }

        static object SyncRoot = new object();

        public View(ViewsManager manager)
        {
            this.Manager = manager;
            this.Relationship = new RelationsCollection(this.Manager);
            this.Filters = new FiltersCollection(this.Manager);
            this.Fields = new FieldsCollection<TableField>(TableField.XML_KEY);
            this.ComputedFields = new FieldsCollection<ComputedField>(ComputedField.XML_KEY);
            this.ComputedFields.InitField += new EventHandler<FieldsCollection<ComputedField>.FieldEventArgs>(ComputedFields_InitField);
            this.Aliases = new Dictionary<Field, string>();
            this.OwnParameters = new ParametersCollection();
            this.UpdateParameters = new ParametersCollection();
            this.Sorting = new ViewSorting(this);
            this.Subviews = new Dictionary<View, Dictionary<string, Field>>();
        }

        void ComputedFields_InitField(object sender, FieldsCollection<ComputedField>.FieldEventArgs e)
        {
            e.Field.Manager = this.Manager;
        }

        public IEnumerable<Table> RelatedTables()
        {
            if (this.Relationship.Count == 0)
                return new Table[] { this.Source };

            return this.Relationship
                        .Select<Relation, Table>(rel => rel.MasterTable)
                        .Union(this.Relationship
                                    .Select<Relation, Table>(rel => rel.DetailsTable))
                        .Distinct();
        }

        public IEnumerable<Table> RelatableTables()
        {
            ViewsManager vm = this.Manager;

            if (this.Relationship.Count == 0)
                return new Table[] { this.Source };

            return vm.Tables
                     .Values
                     .Where(t =>
                     {
                         return vm.Relations
                                  .Select<Relation, Table>(rel => rel.MasterTable)
                                  .Union(vm.Relations
                                           .Select<Relation, Table>(rel => rel.DetailsTable))
                                  .Where(tb =>
                                  {
                                      return this.Relationship
                                                 .Any(r =>
                                                 {
                                                     return r.DetailsTable == tb ||
                                                            r.MasterTable == tb;
                                                 })
                                             ||
                                             vm.Relations
                                               .Any(r =>
                                               {
                                                   return ((r.MasterTable == tb && r.DetailsTable == this.Source) ||
                                                           (r.DetailsTable == tb && r.MasterTable == this.Source));
                                               });
                                  })
                                  .Contains(t);

                     });
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("view");
            writer.WriteAttributeString("name", this.Name);
            writer.WriteAttributeString("description", this.Description);

            writer.WriteStartElement(TableField.XML_KEY);
            foreach (TableField field in this.Fields)
            {
                writer.WriteStartElement("field");
                writer.WriteAttributeString("name", field.Name);
                writer.WriteAttributeString("table", field.Table.Name);
                if (this.Aliases.ContainsKey(field))
                    writer.WriteAttributeString("alias", this.Aliases[field]);
                writer.WriteEndElement();   // field
            }
            writer.WriteEndElement();   // fields

            this.ComputedFields.ToXml(writer); // computedFields

            writer.WriteStartElement("source");
            if (null != this.Source)
                writer.WriteAttributeString("name", this.Source.Name);
            writer.WriteEndElement();   // source

            writer.WriteStartElement("relationship");
            foreach (Relation relation in this.Relationship)
            {
                writer.WriteStartElement("relation");
                writer.WriteAttributeString("name", relation.Name);
                writer.WriteEndElement();   // relation
            }
            writer.WriteEndElement();   // relationship

            writer.WriteStartElement("filters");
            foreach (Filter filter in this.Filters.Values)
                filter.ToXml(writer);
            writer.WriteEndElement();   // filters

            this.OwnParameters.ToXml(writer);

            writer.WriteStartElement("subviews");
            foreach (View subview in this.Subviews.Keys)
            {
                writer.WriteStartElement("subview");
                writer.WriteAttributeString("name", subview.Name);
                foreach (string p in this.Subviews[subview].Keys)
                {
                    Field f = this.Subviews[subview][p];
                    writer.WriteStartElement("parameter");
                    writer.WriteAttributeString("name", p);
                    if (null != f)
                    {
                        if (this.Aliases.ContainsKey(f))
                            writer.WriteAttributeString("field", this.Aliases[f]);
                        else
                            writer.WriteAttributeString("field", f.Name);
                    }
                    writer.WriteEndElement();   // parameter
                }
                writer.WriteEndElement();   // subview
            }
            writer.WriteEndElement();   // subviews

            writer.WriteEndElement();   // view
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            this.Name = element.Attribute("name").Value;
            this.Description = element.Attribute("description").Value;

            foreach (XElement fieldElement in element.Element(TableField.XML_KEY).Elements("field"))
            {
                string tableName = fieldElement.Attribute("table").Value;
                string fieldName = fieldElement.Attribute("name").Value;
                if (!this.Manager.Tables.ContainsKey(tableName)) throw new ApplicationException(String.Format("Can't find table '{0}' to add it's field '{1}' to the view '{2}'", tableName, fieldName, this.Name));
                Table table = this.Manager.Tables[tableName];
                TableField field = table.Fields[fieldName];
                if (null == field) throw new ApplicationException(String.Format("Table '{0}' does not contain field '{1}' to add it to the view '{2}'", tableName, fieldName, this.Name));
                if (null != fieldElement.Attribute("alias"))
                    this.Aliases.Add(field, fieldElement.Attribute("alias").Value);
                this.Fields.Add(field);
            }

            XElement computedFieldsElement = element.Element(ComputedField.XML_KEY);
            if (null != computedFieldsElement)
                this.ComputedFields.FromXml(computedFieldsElement);

            if (null != element.Element("source") && null != element.Element("source").Attribute("name"))
            {
                string sourceName = element.Element("source").Attribute("name").Value;
                this.Source = this.Manager.Tables[sourceName];
            }

            foreach (XElement relationElement in element.Element("relationship").Elements("relation"))
            {
                string relationName = relationElement.Attribute("name").Value;
                Relation relation = this.Manager.Relations[relationName];
                if (null == relation) throw new ApplicationException(String.Format("Cannot find the relation '{0}' to add it to view '{1}'", relationName, this.Name));
                this.Relationship.Add(relation);
            }

            this.Filters.FromXml(element.Element("filters"));

            if (null != element.Element("parameters"))
                this.OwnParameters.FromXml(element.Element("parameters"));

            XElement subviewsElement = element.Element("subviews");
            if (null != subviewsElement)
            {
                foreach (XElement subviewElement in subviewsElement.Elements("subview"))
                {
                    if (!this.Manager.Views.ContainsKey(subviewElement.Attribute("name").Value))
                        throw new InvalidOperationException(String.Format("Subview not found. Failed to load view '{0}' at subview '{1}'", this.Name, subviewElement.Attribute("name").Value));
                    View subview = this.Manager.Views[subviewElement.Attribute("name").Value];
                    Dictionary<string, Field> subviewparams = new Dictionary<string, Field>();
                    foreach (XElement subviewparamElement in subviewElement.Elements("parameter"))
                    {
                        string p = subviewparamElement.Attribute("name").Value;
                        Field f = null;
                        if (null != subviewparamElement.Attribute("field"))
                        {
                            string fname = subviewparamElement.Attribute("field").Value;
                            if (!String.IsNullOrEmpty(fname))
                            {
                                f = this.AllFields.FirstOrDefault(fld => fld.Name == fname);
                                if (null == f)
                                    f = this.Aliases.FirstOrDefault(kvp => kvp.Value == fname).Key;
                            }
                        }
                        subviewparams.Add(p, f);
                    }
                    this.Subviews.Add(subview, subviewparams);
                }
            }
        }

        public void ToSql(StringBuilder builder)
        {
            ToSql(builder, false);
        }

        public void ToSql(StringBuilder builder, bool isFilterSubview)
        {
            string target = isFilterSubview ? "filter" : "select";
            string compiledSql = CompiledSqlFromCache(this.Name, target);

            if (String.IsNullOrEmpty(compiledSql))
            {
                CompileSql(builder, isFilterSubview);
                CompiledSqlToCache(this.Name, target, builder.ToString(), this.Manager.configFile);
            }
            else
                builder.Append(compiledSql);

            if (!this.Parameters.IsEmpty && !isFilterSubview)
            {
                // parameters values
                bool firstParameter = true;
                foreach (string parameter in this.Parameters)
                {
                    if (!String.IsNullOrEmpty(parameter)) // can be empty if this is a subview and this parameter is linked with a field of the superview
                    {
                        string identifier = this.Parameters.SqlIdentifier(parameter);

                        if (!firstParameter)
                            builder.Append(", ");
                        else
                            firstParameter = false;

                        string pval = CastToDbType(this.Parameters[parameter] ?? "", InferParamType(parameter), false);
                        if (pval == "null")
                            builder.AppendFormat("{0} = null", identifier);
                        else
                            builder.AppendFormat("{0} = '{1}'", identifier, pval);
                    }
                }
            }
        }

        static void CompiledSqlToCache(string viewName, string target, string sql, string dependency)
        {
            Cache cache = HttpContext.Current.Cache;
            lock (SyncRoot)
            {
                Dictionary<string, Dictionary<string, string>> compiledviews = cache["CompiledViews"] as Dictionary<string, Dictionary<string, string>>;
                if (null == compiledviews)
                    compiledviews = new Dictionary<string, Dictionary<string, string>>();
                if (!compiledviews.ContainsKey(viewName))
                    compiledviews.Add(viewName, new Dictionary<string, string>());
                if (!compiledviews[viewName].ContainsKey(target))
                    compiledviews[viewName].Add(target, sql);
                else
                    compiledviews[viewName][target] = sql;

                cache.Insert("CompiledViews", compiledviews, new CacheDependency(dependency));
            }
        }

        static string CompiledSqlFromCache(string viewName, string target)
        {
            Cache cache = HttpContext.Current.Cache;
            lock(SyncRoot)
            {
                Dictionary<string, Dictionary<string, string>> compiledviews = cache["CompiledViews"] as Dictionary<string, Dictionary<string, string>>;
                if (null != compiledviews && compiledviews.ContainsKey(viewName) && compiledviews[viewName].ContainsKey(target))
                    return compiledviews[viewName][target];

                return null;
            }
        }

        public void CompileSql(StringBuilder builder, bool isFilterSubview)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            SetUpTokens(!isFilterSubview);

            // cmd
            if (!isFilterSubview)
                builder.AppendLine("exec sp_executesql N'");
            builder.AppendLine("select");
            this.Fields.ToSql(builder, this.Aliases);
            if (this.ComputedFields.Count > 0)
                builder.AppendLine(",");
            this.ComputedFields.ToSql(builder, this.Aliases);
            builder.AppendLine();
            builder.AppendLine("from");
            builder.Append("    ");
            builder.AppendFormat("[{0}]", this.Source.Name);
            builder.AppendLine();
            if (this.Relationship.Count > 0)
                this.Relationship.ToSql(builder, this.Source);
            if (this.Filters.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("where");
                this.Filters.ToSql(builder);
            }
            for (int i = 0; i < this.Sorting.SortedFields.Count; i++)
            {
                FieldSorting sorting = this.Sorting.SortedFields[i];
                string op = null;
                if (sorting.Direction == SortingOptions.Up)
                    op = "asc";
                else if (sorting.Direction == SortingOptions.Down)
                    op = "desc";
                if (String.IsNullOrEmpty(op))
                    continue;

                if (i == 0)
                {
                    builder.AppendLine();
                    builder.Append("order by ");
                }
                else
                    builder.AppendLine(", ");
                builder.AppendFormat("[{0}].[{1}] {2}", sorting.Field.Table.Name, sorting.Field.Name, op);
            }
            if (!isFilterSubview)
                builder.Append("'");

            if (!this.Parameters.IsEmpty && !isFilterSubview)
            {

                builder.AppendLine(",");

                // parameters declaration
                builder.Append("N'");
                bool firstParameter = true;
                foreach (string parameter in this.Parameters)
                {
                    if (!String.IsNullOrEmpty(parameter)) // can be empty if this is a subview and this parameter is linked with a field of the superview
                    {
                        string identifier = this.Parameters.SqlIdentifier(parameter);

                        if (!firstParameter)
                            builder.Append(", ");
                        else
                            firstParameter = false;

                        builder.AppendFormat("{0} varchar(max)", identifier);
                    }
                }

                builder.AppendLine("',");
            }
        }

        public string InferParamType(string parameter)
        {
            var sub_views = FlattenSubviews().ToList();

            var parent_expressions = this.Filters
                                         .Union(sub_views.SelectMany<View, Filter>(sv => sv.Filters))
                                         .SelectMany<Filter, IExpression>(f =>
                                                {
                                                    return f.Expression.Flatten()
                                                                       .Where(e => null != e.Operands.OfType<ParameterToken>()
                                                                                                     .FirstOrDefault(pt => pt.ParameterName == parameter)
                                                                            );
                                                })
                                        .ToList();

            var parent_expressions_fields = parent_expressions.SelectMany(e =>
                                                                           e.Flatten()
                                                                            .SelectMany(ee => ee.Operands.OfType<FieldToken>())
                                                                    )
                                                              .ToList();


            var parent_expressions_corresponding_fields = parent_expressions_fields.Select(ft =>
                                                                                            this.AllFields
                                                                                                .Union(sub_views.SelectMany(sv => sv.AllFields))
                                                                                                .OfType<TableField>()
                                                                                                .FirstOrDefault(af => af.Name == ft.FieldName)
                                                                                        )
                                                                                   .ToList();
            
            var operand_field = parent_expressions_corresponding_fields.FirstOrDefault();

            if (null != operand_field)
                return operand_field.DBType;
            else
            {
                // this parameter could be used in a subview
                var dbtype = this.Subviews.Keys
                                 .Select(v =>
                                     {
                                         return v.InferParamType(parameter);
                                     })
                                 .Where(t => !String.IsNullOrEmpty(t))
                                 .FirstOrDefault();

                if (!String.IsNullOrEmpty(dbtype))
                    return dbtype;
                else
                    return "Text";
            }
        }

        public void ToUpdateSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");
            if (this.UpdateParameters.Count() == 0) throw new InvalidOperationException("No update parameters defined");

            SetUpTokens(true);

            // cmd
            builder.AppendLine("exec sp_executesql N'");
            builder.AppendFormat("update [{0}]", this.Source.Name);
            builder.AppendLine();
            builder.AppendLine("set");

            foreach (string updateParameter in this.UpdateParameters)
            {
                TableField field = this.Source.Fields[updateParameter];
                builder.AppendFormat("    [{0}].[{1}] = {2},", field.Table.Name, field.Name, CastToDbType(this.UpdateParameters[updateParameter], field.DBType, true));
                builder.AppendLine();
            }
            builder.Remove(builder.Length - 3, 3);

            builder.AppendLine();
            builder.AppendLine("from");
            builder.Append("    ");
            builder.AppendFormat("[{0}]", this.Source.Name);
            builder.AppendLine();
            if (this.Relationship.Count > 0)
                this.Relationship.ToSql(builder, this.Source);
            if (this.Filters.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("where");
                this.Filters.ToSql(builder);
            }
            builder.Append("'");

            if (!this.OwnParameters.IsEmpty)
            {

                builder.AppendLine(",");

                // parameters declaration
                builder.Append("N'");
                bool firstParameter = true;
                foreach (string parameter in this.OwnParameters)
                {
                    string identifier = this.OwnParameters.SqlIdentifier(parameter);

                    if (!firstParameter)
                        builder.Append(", ");
                    else
                        firstParameter = false;

                    builder.AppendFormat("{0} varchar(max)", identifier);
                }

                builder.AppendLine("',");

                // parameters values
                firstParameter = true;
                foreach (string parameter in this.OwnParameters)
                {
                    string identifier = this.OwnParameters.SqlIdentifier(parameter);

                    if (!firstParameter)
                        builder.Append(", ");
                    else
                        firstParameter = false;

                    builder.AppendFormat("{0} = '{1}'", identifier, this.OwnParameters[parameter] ?? "");
                }
            }
        }

        public static string CastToDbType(object val, string dbtype, bool includeSqlCast)
        {
            string strval = (val ?? "").ToString();
            strval = strval.Replace("'", ""); // avoid sql injection
            switch (dbtype)
            {
                case "Identifier":
                    if (includeSqlCast)
                        return String.Format("cast(''{0}'' as int)", strval);
                    else
                        return strval;
                case "Text":
                    if (includeSqlCast)
                        return String.Format("cast(''{0}'' as varchar(max))", strval);
                    else
                        return strval;
                case "Number":
                    decimal d;
                    decimal.TryParse(strval, out d);
                    if (includeSqlCast)
                        return String.Format("cast(''{0}'' as decimal(18,4))", d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else
                        return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
                case "Date":
                    if (String.IsNullOrEmpty(strval))
                        return "null";
                    else
                    {
                        DateTime dt = DateTime.Parse(strval);
                        if (includeSqlCast)
                            return String.Format("cast(''{0}'' as smalldatetime)", dt.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        else
                            return dt.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                case "Boolean":
                    if (includeSqlCast)
                        return String.Format("cast(''{0}'' as bit)", strval);
                    else
                        return strval;
            }
            return "''''";
        }

        public void SetUpTokens(bool clearReferences)
        {
            var parameters = this.Filters.SelectMany<Filter, ParameterToken>(f =>
                                            {
                                                return f.Expression.Flatten()
                                                                   .SelectMany(e => e.Operands.OfType<ParameterToken>());
                                            });

            foreach (ParameterToken token in parameters)
            {
                token.View = this;
                if (clearReferences)
                {
                    token.ReferenceField = null;
                    token.ReferenceFieldAlias = null;
                }
            }

            var used_parameters = this.ComputedFields.SelectMany(c => 
                                                        {
                                                            return c.Expression
                                                                    .Flatten()
                                                                    .SelectMany(e => e.Operands.OfType<ParameterToken>());
                                                        });

            foreach (ParameterToken used_token in used_parameters)
            {
                used_token.View = this;
            }

            var viewtokens = SubviewsTokens();

            foreach (ViewToken token in viewtokens)
                token.ViewsManager = this.Manager;

            foreach (View subview in this.Subviews.Keys)
            {
                var subviewparams = subview.Filters.SelectMany<Filter, ParameterToken>(f =>
                                                        {
                                                            return f.Expression.Flatten()
                                                                               .SelectMany(e => e.Operands.OfType<ParameterToken>());
                                                        });
                Dictionary<string, Field> subviewReferences = this.Subviews[subview];
                ParametersCollection allparams = this.Parameters;
                foreach (string p in subviewReferences.Keys)
                {
                    Field referenceField = subviewReferences[p];
                    ParameterToken subviewParamToken = subviewparams.FirstOrDefault(pt => pt.ParameterName == p);
                    if (null != subviewParamToken)
                    {
                        subviewParamToken.ReferenceField = referenceField;
                        if (null != referenceField && this.Aliases.ContainsKey(referenceField))
                            subviewParamToken.ReferenceFieldAlias = this.Aliases[referenceField];
                        if (allparams.Contains(subviewParamToken.ParameterName))
                            subviewParamToken.Identifier = allparams.SqlIdentifier(subviewParamToken.ParameterName);
                    }
                }
            }
        }

        public void CleanupParameters()
        {
            List<string> parameters = new List<string>(this.OwnParameters);
            foreach (string p in parameters)
                CleanupParameter(p);
        }

        public void CleanupParameter(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (!this.OwnParameters.Contains(name))
                return;
            if (!IsParameterUsed(name))
                this.OwnParameters.Remove(name);
        }

        public bool IsParameterUsed(string name)
        {
            return null != this.Filters.SelectMany<Filter, ParameterToken>(f =>
                                            {
                                                return f.Expression.Flatten()
                                                                   .SelectMany(e => e.Operands.OfType<ParameterToken>());
                                            })
                                       .FirstOrDefault(p => p.ParameterName == name);
        }

        public void SyncSubviews()
        {
            Dictionary<View, Dictionary<string, Field>> syncdSubviews = new Dictionary<View,Dictionary<string,Field>>();

            var viewtokens = SubviewsTokens();

            foreach (ViewToken vt in viewtokens)
            {
                if (String.IsNullOrEmpty(vt.ViewName))
                    continue;
                View subview = this.Manager.Views[vt.ViewName];
                Dictionary<string, Field> links = null;
                if (this.Subviews.ContainsKey(subview))
                    links = this.Subviews[subview];
                if (null == links)
                {
                    links = new Dictionary<string, Field>();
                    this.Subviews.Add(subview, links);
                }

                foreach (string p in subview.Parameters)
                    if (!links.ContainsKey(p))
                        links.Add(p, null);

                // remove orphaned params
                foreach (string p in links.Keys.ToArray())
                    if (!subview.Parameters.Contains(p))
                        links.Remove(p);
            }

            // remove unused subviews
            foreach (View sv in this.Subviews.Keys.ToArray())
                if (null == viewtokens.FirstOrDefault(vt => vt.ViewName == sv.Name))
                    this.Subviews.Remove(sv);
        }

        IEnumerable<ViewToken> SubviewsTokens()
        {
            // find the subviews used in computed fields
            var viewtokens = this.ComputedFields
                                .Select<ComputedField, IExpression>(cf => cf.Expression)
                                .SelectMany(ex => ex.Flatten())
                                .OfType<AggregateExpression>()
                                .Select<AggregateExpression, ViewToken>(ex => ex.ViewToken)
                                .Where(vt => vt != null)
                // some types of expression (i.e. exists) have ViewTokens operands
                                .Union(this.ComputedFields
                                            .Select<ComputedField, IExpression>(cf => cf.Expression)
                                            .SelectMany(ex => ex.Flatten())
                                            .SelectMany<IExpression, ViewToken>(e =>
                                            {
                                                return e.Operands
                                                        .OfType<ViewToken>();
                                            })
                                            .Where(vt => vt != null)
                                    );

            // find the subviews used in filters
            viewtokens = viewtokens.Union(this.Filters.Where<Filter>(f => f.Expression != null)
                                                    .Select(f => f.Expression)
                                                    .SelectMany(ex => ex.Flatten())
                                                    .OfType<AggregateExpression>()
                                                    .Select<AggregateExpression, ViewToken>(ex => ex.ViewToken)
                                                    .Where(vt => vt != null)
                                        )
                // some types of expression (i.e. exists) have ViewTokens operands
                                    .Union(this.Filters
                                               .Where<Filter>(f => f.Expression != null)
                                               .SelectMany(f => f.Expression.Flatten())
                                               .SelectMany<IExpression, ViewToken>(e =>
                                               {
                                                   return e.Operands
                                                           .OfType<ViewToken>();
                                               })
                                               .Where(vt => vt != null)
                                            )
                                    .ToList(); // need this to have results when viewtokens from computed fields is empty

            return viewtokens;
        }

        public IEnumerable<View> FlattenSubviews()
        {
            List<View> list = new List<View>();

            list.AddRange(this.Subviews.Keys);
            foreach (View subview in this.Subviews.Keys)
                list.AddRange(subview.FlattenSubviews());

            return list;
        }
    }
}
