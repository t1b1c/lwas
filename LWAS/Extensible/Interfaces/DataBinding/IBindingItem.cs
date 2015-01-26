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
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Extensible.Interfaces.DataBinding
{
	public interface IBindingItem
	{
		object Target
		{
			get;
			set;
		}
		string TargetProperty
		{
			get;
			set;
		}
		object Source
		{
			get;
			set;
		}
		string SourceProperty
		{
			get;
			set;
		}
		object DefaultValue
		{
			get;
			set;
		}
        IExpression Expression
        {
            get;
            set;
        }
		IResult Bind();
		object ExtractValue(object source, string propertyName);
		void SetValue(object target, string propertyName, object value);
	}
}
