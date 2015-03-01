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
using System.Web;

namespace LWAS.Infrastructure.Security
{
	public class User
	{
		private string _rootPath;
		private string _name;
		public string RootPath
		{
			get
			{
				return this._rootPath;
			}
		}
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		public static User CurrentUser
		{
			get
			{
				return new User();
			}
		}
		public User()
		{
			this._name = HttpContext.Current.User.Identity.Name;
			this._rootPath = this._name;
		}
	}
}
