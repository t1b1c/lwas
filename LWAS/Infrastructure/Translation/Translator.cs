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
using System.Collections.Generic;
using System.Web.UI;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Translation;

namespace LWAS.Infrastructure.Translation
{
	public class Translator : ITranslator, IInitializable, ILifetime
	{
		private StringsStorage _storage;
		private string _defaultLanguage;
		private int _initialization;
		private int _creation;
		private int _change;
		private int _completion;
		private RequestInitializationCallback _requestInitialization;
		public StringsStorage Storage
		{
			get
			{
				if (null == this._storage)
				{
					this._storage = new StringsStorage();
				}
				return this._storage;
			}
		}
		public string DefaultLanguage
		{
			get { return this._defaultLanguage; }
			set { this._defaultLanguage = value; }
		}
		public int Initialization
		{
			get { return this._initialization; }
			set { this._initialization = value; }
		}
		public int Creation
		{
			get { return this._creation; }
			set { this._creation = value; }
		}
		public int Change
		{
			get { return this._change; }
			set { this._change = value; }
		}
		public int Completion
		{
			get { return this._completion; }
			set { this._completion = value; }
		}
		public RequestInitializationCallback RequestInitialization
		{
			get { return this._requestInitialization; }
			set { this._requestInitialization = value; }
		}
		public virtual ITranslationResult Translate(string key, string language)
		{
			ITranslationResult result = new TranslationResult();
			if (string.IsNullOrEmpty(language))
			{
				language = this._defaultLanguage;
			}
			StringsStorageEntry entry = this.Storage.GetEntryByKey(key, null);
			if (entry != null && entry.Words.ContainsKey(language))
			{
				result.Translation = entry.Words[language];
			}
			else
			{
				result.Status = ResultStatus.Unsuccessful;
			}
			return result;
		}
		public virtual ITranslationResult Translate(string key, string word, string language)
		{
			ITranslationResult result;
			if (!string.IsNullOrEmpty(key))
			{
				result = this.Translate(key, language);
				if (result.IsSuccessful())
				{
					return result;
				}
			}
			result = new TranslationResult();
			if (string.IsNullOrEmpty(language))
			{
				language = this._defaultLanguage;
			}
			StringsStorageEntry entry = this.Storage.GetEntryByWord(word);
			if (entry != null && entry.Words.ContainsKey(language))
			{
				result.Translation = entry.Words[language];
				return result;
			}
			result.Status = ResultStatus.Unsuccessful;
			return result;
		}
		public virtual ITranslationResult Translate(ITranslatable target, string language)
		{
			ITranslationResult result = null;
			foreach (KeyValuePair<string, Control> pair in target.TranslationTargets)
			{
				if (pair.Value is ITextControl)
				{
					ITextControl textControl = pair.Value as ITextControl;
					result = this.Translate(pair.Key, textControl.Text, language);
					if (result.IsSuccessful())
					{
						textControl.Text = result.Translation;
					}
				}
			}
			return result;
		}
		public virtual void CleanUp()
		{
			this.Storage.Close();
		}
		public virtual void Initialize()
		{
			if (string.IsNullOrEmpty(this._defaultLanguage))
			{
				throw new InvalidOperationException("DefaultLanguage not set.");
			}
			this.Storage.Open();
		}
	}
}
