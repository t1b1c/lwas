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
using SIO = System.IO;

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing
{
    public class ComposedRoute : BaseRoute, IComposedRoute
    {
        public IRoute Root { get; set; }
        public IRoute Route { get; set; }

        public ComposedRoute(IRoute root, IRoute route)
            : base(route.Key, route.OriginalPath)
        {
            this.Root = root;
            this.Route = route;
        }

        public override void Resolve()
        {
            this.Root.Resolve();
            _path = SIO.Path.Combine(this.Root.Path, this.Route.OriginalPath);
            base.Resolve();
        }
    }
}
