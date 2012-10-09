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
using System.Collections;
using System.Collections.Generic;

using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow
{
	public class TransitsCollection : LinkedList<ITransit>, ITransitsCollection, ICollection<ITransit>, IEnumerable<ITransit>, IEnumerable
	{
		public virtual bool Run()
		{
			LinkedListNode<ITransit> transitNode = base.First;
			while (null != transitNode)
			{
				transitNode.Value.Run();
				transitNode = transitNode.Next;
			}
			return true;
		}

        public int IndexOf(ITransit transit)
        {
            int index = 0;
            LinkedListNode<ITransit> test = this.First;
            while (null != test)
            {
                if (transit == test.Value)
                    return index;
                else
                    test = test.Next;
                index++;
            }
            return -1;
        }
    }
}
