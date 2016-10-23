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

namespace LWAS.CustomControls
{
	public class OneClickButton : LinkButton
	{
		protected override void OnPreRender(EventArgs e)
		{
			string script = "this.class += ' disabled'";
			this.OnClientClick = script;
            //this.UseSubmitBehavior = false;

            base.OnPreRender(e);
		}

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.CommandName.Contains("save"))
                this.CssClass += " btn-success";
            else if (this.CommandName.Contains("delete"))
                this.CssClass += " btn-danger";

                base.Render(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.CommandName.Contains("save"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-floppy-disk'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'>" + this.Text + "</span> ");
            }
            else if (this.CommandName.Contains("insert"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-plus-sign'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'>" + this.Text + "</span> ");
            }
            else if (this.CommandName.Contains("view"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-refresh'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'>" + this.Text + "</span> ");
            }
            else if (this.CommandName.Contains("delete"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-remove'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'>" + this.Text + "</span> ");
            }
            else if (this.CommandName.Contains("edit"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-edit'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'>" + this.Text + "</span> ");
            }
            else if (this.CommandName.Equals("select"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-edit'></span> ");
                writer.Write(@"<span class='hidden-xs hidden-sm'></span> ");
            }
            else if (this.CommandName.Contains("paginater:first"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-step-backward'></span>");
            }
            else if (this.CommandName.Contains("paginater:left"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-backward'></span>");
            }
            else if (this.CommandName.Contains("paginater:right"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-forward'></span>");
            }
            else if (this.CommandName.Contains("paginater:last"))
            {
                writer.Write(@"<span class='glyphicon glyphicon-step-forward'></span>");
            }
            else
                base.RenderContents(writer);
        }
    }
}
