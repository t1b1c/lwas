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

using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow
{
	public class WorkFlow : IWorkFlow, ITriggerable, IReporter
	{
		private ConditionsCollection _conditions;
		public IConditionsCollection Conditions
		{
			get
			{
				if (null == this._conditions)
					this._conditions = new ConditionsCollection();

                return this._conditions;
			}
		}
        private JobsCollection _jobs;
        public IJobsCollection Jobs
		{
			get
			{
				if (null == this._jobs)
					this._jobs = new JobsCollection();

                return this._jobs;
			}
		}
        private string _title;
        public string Title
		{
			get { return this._title; }
			set { this._title = value; }
		}
        private IMonitor _monitor;
        public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}

		public virtual void OnTrigger(ITrigger trigger)
		{
		}

		public virtual bool Run()
		{
			bool result;
			if (!this.Conditions.Check())
				result = false;
			else
			{
				if (null != this._monitor)
					this._monitor.Register(this, this._monitor.NewEventInstance("run workflow", EVENT_TYPE.Trace));

                result = this.Jobs.Run();
			}
			return result;
		}
	}
}
