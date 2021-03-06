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

namespace LWAS.Extensible.Exceptions
{
	public class WrongBindingItemTypeException : Exception
	{
		private string _message;
		public override string Message
		{
			get
			{
				return this._message;
			}
		}
		public WrongBindingItemTypeException(string expectedType, string receivedType)
		{
			this._message = string.Format("Binding item has wrong type. Expected {0} but received {1}", expectedType, receivedType);
		}
	}
}
