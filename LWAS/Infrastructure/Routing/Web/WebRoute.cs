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
using System.Web;
using SIO = System.IO;

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing.Web
{
    public class WebRoute : BaseRoute
    {
        public WebRoute(string key, string originalPath)
            : base(key, originalPath)
        {
        }

        SIO.DirectoryInfo info = null;
        public override string Target
        {
            get
            {
                if (null != info)
                    return info.Name;
                return null;
            }
        }

        public override void Resolve()
        {
            if (!SIO.Path.IsPathRooted(this.OriginalPath))
                _path = HttpContext.Current.Server.MapPath(this.OriginalPath);
            else
                _path = this.OriginalPath;
            info = new SIO.DirectoryInfo(_path);
            base.Resolve();
        }

        public override IRoute ToTarget()
        {
            if (null == info)
                Resolve();
            return base.ToTarget();
        }
    }
}
