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

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow.Conditions
{
	public class Condition : ICondition, IReporter
	{
		private IExpression _expression;
		private string _title;
		private IMonitor _monitor;
		public event EventHandler Passed;
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
		public virtual string Title
		{
			get
			{
				return this._title;
			}
			set
			{
				this._title = value;
			}
		}
		public IMonitor Monitor
		{
			get
			{
				return this._monitor;
			}
			set
			{
				this._monitor = value;
			}
		}
		public virtual bool Check()
		{
            if (null == this._expression)
                return true;
            else
            {
                bool check = this._expression.Evaluate().IsSuccessful();
                if (check)
                    this.Monitor.Register(this, this.Monitor.NewEventInstance(String.Format("expression passed: {0}", _expression.Key), null, _expression.Value, EVENT_TYPE.Trace));
                else
                    this.Monitor.Register(this, this.Monitor.NewEventInstance(String.Format("expression failed: {0}", _expression.Key), null, _expression.Value, EVENT_TYPE.Trace));
                return check;
            }
		}
		protected virtual void OnPassed(EventArgs e)
		{
			if (null != this.Passed)
			{
				this.Passed(this, EventArgs.Empty);
			}
		}
	}
}
