﻿/*
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
using System.Xml;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedTransitSource : TemplatedToken
    {
        public override string SerializedName
        {
            get
            {
                return "source";
            }
        }

        public TemplatedTransitSource(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        { }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedTransitSource result = new TemplatedTransitSource(this.ExpressionsManager);
            result.TokenType = this.TokenType;
            result.Id = this.Id;
            result.Member = this.Member;
            result.Value = this.Value;
            
            Make(recipe, makePolicy, result);
            
            return result;
        }
    }
}
