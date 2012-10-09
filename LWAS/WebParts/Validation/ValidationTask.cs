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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Validation;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Validation
{
	public class ValidationTask : IValidationTask
	{
		private object _target;
		private string _member;
		private List<IValidationHandler> _handlers = new List<IValidationHandler>();
		public object Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
			}
		}
		public string Member
		{
			get
			{
				return this._member;
			}
			set
			{
				this._member = value;
			}
		}
		public IList<IValidationHandler> Handlers
		{
			get
			{
				return this._handlers;
			}
			set
			{
				this._handlers = new List<IValidationHandler>(value);
			}
		}
		public IResult Validate()
		{
			return this.Validate(this._target);
		}
		public IResult Validate(object target)
		{
			return this.Validate(target, this._member);
		}
		public IResult Validate(object target, string member)
		{
            if (0 == _handlers.Count)
                throw new InvalidOperationException(String.Format("No handlers to validate the member '{0}'", member));
			object value = ReflectionServices.ExtractValue(target, member);
			IResult result = null;
			foreach (IValidationHandler handler in this._handlers)
			{
				if (null == result)
				{
					result = handler.Validate(value);
				}
				else
				{
					result.Concatenate(handler.Validate(value));
				}
				if (!result.IsSuccessful())
				{
					break;
				}
			}
			return result;
		}
	}
}
