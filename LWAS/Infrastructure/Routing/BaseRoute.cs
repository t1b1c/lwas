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

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing
{
    public class BaseRoute : IRoute
    {
        bool isResolved = false;

        string _key;
        public string Key 
        {
            get { return _key; }
        }
        string _originalPath;
        public string OriginalPath 
        {
            get { return _originalPath; }
        }

        protected string _path;
        public string Path
        {
            get 
            {
                if (!isResolved)
                    Resolve();
                return _path;
            }
        }

        public virtual string Target
        {
            get
            {
                return this.OriginalPath;
            }
        }

        public BaseRoute(string key, string originalPath)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            _key = key;
            _originalPath = originalPath;
        }

        public virtual void Resolve()
        {
            isResolved = true;
        }

        public virtual IRoute ToTarget()
        {
            return new BaseRoute(this.Key, this.Target);
        }
    }
}
