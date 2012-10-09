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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Configuration;

using LWAS.Infrastructure.Configuration;

namespace LWAS.Expressions.Extensions
{
    public static class SqlExtensions
    {
        public static void ToSql(this IExpression expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression is Any)
                ToSql((Any)expression, builder);
            else if (expression is All)
                ToSql((All)expression, builder);
            else if (expression is NotContains)
                ToSql((NotContains)expression, builder);
            else if (expression is NotEquals)
                ToSql((NotEquals)expression, builder);
            else if (expression is NotExists)
                ToSql((NotExists)expression, builder);
            else if (expression is Contains)
                ToSql((Contains)expression, builder);
            else if (expression is Equals)
                ToSql((Equals)expression, builder);
            else if (expression is Exists)
                ToSql((Exists)expression, builder);
            else if (expression is Greater)
                ToSql((Greater)expression, builder);
            else if (expression is Less)
                ToSql((Less)expression, builder);
        }

        public static void ToSql(this IToken token, StringBuilder builder)
        {
            if (token is IExpression)
                ToSql((IExpression)token, builder);
            if (token is IBasicToken)
            {
                IBasicToken basicToken = token as IBasicToken;
                basicToken.Evaluate();
                if (basicToken.Value != null)
                    builder.AppendFormat("'{0}'", basicToken.Value.ToString());
            }
            else
            {
                    System.Reflection.MethodInfo concreteMethod = token.GetType().GetMethod("ToSql", new Type[] { typeof(StringBuilder) });
                    if (null != concreteMethod)
                        concreteMethod.Invoke(token, new object[] { builder });
            }
        }

        public static void ToSql(Any expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                foreach (IToken operand in expression.Operands)
                {
                    builder.Append("(");
                    operand.ToSql(builder);
                    builder.AppendLine(")");
                    builder.Append("    or ");
                }
                builder.Remove(builder.Length - "    or ".Length - 2, "    or ".Length + 2);
            }
        }

        public static void ToSql(All expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                foreach (IToken operand in expression.Operands)
                {
                    builder.Append("(");
                    operand.ToSql(builder);
                    builder.AppendLine(")");
                    builder.Append("    and ");
                }
                builder.Remove(builder.Length - "    and ".Length - 2, "    and ".Length + 2);
            }
        }

        public static void ToSql(Contains expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    builder.Append("CONTAINS(");
                    operand1.ToSql(builder);
                    builder.Append(", ");
                    operand2.ToSql(builder);
                    builder.Append(")");
                }
            }
        }

        public static void ToSql(Equals expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    operand1.ToSql(builder);
                    builder.Append(" = ");
                    operand2.ToSql(builder);
                }
            }
        }

        public static void ToSql(Exists expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand = expression.Operands.First();
                operand.ToSql(builder);
                builder.Append(" IS NOT NULL");
            }
        }

        public static void ToSql(NotContains expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    builder.Append("NOT CONTAINS(");
                    operand1.ToSql(builder);
                    builder.Append(", ");
                    operand2.ToSql(builder);
                    builder.Append(")");
                }
            }
        }

        public static void ToSql(NotEquals expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    operand1.ToSql(builder);
                    builder.Append(" <> ");
                    operand2.ToSql(builder);
                }
            }
        }

        public static void ToSql(NotExists expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand = expression.Operands.First();
                operand.ToSql(builder);
                builder.Append(" IS NULL");
            }
        }

        public static void ToSql(Greater expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    operand1.ToSql(builder);
                    builder.Append(" >= ");
                    operand2.ToSql(builder);
                }
            }
        }

        public static void ToSql(Less expression, StringBuilder builder)
        {
            if (null == expression) throw new ArgumentNullException("expression");
            if (null == builder) throw new ArgumentNullException("builder");

            if (expression.Operands.Count > 0)
            {
                IToken operand1 = expression.Operands
                                            .First();
                IToken operand2 = expression.Operands
                                            .ElementAtOrDefault(1);
                if (null != operand1 && null != operand2)
                {
                    operand1.ToSql(builder);
                    builder.Append(" < ");
                    operand2.ToSql(builder);
                }
            }
        }
    }
}
