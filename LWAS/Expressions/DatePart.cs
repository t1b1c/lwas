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
using System.Linq;
using System.Globalization;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Expressions;

using LWAS.Infrastructure;

namespace LWAS.Expressions
{
    public class DatePart : Expression
    {
        public override string Key
        {
            get { return "datePart"; }
        }

        public override IResult Evaluate()
        {
            IResult result = base.Evaluate();

            IToken first_token = this.Operands.First();
            if (null != first_token)
            {
                DatePartToken token = first_token as DatePartToken;
                if (null == token)
                {
                    result.Exceptions.Add(new InvalidOperationException("DatePart expects the first operand to be DatePartToken"));
                    result.Status = ResultStatus.Unsuccessful;
                    return result;
                }
                if (this.Operands.Count > 1)
                {
                    IToken second_token = this.Operands.ElementAt(1);
                    DateTime dt;
                    if (null != second_token.Value && DateTime.TryParse(second_token.Value.ToString(),
                                                                        out dt))
                    {
                        try
                        {
                            this.Value = ReflectionServices.ExtractValue(dt, token.DatePart.ToString());
                        }
                        catch (Exception e)
                        {
                            result.Exceptions.Add(e);
                            result.Status = ResultStatus.Unsuccessful;
                        }
                    }
                }
            }

            return result;
        }
    }
}
