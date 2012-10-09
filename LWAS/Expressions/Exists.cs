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
	public class Exists : Expression
	{
		public override string Key
		{
			get
			{
				return "exists";
			}
		}
		public override IResult Evaluate()
		{
			IResult result = base.Evaluate();
			bool exists = true;
			foreach (IToken operand in this.Operands)
			{
				exists = (operand.Value != null && operand.Value != DBNull.Value);
				if (exists && operand.Value is string)
				{
					exists = (operand.Value.ToString() != "");
				}
				if (!exists)
				{
					result.Status = ResultStatus.Unsuccessful;
					break;
				}
			}
			this.Value = (result.IsSuccessful() && exists);
			return result;
		}
	}
}
