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
using System.Linq;
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
			get { return _key; }
			set { _key = value; }
		}
        
        private IExpression _expression;
        public IExpression Expression
		{
			get { return _expression; }
			set { _expression = value; }
		}
        
        private bool _isPersistent = false;
        public bool IsPersistent
		{
			get { return _isPersistent; }
			set { _isPersistent = value; }
		}

        private ITransitPoint _source;
        public ITransitPoint Source
		{
			get { return _source; }
			set { _source = value; }
		}
        
        private ITransitPoint _destination;
        public ITransitPoint Destination
		{
			get { return _destination; }
			set { _destination = value; }
		}
        
        private IStatePersistence _storage;
        public IStatePersistence Storage
		{
			get { return _storage; }
			set { _storage = value; }
		}
        
        private string _title;
        public string Title
		{
			get { return this.UniqueID; }
			set { _title = value; }
		}
        
        private IMonitor _monitor;
        public IMonitor Monitor
		{
			get { return _monitor; }
			set { _monitor = value; }
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
			if (string.IsNullOrEmpty(_key)) throw new InvalidOperationException("Invalid key");
			if (null == _storage) throw new InvalidOperationException("State persistence not set");

		    IEvent runEvent = _monitor.NewEventInstance("run transit " + this.Title, _contextEvent, EVENT_TYPE.Trace);
			if (null != _monitor)
				_monitor.Register(this, runEvent);

            bool result = true;
			if (null != _expression)
			{
                result = _expression.Evaluate().IsSuccessful();
                if (result)
                    _monitor.Register(this, _monitor.NewEventInstance(String.Format("expression passed: {0}", _expression.Key), runEvent, _expression.Value, EVENT_TYPE.Trace));
                else
                    _monitor.Register(this, _monitor.NewEventInstance(String.Format("expression failed: {0}", _expression.Key), runEvent, _expression.Value, EVENT_TYPE.Trace));
            }
			if (result)
			{
				if (null != _source)
				{
					object val = null;
                    if (_source.Expression == null)
                    {
                        if (null != _source.Chronicler)
                        {
                            try
                            {
                                if (null != _monitor)
                                {
                                    string title = _source.Chronicler.Title;
                                    if (_source.Chronicler is WebPart)
                                        title = ((WebPart)_source.Chronicler).ID;
                                    _monitor.Register(this, _monitor.NewEventInstance(String.Format("get {0}.{1}", title, _source.Member), runEvent, val, EVENT_TYPE.Trace));
                                }

                                val = ReflectionServices.ExtractValue(_source.Chronicler, _source.Member);
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException(string.Format("extract '{0}'.'{1}' error", _source.Chronicler, _source.Member), ex);
                            }
                        }
                        if (null == val)
                        {
                            val = _source.Value;

                            if (null != _monitor)
                            {
                                _monitor.Register(this, _monitor.NewEventInstance(String.Format("get {0}", _source.Value), runEvent, val, EVENT_TYPE.Trace));
                            }
                        }
                    }
                    else
                    {
                        var eval_result = _source.Expression.Evaluate();    // 'exists' returns unsuccessful status when false
                        val = _source.Expression.Value;
                        //if (eval_result.IsSuccessful())
                        //    val = _source.Expression.Value;
                        //else
                        //    throw new ApplicationException("transit source expression evaluation failed", eval_result.Exceptions.FirstOrDefault());
                    }
                    
					_storage.Push(_key, val);
				}
				if (_destination != null && null != _destination.Chronicler)
				{
					object val = null;
					if (_storage.ContainsKey(_key))
					{
						val = _storage.Pull(_key);
					}
					if (null != _monitor)
					{
                        string title = _destination.Chronicler.Title;
                        if (_destination.Chronicler is WebPart)
                            title = ((WebPart)_destination.Chronicler).ID;
						_monitor.Register(this, _monitor.NewEventInstance(String.Format("set {0}.{1}", title, _destination.Member), runEvent, val, EVENT_TYPE.Trace));
					}
					if (_source != null && _source.Chronicler == null && !string.IsNullOrEmpty(_source.Member) && null != val)
					{
						object obj;
						try
						{
							obj = ReflectionServices.ExtractValue(val, _source.Member);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("extract '{1}' from repository key '{0}' error", _key, _source.Member), ex);
						}
						try
						{
							ReflectionServices.SetValue(_destination.Chronicler, _destination.Member, obj);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("set value to '{0}'.'{1}' error", _destination.Chronicler.Title, _destination.Member), ex);
						}
					}
					else
					{
						try
						{
							ReflectionServices.SetValue(_destination.Chronicler, _destination.Member, val);
						}
						catch (Exception ex)
						{
							throw new ApplicationException(string.Format("set value to '{0}'.'{1}' error", _destination.Chronicler.Title, _destination.Member), ex);
						}
					}
				}
				if (!_isPersistent)
				{
					_storage.Erase(_key);
				}
				result = true;
			}
			return result;
		}
	}
}
