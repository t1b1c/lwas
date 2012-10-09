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
using System.Web.UI;
using System.Web.UI.WebControls;
using Classic = System.Web.UI.WebControls;
using AjaxControlToolkit;

namespace LWAS.CustomControls
{
	public class Menu : CompositeControl
	{
        WebControl label;
        Classic.BulletedList commandsList;
        DropDownExtender dropDownExtender;

        public string Label
        {
            get 
            {
                EnsureChildControls();
                if (label is Label)
                    return ((Label)label).Text;
                else if (label is LinkButton)
                    return ((LinkButton)label).Text;
                return null;
            }
            set 
            {
                EnsureChildControls();
                if (label is Label)
                    ((Label)label).Text = value;
                else if (label is LinkButton)
                    ((LinkButton)label).Text = value;
            }
        }
        public ListItemCollection Commands
        {
            get 
            {
                EnsureChildControls();
                return commandsList.Items; 
            }
        }
        public new bool Enabled
        {
            get 
            {
                EnsureChildControls();
                return dropDownExtender.Enabled; 
            }
            set 
            {
                EnsureChildControls();
                dropDownExtender.Enabled = value; 
            }
        }

        public override Unit Width
        {
            get
            {
                EnsureChildControls();
                return label.Width;
            }
            set
            {
                EnsureChildControls();
                label.Width = value;
            }
        }

        public override Unit Height
        {
            get
            {
                EnsureChildControls();
                return label.Height;
            }
            set
            {
                EnsureChildControls();
                label.Height = value;
            }
        }

        public BulletedListDisplayMode DisplayMode { get; set; }
        public event EventHandler<MenuEventArgs> MenuClick;
        public string Argument { get; set; }

        public Menu()
        {
            this.DisplayMode = BulletedListDisplayMode.HyperLink;
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

            if (this.DisplayMode == BulletedListDisplayMode.HyperLink)
            {
                label = new Label();
                label.ID = "label";
                label.CssClass = "menu_label";
            }
            else
            {
                label = new LinkButton();
                label.ID = "label";
                label.CssClass = "menu_link";
                ((LinkButton)label).Click += (s, e) =>
                    {
                        MenuClick(this, new MenuEventArgs()
                        {
                            CommandValue = this.Argument,
                            CommandText = ((LinkButton)label).Text
                        });
                    };
            }
            this.Controls.Add(label);

            commandsList = new Classic.BulletedList();
            commandsList.ID = "commandsList";
            commandsList.CssClass = "menu_commands";
            commandsList.Style.Add("display", "none");
            commandsList.DisplayMode = this.DisplayMode;
            if (this.DisplayMode == BulletedListDisplayMode.LinkButton)
            {
                commandsList.Click += (s, e) =>
                    {
                        ListItem item = commandsList.Items[e.Index];
                        if (null != this.MenuClick && null != item)
                            MenuClick(this, new MenuEventArgs()
                                            {
                                                CommandValue = item.Value,
                                                CommandText = item.Text
                                            });
                    };
            }
            this.Controls.Add(commandsList);

            dropDownExtender = new DropDownExtender();
            dropDownExtender.ID = "dropDownExtender";
            //dropDownExtender.BehaviorID = this.ID + "Behaviour";
            dropDownExtender.TargetControlID = "label";
            dropDownExtender.DropDownControlID = "commandsList";
            this.Controls.Add(dropDownExtender);
        }
	}

    public class MenuEventArgs : EventArgs
    {
        public string CommandValue;
        public string CommandText;
    }
}
