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

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedTransitDestination : TemplatedToken
    {
        public override TransitTokenTypeEnum TokenType
        {
            get { return TransitTokenTypeEnum.Reference; }
            set { ; }
        }

        public override string SerializedName
        {
            get
            {
                return "destination";
            }
        }

        public TemplatedTransitDestination(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedTransitDestination result = new TemplatedTransitDestination(this.ExpressionsManager);
            result.Id = this.Id;
            result.Member = this.Member;

            Make(recipe, makePolicy, result);

            return result;
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");
            
            this.Id = element.Attribute("id").Value;
            this.Member = element.Attribute("member").Value;
        }
    }
}
