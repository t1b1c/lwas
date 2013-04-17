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
using System.Xml;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Workflow.Recipes
{
    public class TemplatedJob : TemplatedFlow
    {
        public TemplatedConditionCollection Conditions { get; set; }
        public TemplatedTransitCollection Transits { get; set; }

        public TemplatedJob(IExpressionsManager expressionsManager)
            : base(expressionsManager)
        {
            this.Conditions = new TemplatedConditionCollection(this.ExpressionsManager);
            this.Transits = new TemplatedTransitCollection(this.ExpressionsManager);
        }

        public override TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy)
        {
            TemplatedJob job = new TemplatedJob(this.ExpressionsManager);
            foreach (TemplatedCondition condition in this.Conditions)
                job.Conditions.Add(condition.Make(recipe, makePolicy) as TemplatedCondition);
            foreach (TemplatedTransit transit in this.Transits)
                job.Transits.Add(transit.Make(recipe, makePolicy) as TemplatedTransit);
            return job;
        }

        public override void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");

            writer.WriteStartElement("job");
            writer.WriteAttributeString("key", this.Key);
            this.Conditions.ToXml(writer);
            this.Transits.ToXml(writer);
            writer.WriteEndElement();   // job
        }

        public override void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Key = element.Attribute("key").Value;
            this.Conditions.FromXml(element.Element("conditions"));
            this.Transits.FromXml(element.Element("transits"));
        }
    }
}
