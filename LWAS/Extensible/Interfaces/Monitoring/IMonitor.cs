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
using System.Xml.Serialization;

namespace LWAS.Extensible.Interfaces.Monitoring
{
	public interface IMonitor : IXmlSerializable
	{
		IRecordsCollection Records { get; }
        bool IsMonitoring { get; set; }
		void Start();
		void Register(IReporter reporter, IEvent e);
		void Stop();
		string Dump(bool current);
		IEvent NewEventInstance(string key, EVENT_TYPE eventType);
		IEvent NewEventInstance(string key, IEvent parent, EVENT_TYPE eventType);
		IEvent NewEventInstance(string key, IEvent parent, object data, EVENT_TYPE eventType);
		bool HasErrors();
	}
}
