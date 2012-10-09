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
using LWAS.Extensible.Interfaces.Validation;

namespace LWAS.WebParts.Validation
{
	public class ExpressionHandler : ValidationHandler, IExpressionHandler, IValidationHandler
	{
		private IExpression _expression;
		public IExpression Expression
		{
			get
			{
				return this._expression;
			}
			set
			{
				this._expression = value;
			}
		}
		public override string Key
		{
			get
			{
				string result;
				if (string.IsNullOrEmpty(base.Key))
				{
					result = "expression";
				}
				else
				{
					result = base.Key;
				}
				return result;
			}
			set
			{
				base.Key = value;
			}
		}
		public override IResult Validate(object value)
		{
			IResult result = base.Validate(value);
			if (null != this._expression)
			{
				result = this._expression.Evaluate();
			}
			if (result.Status == ResultStatus.Unsuccessful)
			{
				result.Exceptions.Add(new Exception("Validation expression failed!"));
			}
			return result;
		}
	}
}
