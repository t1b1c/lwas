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

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow
{
	public class TransitPoint : ITransitPoint
	{
		private IChronicler _chronicler;
		private string _member;
		private object _value;
		public IChronicler Chronicler
		{
			get
			{
				return this._chronicler;
			}
			set
			{
				this._chronicler = value;
			}
		}
		public string Member
		{
			get
			{
				return this._member;
			}
			set
			{
				this._member = value;
			}
		}
		public object Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}
	}
}
