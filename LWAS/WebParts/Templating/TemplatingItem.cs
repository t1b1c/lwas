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
using System.Web.UI.WebControls;

using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class TemplatingItem : ITemplatingItem
	{
		private bool _isNew = false;
		private bool _isReadOnly = true;
		private bool _isCurrent = false;
		private bool _isGrouping = false;
		private bool _isTotals = false;
		private bool _hasChanges = false;
		private bool _isValid = true;
		private object _data;
		private string _invalidMember;
		private IDictionary<string, List<BaseDataBoundControl>> _boundControls = new Dictionary<string, List<BaseDataBoundControl>>();
		public bool IsNew
		{
			get
			{
				return this._isNew;
			}
			set
			{
				this._isNew = value;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return this._isReadOnly;
			}
			set
			{
				this._isReadOnly = value;
			}
		}
		public bool IsCurrent
		{
			get
			{
				return this._isCurrent;
			}
			set
			{
				this._isCurrent = value;
			}
		}
		public bool IsGrouping
		{
			get
			{
				return this._isGrouping;
			}
			set
			{
				this._isGrouping = value;
			}
		}
		public bool IsTotals
		{
			get
			{
				return this._isTotals;
			}
			set
			{
				this._isTotals = value;
			}
		}
		public bool HasChanges
		{
			get
			{
				return this._hasChanges;
			}
			set
			{
				this._hasChanges = value;
			}
		}
		public bool IsValid
		{
			get
			{
				return this._isValid;
			}
			set
			{
				this._isValid = value;
			}
		}
		public object Data
		{
			get
			{
				return this._data;
			}
			set
			{
				this._data = value;
			}
		}
		public string InvalidMember
		{
			get
			{
				return this._invalidMember;
			}
			set
			{
				this._invalidMember = value;
			}
		}
		public IDictionary<string, List<BaseDataBoundControl>> BoundControls
		{
			get
			{
				return this._boundControls;
			}
		}
	}
}
