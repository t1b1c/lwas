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
using System.Collections;
using System.Collections.Generic;

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow
{
	public class ConditionsCollection : LinkedList<ICondition>, IConditionsCollection, ICollection<ICondition>, IEnumerable<ICondition>, IEnumerable
	{
		private ConditionsCollectionType _type = ConditionsCollectionType.And;
		public ConditionsCollectionType Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}
		public virtual bool Check()
		{
			LinkedListNode<ICondition> conditionNode = base.First;
			bool result;
			while (null != conditionNode)
			{
				bool test = conditionNode.Value.Check();
				if (this._type == ConditionsCollectionType.And && !test)
				{
					result = false;
				}
				else
				{
					if (this._type != ConditionsCollectionType.Or || !test)
					{
						conditionNode = conditionNode.Next;
						continue;
					}
					result = true;
				}
				return result;
			}
			result = (this._type == ConditionsCollectionType.And);
			return result;
		}
	}
}
