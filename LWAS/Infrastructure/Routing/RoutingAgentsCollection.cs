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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing
{
    public class RoutingAgentsCollection : IRoutingAgentsCollection
    {
        List<IRoutingAgent> agents;

        public RoutingAgentsCollection()
        {
            agents = new List<IRoutingAgent>();
        }

        public void Add(IRoutingAgent agent)
        {
            agents.Add(agent);
        }

        public void Remove(IRoutingAgent agent)
        {
            agents.Remove(agent);
        }

        public void Clear()
        {
            agents.Clear();
        }

        public IEnumerator<IRoutingAgent> GetEnumerator()
        {
            return agents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
