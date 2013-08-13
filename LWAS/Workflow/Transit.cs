/*
 * Copyright 2006-2013 TIBIC SOLUTIONS
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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;
using LWAS.Infrastructure;

namespace LWAS.WorkFlow
{
	public class Transit : ITransit, IReporter
	{
        private string _key;
        public string Key
		{
			get { return this._key; }
			set { this._key = value; }
		}
        
        private IExpression _expression;
        public IExpression Expression
		{
			get { return this._expression; }
			set { this._expression = value; }
		}
        
        private bool _isPersistent = false;
        public bool IsPersistent
		{
			get { return this._isPersistent; }
			set { this._isPersistent = value; }
		}

        private ITransitPoint _source;
        public ITransitPoint Source
		{
			get { return this._source; }
			set { this._source = value; }
		}
        
        private ITransitPoint _destination;
        public ITransitPoint Destination
		{
			get { return this._destination; }
			set { this._destination = value; }
		}
        
        private IStatePersistence _storage;
        public IStatePersistence Storage
		{
			get { return this._storage; }
			set { this._storage = value; }
		}
        
        private string _title;
        public string Title
		{
			get { return this.UniqueID; }
			set { this._title = value; }
		}
        
        private IMonitor _monitor;
        public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}

        private IEvent _contextEvent;
        public IEvent ContextEvent
        {
            get { return _contextEvent; }
            set { _contextEvent = value; }
        }
        
        public string UniqueID
        {
            get;
            set;
        }
		
        public virtual bool Run()
		{
			if (string.IsNullOrEmpty(this._key)) throw new InvalidOperationException("Invalid key");
			if (null == this._storage) throw new InvalidOperationException("State persistence not set");

		    IEvent runEvent = this._monitor.NewEventInstance("run transit " + this.Title, _contextEvent, EVENT_TYPE.Trace);
			if (null != this._monitor)
				this._monitor.Register(this, runEvent);

            bool result = true;
			if (null != this._expression)
			{
                result = this._expression.Evaluate().IsSuccessful();
                if (result)
                    this._monitor.Register(this, this._monitor.NewEventInstance(String.Format("expression passed: {0}", _expression.Key), runEvent, _expression.Value, EVENT_TYPE.Trace));
                else
                    this._monitor.Register(this, this._monitor.NewEventInstance(String.Format("expression failed: {0}", _expression.Key), runEvent, _expression.Value, EVENT_TYPE.Trace));
            }
			if (result)
			{
				if (null != this._source)
				{
					object val = null;
					if (null != this._source.Chronicler)
					{
						try
                        {
                            if (null != this._monitor)
                            {
                                string title = _source.Chronicler.Title;
                                if (_source.Chronicler is WebPart)
                                    title = ((WebPart)_source.Chronicler).ID;
                                this._monitor.Register(this, this._monitor.NewEventInstance(String.Format("get {0}.{1}", title, this._source.Member), runEvent, val, EVENT_TYPE.Trace));
                            }

							val = ReflectionServices.ExtractValue(this._source.Chronicler, this._source.Member);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("extract '{0}'.'{1}' error", this._source.Chronicler, this._source.Member), ex);
						}
					}
					if (null == val)
					{
						val = this._source.Value;

                        if (null != this._monitor)
                        {
                            this._monitor.Register(this, this._monitor.NewEventInstance(String.Format("get {0}", this._source.Value), runEvent, val, EVENT_TYPE.Trace));
                        }
					}
					this._storage.Push(this._key, val);
				}
				if (this._destination != null && null != this._destination.Chronicler)
				{
					object val = null;
					if (this._storage.ContainsKey(this._key))
					{
						val = this._storage.Pull(this._key);
					}
					if (null != this._monitor)
					{
                        string title = _destination.Chronicler.Title;
                        if (_destination.Chronicler is WebPart)
                            title = ((WebPart)_destination.Chronicler).ID;
						this._monitor.Register(this, this._monitor.NewEventInstance(String.Format("set {0}.{1}", title, this._destination.Member), runEvent, val, EVENT_TYPE.Trace));
					}
					if (this._source != null && this._source.Chronicler == null && !string.IsNullOrEmpty(this._source.Member) && null != val)
					{
						object obj;
						try
						{
							obj = ReflectionServices.ExtractValue(val, this._source.Member);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("extract '{1}' from repository key '{0}' error", this._key, this._source.Member), ex);
						}
						try
						{
							ReflectionServices.SetValue(this._destination.Chronicler, this._destination.Member, obj);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("set value to '{0}'.'{1}' error", this._destination.Chronicler.Title, this._destination.Member), ex);
						}
					}
					else
					{
						try
						{
							ReflectionServices.SetValue(this._destination.Chronicler, this._destination.Member, val);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("set value to '{0}'.'{1}' error", this._destination.Chronicler.Title, this._destination.Member), ex);
						}
					}
				}
				if (!this._isPersistent)
				{
					this._storage.Erase(this._key);
				}
				result = true;
			}
			return result;
		}
	}
}
