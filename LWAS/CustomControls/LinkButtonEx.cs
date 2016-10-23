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
	public class LinkButtonEx : LinkButton
	{
		public bool ReadOnly { get; set; }
        public override Unit Width
        {
            get { return base.Width; }
            set { ; }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.ReadOnly)
                return;

            base.Render(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(@"<span class='glyphicon glyphicon-link'></span> ");
            writer.Write(@"<span>" + this.Text + "</span> ");
        }

    }
}
