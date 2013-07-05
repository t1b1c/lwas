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
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Expressions
{
	public class Expression : Token, IExpression, IToken
	{
		public override string Key
		{
			get { return "expression"; }
		}

        private ITokensCollection _operands;
        public virtual ITokensCollection Operands
		{
			get
			{
				if (null == this._operands)
					this._operands = new TokensCollection();
				return this._operands;
			}
			set { this._operands = value; }
		}

        public override bool IsValid
        {
            get
            {
                foreach (Token token in this.Operands)
                    if (!token.IsValid)
                        return false;
                return true;
            }
        }

		public override IResult Evaluate()
		{
            IResult result = new ExpressionResult();

			foreach (IToken child in this.Operands)
				result.Concatenate(child.Evaluate());

            return result;
		}

		public override void Make(IConfigurationType config, IExpressionsManager manager)
		{
			if (null == manager) throw new ArgumentNullException("manager");
			if (null == config)throw new ArgumentNullException("config");
			IConfigurationElement element = config as IConfigurationElement;
			if (null == element) throw new ArgumentException("config must be an IConfigurationElement");

			if (element.Elements.ContainsKey("operands"))
			{
				foreach (IConfigurationElement operandElement in element.GetElementReference("operands").Elements.Values)
				{
					string type = operandElement.GetAttributeReference("type").Value.ToString();
					IToken token = manager.Token(type);
					if (null == token) throw new InvalidOperationException(string.Format("Cannot make the type '{0}'", type));

					token.Make(operandElement, manager);
					this.Operands.Add(token);
				}
			}
		}
	}
}
