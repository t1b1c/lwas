/*
 * Copyright 2006-2013 TIBIC SOLUTIONS
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
using System.Text;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Expressions;

namespace LWAS.Database.Expressions.Aggregate
{
    public abstract class AggregateExpression : DatabaseExpression
    {
        public ViewToken ViewToken 
        {
            get
            {
                return this.Operands.OfType<ViewToken>()
                                    .SingleOrDefault();
            }
            set
            {
                var token = this.Operands.OfType<ViewToken>()
                                         .SingleOrDefault();
                if (null == token)
                    this.Operands.Add(value);
                else
                {
                    this.Operands.Remove(token);
                    this.Operands.Add(value);
                }
            }
        }

        public override void ToSql(StringBuilder builder)
        {
            if (null != this.ViewToken)
            {
                this.ViewToken.Reset();
                this.ViewToken.BeginAggregation += new EventHandler<ViewToken.ViewTokenEventArgs>(ViewToken_BeginAggregation);
                this.ViewToken.EndAggregation += new EventHandler<ViewToken.ViewTokenEventArgs>(ViewToken_EndAggregation);
                this.ViewToken.ToSql(builder);
            }
        }

        void ViewToken_BeginAggregation(object sender, ViewToken.ViewTokenEventArgs e)
        {
            BeginAggregation(e.Builder);
        }

        void ViewToken_EndAggregation(object sender, ViewToken.ViewTokenEventArgs e)
        {
            EndAggregation(e.Builder);
        }

        public abstract void BeginAggregation(StringBuilder builder);
        public abstract void EndAggregation(StringBuilder builder);
    }
}
