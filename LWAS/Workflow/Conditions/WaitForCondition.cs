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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WorkFlow;

namespace LWAS.WorkFlow.Conditions
{
	public class WaitForCondition : Condition, ITrigger
	{
		private string _milestone;
		private IChronicler _chronicler;
		private bool IsMilestoneReached = false;
		private ITriggerable _target;
		public string Milestone
		{
			get
			{
				return this._milestone;
			}
			set
			{
				this._milestone = value;
			}
		}
		public IChronicler Chronicler
		{
			get
			{
				return this._chronicler;
			}
			set
			{
				this.Reset();
				this._chronicler = value;
				if (null != this._chronicler)
				{
					this._chronicler.MilestoneHandler += new MilestoneEventHandler(this.Chronicler_Milestone);
				}
			}
		}
		public ITriggerable Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
			}
		}
		public override string Title
		{
			get
			{
				string result;
				if (string.IsNullOrEmpty(base.Title))
				{
					string title = "";
					if (null != this._chronicler)
					{
                        if (_chronicler is WebPart)
                            title = ((WebPart)_chronicler).ID;
                        else
                            title = this._chronicler.Title;
                    }
                    if (!string.IsNullOrEmpty(this._milestone))
                    {
                        title = title + "." + this._milestone;
                    }
					result = title;
				}
				else
				{
					result = base.Title;
				}
				return result;
			}
			set
			{
				base.Title = value;
			}
		}
		protected virtual void Reset()
		{
			if (null != this._chronicler)
			{
				this._chronicler.MilestoneHandler -= new MilestoneEventHandler(this.Chronicler_Milestone);
			}
			this.IsMilestoneReached = false;
		}
		private void Chronicler_Milestone(IChronicler chronicler, string key)
		{
			if (key == this._milestone)
			{
				this.IsMilestoneReached = true;
				this.OnPassed(EventArgs.Empty);
			}
		}
		protected override void OnPassed(EventArgs e)
        {
            if (null != this.Monitor)
            {
                this.Monitor.Register(this, this.Monitor.NewEventInstance("waitfor passed", EVENT_TYPE.Trace));
            }
			if (null != this._target)
			{
				this._target.OnTrigger(this);
			}
			base.OnPassed(e);
		}
		public override bool Check()
		{
			return base.Check() && this.IsMilestoneReached;
		}
	}
}
