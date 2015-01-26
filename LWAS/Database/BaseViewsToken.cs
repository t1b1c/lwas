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
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Exceptions;
using LWAS.Infrastructure;

namespace LWAS.Database
{
    public abstract class BaseViewsToken: IToken
    {
        public abstract string Key { get; }

        public object Value { get; set; }
        public virtual bool IsValid
        {
            get { return true; }
        }

        public abstract void Make(IConfigurationType config, IExpressionsManager manager);
        public abstract IResult Evaluate();
        public abstract void ToConfiguration(IConfigurationElement config);
        public abstract void ToSql(StringBuilder builder);
    }
}
