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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing.Screens
{
    public class ApplicationRoute : BaseRoute
    {        
        public override string Target
        {
            get { return this.Key; }
        }

        public ApplicationRoute(string key)
            : base(key, key)
        {}

        public override void Resolve()
        {
            _path = this.OriginalPath;
            base.Resolve();
        }
    }
}
