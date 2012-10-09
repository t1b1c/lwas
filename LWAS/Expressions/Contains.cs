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
	public class Contains : Expression
	{
		public override string Key
		{
			get
			{
				return "contains";
			}
		}
		public override IResult Evaluate()
		{
			IResult result = base.Evaluate();
			try
			{
				object target = null;
				bool initialized = false;
				foreach (IToken token in this.Operands)
				{
					BasicToken basicToken = token as BasicToken;
					if (null == basicToken)
					{
						throw new InvalidOperationException("This operand is not a BasicToken");
					}
					if (target == null && !initialized)
					{
						target = basicToken.Value;
						initialized = true;
					}
					else
					{
						if (target != basicToken.Value)
						{
							if ((target == null && basicToken.Value != null) || (target != null && null == basicToken.Value))
							{
								this.Value = false;
							}
							else
							{
								this.Value = target.ToString().Contains(basicToken.Value.ToString());
							}
							if (!(bool)this.Value)
							{
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				result.Exceptions.Add(e);
			}
			return result;
		}
	}
}
