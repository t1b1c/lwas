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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Exceptions;
using LWAS.Infrastructure;

namespace LWAS.Database
{
    public class ViewToken : FieldToken
    {
        public class ViewTokenEventArgs : EventArgs
        {
            public StringBuilder Builder;
        }

        public override string Key
        {
            get { return "view as token"; }
        }

        public ViewsManager ViewsManager { get; set; }
        public string ViewName { get; set; }
        public event EventHandler<ViewTokenEventArgs> BeginAggregation;
        public event EventHandler<ViewTokenEventArgs> EndAggregation;
        View view = null;
        Field field = null;

        public ViewToken()
        { }

        public ViewToken(ViewsManager manager)
        {
            if (null == manager) throw new ArgumentNullException("manager");

            this.ViewsManager = manager;
        }

        protected override void Make(IConfigurationElement sourceElement, IExpressionsManager manager)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");
            if (null == manager) throw new ArgumentNullException("manager");

            if (!sourceElement.Attributes.ContainsKey("view")) throw new ConfigurationException("Bad view as token configuration: 'source' element has no 'view' attribute");
            this.ViewName = sourceElement.GetAttributeReference("view").Value.ToString();

            base.Make(sourceElement, manager);
        }

        protected override void WriteConfiguration(IConfigurationElement sourceElement)
        {
            if (null == sourceElement) throw new ArgumentNullException("sourceElement");

            sourceElement.AddAttribute("view").Value = this.ViewName;

            base.WriteConfiguration(sourceElement);
        }

        public void Setup()
        {
            if (null == this.ViewsManager) throw new InvalidOperationException("Failed to evaluate view as token. ViewsManager not set.");

            view = this.ViewsManager.Views[this.ViewName];
            field = view.Fields[this.FieldName];
            if (null == field)
                field = view.ComputedFields[this.FieldName];
            if (null == field)
                field = view.Aliases
                            .Keys
                            .FirstOrDefault(tf => tf.Name == this.FieldName);

        }

        public override void ToSql(StringBuilder builder)
        {
            ToSql(builder, false);
        }

        // generate a subquery select of one field - the tokenizer
        public void ToSql(StringBuilder builder, bool ignoreAlias)
        {
            if (null == builder) throw new ArgumentNullException("builder");

            Setup();
            if (null == view)
                return;

            view.SetUpTokens(false);

            builder.AppendLine("(select");
            
            if (null != this.BeginAggregation)
                BeginAggregation(this, new ViewTokenEventArgs() { Builder = builder });
            string alias = view.Aliases.ContainsKey(field) ? view.Aliases[field] : null;
            if (ignoreAlias)
                field.ToSql(builder);
            else
                field.ToSql(builder, alias);
            if (null != this.EndAggregation)
                EndAggregation(this, new ViewTokenEventArgs() { Builder = builder });
            builder.AppendLine();
            builder.AppendLine("from");
            builder.Append("    ");
            builder.AppendFormat("[{0}]", view.Source.Name);
            builder.AppendLine();
            if (view.Relationship.Count > 0)
                view.Relationship.ToSql(builder, view.Source);
            if (view.Filters.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("where");
                view.Filters.ToSql(builder);
            }

            builder.Append(")");
        }

        public void Reset()
        {
            this.BeginAggregation = null;
            this.EndAggregation = null;
        }
    }
}
