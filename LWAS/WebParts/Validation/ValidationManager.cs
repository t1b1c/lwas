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
using System.Collections.Generic;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Validation;

namespace LWAS.WebParts.Validation
{
	public class ValidationManager : IValidationManager, IReporter
	{
		private IExpressionsManager _expressionsManager;
		private List<IValidationTask> _tasks = new List<IValidationTask>();
		private Dictionary<string, IValidationHandler> _registry = new Dictionary<string, IValidationHandler>();
		private IValidationTask _failTask;
		private string _title = "ValidationManager";
		private IMonitor _monitor;
		public IExpressionsManager ExpressionsManager
		{
			get
			{
				return this._expressionsManager;
			}
			set
			{
				this._expressionsManager = value;
			}
		}
		public List<IValidationTask> Tasks
		{
			get
			{
				return this._tasks;
			}
			set
			{
				this._tasks = value;
			}
		}
		public Dictionary<string, IValidationHandler> Registry
		{
			get
			{
				return this._registry;
			}
		}
		public IValidationHandler Add
		{
			set
			{
				this._registry.Add(value.Key, value);
			}
		}
		public string Title
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
		public IValidationHandler Handler(string key)
		{
			if (!this._registry.ContainsKey(key))
			{
				throw new ArgumentException(string.Format("Unknown validation handler '{0}'", key));
			}
			return this._registry[key];
		}
		public IValidationTask NewValidationTaskInstance()
		{
			return new ValidationTask();
		}
		public void RegisterTask(IValidationTask task)
		{
			this._tasks.Add(task);
		}
		public IResult Validate()
		{
			IResult result = null;
			foreach (IValidationTask task in this._tasks)
			{
				if (null == result)
				{
					result = task.Validate();
				}
				else
				{
					result.Concatenate(task.Validate());
				}
				if (!result.IsSuccessful())
				{
					break;
				}
			}
			return result;
		}
		public IResult Validate(object target)
		{
			IResult result = null;
			foreach (IValidationTask task in this._tasks)
			{
				if (null == result)
				{
					result = task.Validate(target);
				}
				else
				{
					result.Concatenate(task.Validate(target));
				}
				if (!result.IsSuccessful())
				{
					this._failTask = task;
					break;
				}
			}
			return result;
		}
		public IValidationTask LastFail()
		{
			return this._failTask;
		}
	}
}
