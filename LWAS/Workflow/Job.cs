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

using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow
{
	public class Job : IJob, ITriggerable, IReporter
	{
		private bool IsRunning = false;
        
        private bool _isRequired = false;
        public bool IsRequired
		{
			get { return this._isRequired; }
			set { this._isRequired = value; }
		}
        
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
        
        private TransitsCollection _transits;
        public ITransitsCollection Transits
		{
			get
			{
				if (null == this._transits)
					this._transits = new TransitsCollection();

                return this._transits;
			}
		}

        private string _title;
        public string Title
		{
			get { return "run job " + this._title; }
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
			if (this.IsRunning)
				this.Run();
		}
		
        public virtual bool Run()
		{
			this.IsRunning = true;
			bool result;
			if (!this.Conditions.Check())
				result = false;
			else
			{
				IEvent runEvent = this._monitor.NewEventInstance(this.Title, EVENT_TYPE.Trace);
				if (null != this._monitor)
					this._monitor.Register(this, runEvent);

                foreach (ITransit transit in this.Transits)
				{
					try
					{
                        transit.ContextEvent = runEvent;
                        transit.UniqueID = _title + "::" + this.Transits.IndexOf(transit) + "::" + transit.Key;
						transit.Run();
					}
					catch (Exception ex)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("transit '{0}' failed", transit.Key), null, ex, EVENT_TYPE.Error));
					}
				}
				result = true;
			}
			return result;
		}
	}
}
