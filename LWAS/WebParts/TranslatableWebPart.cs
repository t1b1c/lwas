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
using System.Web.UI;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Translation;

namespace LWAS.WebParts
{
	public class TranslatableWebPart : ConfigurableWebPart, ITranslatable
	{
		private IDictionary<string, Control> _translationTargets;
		private ITranslator _translator = null;
		private string _language = null;
		public IDictionary<string, Control> TranslationTargets
		{
			get
			{
				if (null == this._translationTargets)
				{
					this._translationTargets = new Dictionary<string, Control>();
				}
				return this._translationTargets;
			}
		}
		public ITranslator Translator
		{
			get
			{
				return this._translator;
			}
			set
			{
				this._translator = value;
			}
		}
		public string Language
		{
			get
			{
				return this._language;
			}
			set
			{
				this._language = value;
			}
		}
		public override void Initialize()
		{
			if (null == this._translator)
			{
				throw new MissingProviderException("Translator");
			}
			base.Initialize();
		}
		protected override void OnComplete()
		{
			if (null == this._translator)
			{
				throw new MissingProviderException("Translator");
			}
			this._translator.Translate(this, this._language);
			this._translator.CleanUp();
			base.OnComplete();
		}
	}
}
