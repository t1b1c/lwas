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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Expressions
{
	public class Equals : Expression
	{
		public override string Key
		{
			get
			{
				return "equals";
			}
		}
		public override IResult Evaluate()
		{
			IResult result = base.Evaluate();
			bool equals = true;
			object comparison = null;
			bool initialized = false;
			foreach (IToken operand in this.Operands)
			{
				if (!initialized)
				{
					comparison = operand.Value;
					initialized = true;
				}
				else
				{
					equals = (comparison == null && operand.Value == null);
					equals = ((equals || null != operand.Value) && comparison != null && comparison.ToString() == operand.Value.ToString());
				}
				if (!equals)
				{
					break;
				}
			}
			this.Value = (result.IsSuccessful() && equals);
			if (!(bool)this.Value)
			{
				result.Status = ResultStatus.Unsuccessful;
			}
			return result;
		}
	}
}
