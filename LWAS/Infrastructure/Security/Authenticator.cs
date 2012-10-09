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
using System.Configuration;
using System.Web.Security;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Security
{
	public class Authenticator
	{
		private IStorageAgent _agent;
		public static Authenticator Instance;
		public IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
			set
			{
				this._agent = value;
			}
		}
		static Authenticator()
		{
			Authenticator.Instance = new Authenticator();
		}
		public bool Verify(string user, string pwd)
		{
			if (null == this._agent)
			{
				throw new InvalidOperationException("Agent not set");
			}
			string key = ConfigurationManager.AppSettings["USERS"];
			if (string.IsNullOrEmpty(key))
			{
				throw new InvalidOperationException("USERS key not set in config file");
			}
			XmlDocument doc = new XmlDocument();
			string hash = FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "SHA1");
			try
			{
				doc.Load(this._agent.OpenStream(key));
			}
			finally
			{
				this._agent.CloseStream(key);
			}
			XmlNode root = doc.SelectSingleNode("users");
			if (null == root)
			{
				throw new InvalidOperationException("Users files has no users node");
			}
			bool result;
			foreach (XmlNode node in root.ChildNodes)
			{
				if (node.Attributes["name"].Value == user && node.Attributes["pwd"].Value == hash)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
	}
}
