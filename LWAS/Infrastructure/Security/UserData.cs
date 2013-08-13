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
using System.Configuration;
using System.IO;
using System.Web;

namespace LWAS.Infrastructure.Security
{
	public class UserData
	{
		protected const string USER_DATA = "USER_DATA";
		
        public static UserData Instance = new UserData();

        public string RootPath
		{
			get
			{
                string path = "";
                path = ConfigurationManager.AppSettings[UserData.USER_DATA];
                path = Path.Combine(path, User.CurrentUser.RootPath);
                path = HttpContext.Current.Server.MapPath(path);
                return path;
			}
		}
		public UserData()
		{
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[UserData.USER_DATA])) throw new InvalidOperationException(string.Format("'{0}' setting not set", UserData.USER_DATA));

		}
	}
}
