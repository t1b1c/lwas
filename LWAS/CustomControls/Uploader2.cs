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
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

namespace LWAS.CustomControls
{
    public class Uploader2 : CompositeControl
    {
        private FileUpload fileUpload;
        private HiddenField fileNameHidden;
        private HiddenField filePathHidden;
        Button hiddenCommand;
        private UploadToEnum _uploadTo;

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
            set { _uploadTo = value; }
        }
        public bool UseUploadedFileName { get; set; }
        public bool RedirectAfterUpload { get; set; }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            fileUpload = new FileUpload();
            fileUpload.ID = "fileUpload";
            this.Controls.Add(fileUpload);

            fileNameHidden = new HiddenField();
            fileNameHidden.ID = "fileNameHidden";
            this.Controls.Add(fileNameHidden);

            filePathHidden = new HiddenField();
            filePathHidden.ID = "filePathHidden";
            this.Controls.Add(filePathHidden);
        
            hiddenCommand = new Button();
            hiddenCommand.ID = "hiddenCommand";
            hiddenCommand.CommandName = "upload complete";
            hiddenCommand.Style.Add(HtmlTextWriterStyle.Display, "none");
            this.Controls.Add(hiddenCommand);
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

        void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (!this.Enabled)
            {
                fileUpload.Visible = false;
            }
            else
            {
                fileUpload.Visible = true;
                string path = this.Page.ResolveUrl("~");
                string cookie = this.Page.Request.Cookies[FormsAuthentication.FormsCookieName] == null ? string.Empty : this.Page.Request.Cookies[FormsAuthentication.FormsCookieName].Value;

                string script = @"
    var UploadifyAuthCookie = '" + this.Page.Server.UrlEncode(cookie) + @"';
    var UploadifySessionId = '" + this.Page.Server.UrlEncode(this.Page.Session.SessionID) + @"';

    $(""#" + fileUpload.ClientID + @""").uploadify({
        'swf': '" + path + @"scripts/uploadify.swf',
        'cancelImg': '" + path + @"scripts/uploadify-cancel.png',
        'buttonText': 'Incarca...',
        'uploader': '" + path + @"handlers/Upload.ashx?filePath=" + this.Page.Server.UrlEncode(this.FilePath) + @"&storage=" + this.UploadTo + @"',
        'multi': false,
        'formData': { 'RequireUploadifySessionSync': true, 'SecurityToken': UploadifyAuthCookie, 'SessionId': UploadifySessionId }, 
        'auto': true,
        'onUploadComplete' : function(file) {
            $(""#" + fileNameHidden.ClientID + @""").val(file.name);
            $(""#" + hiddenCommand.ClientID + @""").click();
        }
    });
    ";

                ToolkitScriptManager.RegisterClientScriptBlock(this, typeof(Uploader2), this.ID, script, true);
            }
        }
    }
}
