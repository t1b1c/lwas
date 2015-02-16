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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing
{
    public class RoutesCollection : IRoutesCollection
    {
        List<IRoute> routes;

        public IRoute this[string key]
        {
            get
            {
                return routes.SingleOrDefault(r => r.Key == key);
            }
            set
            {
                IRoute route = routes.SingleOrDefault(r => r.Key == key);
                if (null != route)
                    routes.Remove(route);
                routes.Add(value);
            }
        }

        public RoutesCollection()
        {
            routes = new List<IRoute>();
        }

        public void Add(IRoute route)
        {
            routes.Add(route);
        }

        public void Remove(IRoute route)
        {
            if (routes.Contains(route))
                routes.Remove(route);
        }

        public void Clear()
        {
            routes.Clear();
        }

        public IEnumerator<IRoute> GetEnumerator()
        {
            return routes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
