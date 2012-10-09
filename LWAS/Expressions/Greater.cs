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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Expressions
{
	public class Greater : Expression
	{
		public override string Key
		{
			get
			{
				return "greater";
			}
		}
		public override IResult Evaluate()
		{
			IResult result = base.Evaluate();
			decimal? compare = null;
			decimal value = 0m;
			foreach (IToken operand in this.Operands)
			{
				if (!compare.HasValue)
				{

                    if (null != operand.Value)
                    {
                        DateTime dtcompare = DateTime.MinValue;
                        if (DateTime.TryParse(operand.Value.ToString(), out dtcompare))
                            compare = dtcompare.Ticks;
                    }
                    if (!compare.HasValue)
                    {
                        decimal test;
                        if (operand.Value == null || !decimal.TryParse(operand.Value.ToString(), out test))
                        {
                            result.Status = ResultStatus.Unsuccessful;
                            break;
                        }
                        compare = new decimal?(test);
                    }
				}
				else
                {
                    DateTime dtvalue = DateTime.MinValue;
                    if (null != operand.Value)
                    {
                        if (DateTime.TryParse(operand.Value.ToString(), out dtvalue))
                            value = dtvalue.Ticks;
                    }
                    else if (operand.Value == null || (dtvalue != DateTime.MinValue && !decimal.TryParse(operand.Value.ToString(), out value)))
                    {
                        result.Status = ResultStatus.Unsuccessful;
                        break;
                    }
					if (compare.Value <= value)
					{
						result.Status = ResultStatus.Unsuccessful;
						break;
					}
				}
			}
			this.Value = result.IsSuccessful();
			return result;
		}
	}
}
