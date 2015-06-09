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

namespace LWAS.Database.Expressions
{
    public class Function : DatabaseExpression
    {
        public override string Key
        {
            get { return "function"; }
        }

        public override void ToSql(StringBuilder builder)
        {
            var operandsList = this.Operands.ToList();
            var function = this.Operands.FirstOrDefault();
            if (null == function) throw new ArgumentException("The function expression requires a first operand for the sql function name");

            function.Evaluate();    // BasicToken has no value until Evaluate
            builder.AppendFormat("dbo.{0}(", function.Value);

            foreach(var param in this.Operands.Where(o => o != function))
            {
                param.ToSql(builder);

                if (operandsList.IndexOf(param) < operandsList.Count - 1)
                    builder.Append(", ");
            }

            builder.Append(")");
        }
    }
}
