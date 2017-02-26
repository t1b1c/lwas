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
    public enum UploadToEnum { UploadRepo, UserRepo };
    
    public class Uploader2 : CompositeControl
    {
        private UploadToEnum _uploadTo;

        private FileUpload fileUpload;
        private LiteralControl literal1;
        private LiteralControl literal2;
        private LiteralControl literal3;
        private Button submitButton;
        private HiddenField fileNameHidden;
        private HiddenField filePathHidden;
        private HiddenField storageHidden;

        public string FileName
        {
            get { return null != fileNameHidden ? fileNameHidden.Value : null; }
            set
            {
                EnsureChildControls();
                fileNameHidden.Value = value;
            }
        }
        
        public string FilePath
        {
            get { return null != filePathHidden ? filePathHidden.Value : null; }
            set
            {
                EnsureChildControls();
                filePathHidden.Value = value;
            }
        }

        public UploadToEnum UploadTo
        {
            get { return _uploadTo; }
            set 
            { 
                _uploadTo = value;
                EnsureChildControls();
                storageHidden.Value = value.ToString();
            }
        }

        public bool UseUploadedFileName { get; set; }
        public bool RedirectAfterUpload { get; set; }

		public Uploader2()
		{
            _uploadTo = UploadToEnum.UploadRepo;
            this.UseUploadedFileName = false;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            fileNameHidden = new HiddenField();
            fileNameHidden.ID = "fileNameHidden";
            this.Controls.Add(fileNameHidden);

            filePathHidden = new HiddenField();
            filePathHidden.ID = "filePathHidden";
            this.Controls.Add(filePathHidden);

            storageHidden = new HiddenField();
            storageHidden.ID = "storageHidden";
            storageHidden.Value = _uploadTo.ToString();
            this.Controls.Add(storageHidden);

            literal1 = new LiteralControl();
            literal1.Text = "";
            this.Controls.Add(literal1);

            fileUpload = new FileUpload();
            fileUpload.ID = "fileUpload";
            fileUpload.Style.Add(HtmlTextWriterStyle.Display, "none");
            this.Controls.Add(fileUpload);

            literal2 = new LiteralControl();
            literal2.Text = "";
            this.Controls.Add(literal2);

            submitButton = new Button();
            submitButton.CssClass = "uploader-submit";
            submitButton.Style.Add(HtmlTextWriterStyle.Display, "none");
            submitButton.CommandName = "upload complete";
            submitButton.Click += submitButton_Click;
            this.Controls.Add(submitButton);

            literal3 = new LiteralControl();
            literal3.Text = "";
            this.Controls.Add(literal3);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
        }

        void submitButton_Click(object sender, EventArgs e)
        {
            if (this.RedirectAfterUpload)
                Redirect();
            else
                RaiseBubbleEvent(sender, e);
        }

        private void Redirect()
        {
            string url = this.Page.Request.Url.ToString();
            string script = @"window.location.href = '" + url + @"';";
            ScriptManager.RegisterClientScriptBlock(this, typeof(Uploader2), "redirect on upload", script, true);
        }

        void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                fileUpload.Visible = false;
                submitButton.Visible = false;
            }
            else
            {
                fileUpload.Visible = true;
                submitButton.Visible = true;

                // jasny bootstrap file upload framing
                literal1.Text = @"
<div class=""fileinput fileinput-new input-group"" data-provides=""fileinput"">
	<div class=""form-control"" data-trigger=""fileinput"">
		<i class=""glyphicon glyphicon-file fileinput-exists""></i> 
		<span class=""fileinput-filename""></span>
	</div>
	<span class=""input-group-addon btn btn-default btn-file"">
		<span class=""fileinput-new"">Select file</span>
                ";

                literal2.Text = @"
    </span>
                ";

                // jquery File Upload progress bar
                literal3.Text = @"
</div>
<div id=""progress"" class=""progress"">
    <div class=""progress-bar progress-bar-success""></div>
</div>
                ";
            }
        }
    }
}
