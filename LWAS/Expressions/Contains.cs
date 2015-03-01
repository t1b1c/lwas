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
                this.Value = false;
				object target = null;
				bool initialized = false;
				foreach (IToken token in this.Operands)
				{
					if (!initialized)
					{
						target = token.Value;
						initialized = true;
					}
					else
					{
                        if (target != token.Value)
                        {
                            if ((target == null && token.Value != null) || (target != null && null == token.Value))
                                this.Value = false;
                            else
                                this.Value = target.ToString().ToLower().Contains(token.Value.ToString().ToLower());

                            if (!(bool)this.Value)
                                break;
                        }
                        else
                            this.Value = true;
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
