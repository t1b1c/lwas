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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace LWAS.CustomControls
{
	public class Menu : CompositeControl, IButtonControl
	{
        HtmlButton label;
        Panel container;
        BulletedList commandsList = new BulletedList();

        public ListItemCollection Commands
        {
            get { return commandsList.Items; }
        }

        public BulletedListDisplayMode DisplayMode { get; set; }
        public event EventHandler<MenuEventArgs> MenuClick;

        public event EventHandler Click;
        public event CommandEventHandler Command;

        public string CommandArgument { get; set; }
        public string CommandName
        {
            get { return this.Text; }
            set { this.Text = value; }
        }
        public string PostBackUrl { get; set; }
        string _text;
        public string Text 
        {
            get { return _text; }
            set
            {
                _text = value;
                if (this.ChildControlsCreated)
                {
                    label.InnerHtml = _text + @" <span class=""caret""></span>";
                }
            }
        }
        public string ValidationGroup { get; set; }
        public bool CausesValidation { get; set; }

        public string Label
        {
            get { return this.Text; }
            set { this.Text = value; }
        }

        public string Argument { get; set; }
        public bool BubblesMilestones { get; set; }
        public bool ActiveLabel { get; set; }
        public Unit LabelWidth { get; set; }
        public Unit LabelHeight { get; set; }
        public override bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; }
        }

        public Menu()
        {
            this.DisplayMode = BulletedListDisplayMode.HyperLink;
            this.BubblesMilestones = false;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.CssClass = "menu";
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            container = new Panel();
            container.ID = "container";
            container.CssClass = "dropdown";
            container.Width = this.Width;
            container.Height = this.Height;
            this.Controls.Add(container);

            label = new HtmlButton();
            label.ID = "label";
            if (this.DisplayMode == BulletedListDisplayMode.HyperLink || !this.ActiveLabel)
            {
            }
            else
            {
                label.ServerClick += (s, e) =>
                    {
                        if (!this.BubblesMilestones)
                            OnMenuClick(this.Label, this.Argument);
                        else
                            OnMenuClick(this.CommandName, this.CommandArgument);
                    };
            }
            label.Attributes["class"] = "btn btn-default dropdown-toggle";
            label.Attributes["type"] = "button";
            label.Attributes["data-toggle"] = "dropdown";
            label.Attributes["aria-haspopup"] = "true";
            label.Attributes["aria-expanded"] = "false";
            label.InnerHtml = this.Label + @" <span class=""caret""></span>";
            container.Controls.Add(label);

            commandsList.ID = "commandsList";
            commandsList.CssClass = "dropdown-menu";
            commandsList.Attributes.Add("role", "menu");
            commandsList.Attributes.Add("aria-labelledby", label.ClientID);
            commandsList.DisplayMode = this.DisplayMode;
            if (this.DisplayMode == BulletedListDisplayMode.LinkButton)
            {
                commandsList.Click += (s, e) =>
                    {
                        ListItem item = commandsList.Items[e.Index];
                        if (null != item)
                        {
                            if (!this.BubblesMilestones)
                                OnMenuClick(item.Text, item.Value);
                            else
                                OnMenuClick(item.Value, this.CommandArgument);
                        }
                    };
            }
            container.Controls.Add(commandsList);
        }

        void OnMenuClick(string command, string argument)
        {
            if (null != this.Click)
                Click(this, new CommandEventArgs(command, argument));
            if (null != this.MenuClick)
                MenuClick(this, new MenuEventArgs() { CommandText = command, CommandValue = argument });
            if (null != this.Command)
                Command(this, new CommandEventArgs(command, argument));
            
            if (this.BubblesMilestones)
                RaiseBubbleEvent(this, new CommandEventArgs(command, argument));
        }
    }

    public class MenuEventArgs : EventArgs
    {
        public string CommandValue;
        public string CommandText;
    }
}
