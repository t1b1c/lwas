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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;

namespace LWAS.Infrastructure.Personalization
{
	public class DictionaryPersonalizationState : PersonalizationState
	{
        public Dictionary<string, PersonalizationDictionary> States { get; set; }
        public bool ReadOnly { get; set; }
		public override bool IsEmpty
		{
			get { return 0 == this.States.Count; }
		}

		public DictionaryPersonalizationState(WebPartManager manager) : base(manager)
		{
			this.States = new Dictionary<string, PersonalizationDictionary>();
		}

        public override void ApplyWebPartManagerPersonalization()
		{
			if (this.States.ContainsKey(base.WebPartManager.ID))
				this.ApplyPersonalization(base.WebPartManager, this.States[base.WebPartManager.ID]);
		}
		
        public override void ApplyWebPartPersonalization(WebPart webPart)
		{
			if (this.States.ContainsKey(webPart.ID))
				this.ApplyPersonalization(webPart, this.States[webPart.ID]);
		}

        public virtual void ApplyPersonalization(Control target, PersonalizationDictionary personalizations)
		{
			if (target is ITrackingPersonalizable)
				((ITrackingPersonalizable)target).BeginLoad();

            if (target is IPersonalizable)
				((IPersonalizable)target).Load(personalizations);

            if (target is IVersioningPersonalizable)
				((IVersioningPersonalizable)target).Load(personalizations);

            foreach (string key in personalizations.Keys)
			{
				if (null != target.GetType().GetProperty(key))
					ReflectionServices.SetValue(target, key, personalizations[key].Value, true);
			}

            if (target is ITrackingPersonalizable)
				((ITrackingPersonalizable)target).EndLoad();
		}
		
        public override void ExtractWebPartManagerPersonalization()
		{
			this.ExtractPersonalization(base.WebPartManager);
			base.SetDirty();
		}
		
        public override void ExtractWebPartPersonalization(WebPart webPart)
		{
			this.ExtractPersonalization(webPart);
		}
		
        public virtual void ExtractPersonalization(Control source)
		{
			if (!this.States.ContainsKey(source.ID))
				this.States.Add(source.ID, new PersonalizationDictionary());

            PersonalizationDictionary personalizations = this.States[source.ID];
			
            if (source is ITrackingPersonalizable)
				((ITrackingPersonalizable)source).BeginSave();

            if (source is IPersonalizable)
				((IPersonalizable)source).Save(personalizations);

            this.FillPersonalizationDictionary(source, PersonalizableAttribute.GetPersonalizableProperties(source.GetType()), personalizations);
			
            if (source is ITrackingPersonalizable)
				((ITrackingPersonalizable)source).EndSave();

            if (!this.States.ContainsKey(source.ID))
				this.States.Add(source.ID, personalizations);
		}

		public void FillPersonalizationDictionary(Control control, ICollection propertyInfos, PersonalizationDictionary personalizations)
		{
			foreach (PropertyInfo propertyInfo in propertyInfos)
			{
				PersonalizableAttribute attribute = (PersonalizableAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(PersonalizableAttribute));
				PersonalizationEntry entry = new PersonalizationEntry(ReflectionServices.ExtractValue(control, propertyInfo.Name), attribute.Scope, attribute.IsSensitive);
				if (!personalizations.Contains(propertyInfo.Name))
					personalizations.Add(propertyInfo.Name, entry);
				else
					personalizations[propertyInfo.Name] = entry;
			}
		}

        public override string GetAuthorizationFilter(string webPartID)
		{
			string result;
			if (null == this.States)
				result = string.Empty;
			else
			{
				if (!this.States.ContainsKey(webPartID))
					result = string.Empty;
				else
				{
					if (!this.States[webPartID].Contains("AuthorizationFilter"))
						result = string.Empty;
					else
						result = this.States[webPartID]["AuthorizationFilter"].Value.ToString();
				}
			}
			return result;
		}

		public override void SetWebPartDirty(WebPart webPart)
		{
		}

		public override void SetWebPartManagerDirty()
		{
		}

		public bool IsPartPresent(string id)
		{
			bool ret = false;
			if (id == base.WebPartManager.ID)
				ret = true;
			else
			{
				WebPart part = base.WebPartManager.WebParts[id];
				if (null != part)
				{
					ret = true;
					if (part.IsClosed)
						ret = false;
				}
			}

            return ret;
		}
	}
}
