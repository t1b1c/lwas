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
using System.IO;
using System.Xml;
using System.Text;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Translation
{
	public class StringsStorage
	{
        string strings;
		private IStorageAgent _agent;
		private string _filePath;
		private string _paramsSeparator;
		public IStorageAgent Agent
		{
			get { return this._agent; }
			set { this._agent = value; }
		}
		public string FilePath
		{
			get { return this._filePath; }
			set { this._filePath = value; }
		}
		public string ParamsSeparator
		{
			get { return this._paramsSeparator; }
			set { this._paramsSeparator = value; }
		}
		public virtual void Open()
		{
			if (null == this._agent) throw new InvalidOperationException("Agent not set");
			if (string.IsNullOrEmpty(this._filePath)) throw new InvalidOperationException("Unknown file path");

			strings = this._agent.Read(this._filePath);
		}
		public virtual StringsStorageEntry GetEntryByKey(string key, string source)
		{
			if (null == this._agent) throw new InvalidOperationException("Agent not set");

			StringsStorageEntry result;
			if (string.IsNullOrEmpty(key))
				result = null;
			else
			{
				StringsStorageEntry entry = null;
                if (String.IsNullOrEmpty(strings))
                    Open();

                XmlReader reader = XmlReader.Create(new StringReader(strings));
                if (reader.ReadToFollowing(key))
                {
                    entry = new StringsStorageEntry();
                    entry.Key = key;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                            entry.Words.Add(reader.Name, this.GetFormatedWord(reader.Value, source));
                        }
                    }
                }
				result = entry;
			}
			return result;
		}
		public virtual StringsStorageEntry GetEntryByWord(string word)
		{
			if (null == this._agent)
			{
				throw new InvalidOperationException("Agent not set");
			}
			StringsStorageEntry result = null;
			if (string.IsNullOrEmpty(word))
			{
				result = null;
			}
			else
			{
				XmlReader reader = XmlReader.Create(new StringReader(strings));
				string compare = this.GetFormat(word);
				while (reader.Read())
				{
					if (reader.HasAttributes)
					{
                        StringsStorageEntry entry = new StringsStorageEntry();
                        entry.Key = reader.Name;
						while (reader.MoveToNextAttribute())
                        {
                            entry.Words.Add(reader.Name, this.GetFormatedWord(reader.Value, word));
							if (reader.Value == compare)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    entry.Words.Add(reader.Name, this.GetFormatedWord(reader.Value, word));
                                }
								result = entry;
							}
						}
					}
				}
			}
			return result;
		}
		public virtual string GetFormatedWord(string word, string source)
		{
			string[] parameters = null;
			if (!string.IsNullOrEmpty(this._paramsSeparator) && !string.IsNullOrEmpty(source))
			{
				string[] tokens = source.Split(this._paramsSeparator.ToCharArray());
				if (tokens.Length > 1)
				{
					parameters = tokens[1].Split(new char[]
					{
						','
					});
				}
			}
			string result;
			if (null == parameters)
			{
				result = word;
			}
			else
			{
				try
				{
					result = string.Format(word, parameters);
				}
				catch
				{
					result = word;
				}
			}
			return result;
		}
		public virtual string GetFormat(string word)
		{
			string[] tokens = null;
			if (!string.IsNullOrEmpty(this._paramsSeparator))
			{
				tokens = word.Split(this._paramsSeparator.ToCharArray());
			}
			string result;
			if (null == tokens)
			{
				result = word;
			}
			else
			{
				result = tokens[0];
			}
			return result;
		}
		public virtual void Close()
		{
            strings = null;
		}
	}
}
