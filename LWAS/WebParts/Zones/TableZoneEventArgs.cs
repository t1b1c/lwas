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
using System.ComponentModel;

namespace LWAS.WebParts.Zones
{
	public class TableZoneEventArgs : CancelEventArgs
	{
		public string FirstPart;
		public string FirstRow;
		public string FirstCell;
		public string FirstContainer;
		public string SecondPart;
		public string SecondRow;
		public string SecondCell;
		public string SecondContainer;
		public TableZoneEventArgs(string firstPart, string firstRow, string firstCell, string firstContainer, string secondPart, string secondRow, string secondCell, string secondContainer)
		{
			this.FirstPart = firstPart;
			this.FirstRow = firstRow;
			this.FirstCell = firstCell;
			this.FirstContainer = firstContainer;
			this.SecondPart = secondPart;
			this.SecondRow = secondRow;
			this.SecondCell = secondCell;
			this.SecondContainer = secondContainer;
		}
		public TableZoneEventArgs(string[] args, int startIndex)
		{
			if (args.Length > startIndex)
			{
				this.FirstPart = args[startIndex];
			}
			if (args.Length > startIndex + 1)
			{
				this.FirstRow = args[startIndex + 1];
			}
			if (args.Length > startIndex + 2)
			{
				this.FirstCell = args[startIndex + 2];
			}
			if (args.Length > startIndex + 3)
			{
				this.FirstContainer = args[startIndex + 3];
			}
			if (args.Length > startIndex + 4)
			{
				this.SecondPart = args[startIndex + 4];
			}
			if (args.Length > startIndex + 5)
			{
				this.SecondRow = args[startIndex + 5];
			}
			if (args.Length > startIndex + 6)
			{
				this.SecondCell = args[startIndex + 6];
			}
			if (args.Length > startIndex + 7)
			{
				this.SecondContainer = args[startIndex + 7];
			}
		}
	}
}
