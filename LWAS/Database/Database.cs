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
using System.Xml;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Database
{
    public class Database
    {
        public string Name { get; private set; }
        public string ViewsKey { get; private set; }
        public ViewsManager ViewsManager {get; private set;}

        public Database(string name, string viewsKey, IStorageAgent agent, IExpressionsManager expressionsManager)
        {
            this.Name = name;
            this.ViewsKey = viewsKey;
            this.ViewsManager = new ViewsManager(viewsKey, agent, expressionsManager);
        }
    }
}
