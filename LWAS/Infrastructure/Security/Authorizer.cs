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
using System.Collections.Generic;
using System.Configuration;
using System.Web.Security;

using LWAS.Extensible.Interfaces;

namespace LWAS.Infrastructure.Security
{
	public class Authorizer : IAuthorizer
	{
		private string super_role = string.Empty;
		private Dictionary<string, List<string>> roles;
		public Authorizer()
		{
			this.super_role = ConfigurationManager.AppSettings["SUPER_ROLE"];
			if (!Roles.Enabled)
			{
				this.roles = new RoleManager().LoadRoles();
			}
		}
		protected virtual bool UserInRole(string user, string role)
		{
			bool result;
			if (!Roles.Enabled)
			{
				result = (this.roles.ContainsKey(role) && this.roles[role].Contains(user));
			}
			else
			{
				result = Roles.IsUserInRole(user, role);
			}
			return result;
		}
		public bool IsAuthorized(string filter)
		{
			return this.IsAuthorized(filter, User.CurrentUser.Name);
		}
		public bool IsAuthorized(string filter, string user)
		{
			bool result;
			if (string.IsNullOrEmpty(filter) || (!string.IsNullOrEmpty(this.super_role) && this.UserInRole(user, this.super_role)))
			{
				result = true;
			}
			else
			{
				string[] roles = filter.Split(new char[]
				{
					'|'
				});
				string[] array = roles;
				for (int i = 0; i < array.Length; i++)
				{
					string role = array[i];
					if (this.UserInRole(user, role))
					{
						result = true;
						return result;
					}
				}
				result = false;
			}
			return result;
		}

        public bool HasAccess(string screen)
        {
            return AccessVerifier.Instance.HasAccess(User.CurrentUser.Name, screen);
        }
	}
}
