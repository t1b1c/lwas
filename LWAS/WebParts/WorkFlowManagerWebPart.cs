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

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WebParts
{
	public class WorkFlowManagerWebPart : EditableWebPart, IReporter
	{
		private IWorkFlowsCollection _workFlows;
		public IWorkFlowsCollection WorkFlows
		{
			get { return this._workFlows; }
			set { this._workFlows = value; }
		}

        private IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager
		{
			get { return this._expressionsManager; }
			set { this._expressionsManager = value; }
		}

        private IStatePersistence _statePersistence;
        public IStatePersistence StatePersistence
		{
			get { return this._statePersistence; }
			set { this._statePersistence = value; }
		}

        private IMonitor _monitor;
        public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}

        public WorkFlowManagerWebPart()
		{
			this.Hidden = true;
		}
		
        public override void Initialize()
		{
			if (null == this.ConfigurationParser) throw new MissingProviderException("configuration parser");
			if (null == this._workFlows) throw new InitializationException("WorkFlows collection not set");
			if (null == this._monitor) throw new InitializationException("Monitor not set");
			if (null == this._expressionsManager) throw new InitializationException("ExpressionsManager not set");
			if (null == this._statePersistence) throw new InitializationException("StatePersitence not set");

			this.ConfigurationParser.Parse(this);
			this.StatePersistence.Open();
			this.WorkFlows.Run();
			base.Initialize();
		}

		protected override void OnUnload(EventArgs e)
		{
			if (null != this.StatePersistence)
				this.StatePersistence.Close();

            base.OnUnload(e);
		}
	}
}
