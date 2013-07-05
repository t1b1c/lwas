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
using System.Text.RegularExpressions;

using LWAS.Extensible.Interfaces.Expressions;

using LWAS.Expressions.Extensions;

namespace LWAS.Workflow.Recipes
{
    public enum MakePolicyType { Full, ForEdit }

    public abstract class TemplatedFlow
    {
        public const string WILDCARD = "?";
        public const string PLACEHOLDER = "{?}";
        public string Key { get; set; }

        IExpressionsManager _expressionsManager;
        public IExpressionsManager ExpressionsManager 
        {
            get { return _expressionsManager; }
        }

        public TemplatedFlow(IExpressionsManager expressionsManager)
        {
            _expressionsManager = expressionsManager;
        }

        static Regex _templatePattern = new Regex(@"\{(.*)}", RegexOptions.Compiled);
        public static Regex TemplatePattern
        {
            get { return _templatePattern; }
        }

        public static IEnumerable<string> ListTemplates(string source)
        {
            foreach (Match match in TemplatePattern.Matches(source))
                yield return match.Groups[1].Value;
        }

        public static bool IsTemplate(string source)
        {
            return TemplatePattern.IsMatch(source);
        }

        public static TemplatedFlow FromType(string type, IExpressionsManager expressionsManager)
        {
            TemplatedFlow flow = null;
            switch (type)
            {
                case "job":
                    flow = new TemplatedJob(expressionsManager);
                    break;
                case "condition":
                    flow = new TemplatedCondition(expressionsManager);
                    break;
                case "transit":
                    flow = new TemplatedTransit(expressionsManager);
                    break;
                case "source":
                    flow = new TemplatedTransitSource(expressionsManager);
                    break;
                case "destination":
                    flow = new TemplatedTransitDestination(expressionsManager);
                    break;
            }
            return flow;
        }

        public abstract TemplatedFlow Make(Recipe recipe, MakePolicyType makePolicy);
        public abstract void ToXml(XmlTextWriter writer);
        public abstract void FromXml(XElement element);

        protected void Make(Recipe recipe, MakePolicyType makePolicy, IExpression expression)
        {
            foreach (IBasicToken token in expression.Flatten().SelectMany(ex => ex.Operands.OfType<IBasicToken>()))
                Make(recipe, makePolicy, token);
        }

        protected void Make(Recipe recipe, MakePolicyType makePolicy, IBasicToken token)
        {
            string r_id = token.Source == null ? null : token.Source.ToString();
            string r_member = token.Member;
            string r_value = r_id;

            Make(recipe, makePolicy, ref r_id, ref r_member, ref r_value, !String.IsNullOrEmpty(token.Member));

            if (!String.IsNullOrEmpty(token.Member))
            {
                token.Source = r_id;
                token.Member = r_member;
            }
            else
                token.Value = r_value;
        }

        protected void Make(Recipe recipe, MakePolicyType makePolicy, ref string r_id, ref string r_member, ref string r_value, bool tokenIsReference)
        {
            foreach (VariableRecipeComponent component in recipe.VariableComponents)
            {
                RecipePartMember recipePartMember = component as RecipePartMember;
                string value = component.GetValue();
                string template = TemplatedFlow.PLACEHOLDER.Replace(TemplatedFlow.WILDCARD, component.Name);
                if (tokenIsReference && component is RecipePart)
                {
                    if (null != recipePartMember)
                    {
                        if (makePolicy == MakePolicyType.ForEdit)
                            value = String.Format("{0}.{1}", recipePartMember.Member, template);

                        if (!String.IsNullOrEmpty(r_member) && r_member.Contains(template))
                        {
                            if (makePolicy == MakePolicyType.ForEdit)
                            {
                                r_id = recipePartMember.Part;
                                r_member = r_member.Replace(template, value);
                            }
                            else
                            {
                                r_id = recipePartMember.ValuePart;
                                if (recipePartMember.IsValueFromReference)
                                    r_member = recipePartMember.ValueMember;
                                else
                                    r_member = String.Format("{0}.{1}", recipePartMember.Member, recipePartMember.ValueMember);
                            }
                        }
                    }
                    else if (r_id == template)
                        r_id = value;
                }
                else if (!tokenIsReference &&
                         null != recipePartMember &&
                         recipePartMember.IsValueFromReference &&
                         !String.IsNullOrEmpty(r_value) &&
                         r_value.Contains(template))
                {
                    value = recipePartMember.ValueMember;
                    if (String.IsNullOrEmpty(value) && makePolicy == MakePolicyType.ForEdit)
                        value = template;

                    r_value = r_value.Replace(template, value);

                }
                else if (tokenIsReference && component is AbsoluteValue)
                {
                    if (makePolicy == MakePolicyType.Full)
                        r_member = r_member.Replace(template, value);
                }
                else if (!tokenIsReference && component is AbsoluteValue)
                {
                    if (!(String.IsNullOrEmpty(value) && makePolicy == MakePolicyType.ForEdit))
                        r_value = r_value.Replace(template, value);
                }

            }
        }
    }
}
