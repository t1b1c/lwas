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

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Infrastructure;

namespace LWAS.WebParts.DataBinding
{
	public class BindingItem : IBindingItem
	{
		private object _target;
		private string _targetProperty;
		private object _source;
		private string _sourceProperty;
		private object _defaultValue;
        private IExpression _expression;

		public object Target
		{
			get { return this._target; }
			set { this._target = value; }
		}
		public string TargetProperty
		{
			get { return this._targetProperty; }
			set { this._targetProperty = value; }
		}
		public object Source
		{
			get { return this._source; }
			set { this._source = value; }
		}
		public string SourceProperty
		{
			get { return this._sourceProperty; }
			set { this._sourceProperty = value; }
		}
		public object DefaultValue
		{
			get { return this._defaultValue; }
			set { this._defaultValue = value; }
		}
        public IExpression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

		public BindingItem()
		{
		}
		public BindingItem(object target, string targetProperty, object source, string sourceProperty)
		{
			this._target = target;
			this._targetProperty = targetProperty;
			this._source = source;
			this._sourceProperty = sourceProperty;
		}
		public IResult Bind()
		{
			BindingResult result = new BindingResult();
			try
			{
				if (null == this._target) throw new MissingBindingElementException("target object");
				if (string.IsNullOrEmpty(this._targetProperty)) throw new MissingBindingElementException("target property name");
				if (null == this._source) throw new MissingBindingElementException("source object");
				if (string.IsNullOrEmpty(this._sourceProperty)) throw new MissingBindingElementException("source property name");

                if (null == this.Expression || this.Expression.Evaluate().IsSuccessful())
                {
                    object val = this.ExtractValue(this._source, this._sourceProperty);
                    if ((val == null || (val is string && string.IsNullOrEmpty((string)val))) && null != this._defaultValue)
                        val = this._defaultValue;

                    this.SetValue(this._target, this._targetProperty, val);
                }

                result.Status = ResultStatus.Successful;
			}
			catch (Exception exception)
			{
				result.Status = ResultStatus.Unsuccessful;
				result.Exceptions.Add(exception);
			}
			return result;
		}
		public object ExtractValue(object source, string propertyName)
		{
			return ReflectionServices.ExtractValue(source, propertyName);
		}
		public void SetValue(object target, string propertyName, object value)
		{
			ReflectionServices.SetValue(target, propertyName, value);
		}
	}
}
