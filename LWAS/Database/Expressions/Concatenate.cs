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
using System.Text;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Expressions;
using LWAS.Expressions.Extensions;
using LWAS.Extensible.Interfaces;

namespace LWAS.Database.Expressions
{
    public class Concatenate : DatabaseExpression
    {
        public override string Key
        {
            get { return "concatenate"; }
        }

        public override IResult Evaluate()
        {
            IResult result = base.Evaluate();

            this.Value = String.Empty;
            foreach (IToken operand in this.Operands)
            {
                this.Value = (string)this.Value + operand.Value ?? "";
            }

            result.Status = ResultStatus.Successful;
            return result;
        }

        public override void ToSql(StringBuilder builder)
        {
            bool first = true;
            foreach (IToken token in this.Operands)
            {
                if (!first)
                    builder.Append(" + ");

                if (token is IBasicToken)
                    builder.Append(Cast((IBasicToken)token));
                else if (token is IExpression)
                {
                    builder.Append("cast( (");
                    token.ToSql(builder);
                    builder.Append(") as nvarchar(max) )");

                }
                else
                {
                    builder.Append("cast(");
                    token.ToSql(builder);
                    builder.Append(" as nvarchar(max) )");
                }

                first = false; 
            }
        }

        string Cast(IBasicToken token)
        {
            if (token.Source != null && !String.IsNullOrEmpty(token.ToString()))
                return "''" + token.Source.ToString() + "''";
            return "''''";
        }
    }
}
