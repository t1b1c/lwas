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
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;

namespace LWAS.CustomControls
{
	public class Uploader : CompositeControl
	{
        public class UploaderEventArgs : EventArgs
        {
            public string FileName { get; set; }
            public UploaderEventArgs(string fileName)
            {
                this.FileName = fileName;
            }
        }
        public enum UploadToEnum { UploadRepo, UserRepo };

		private AsyncFileUpload uploader;
		private DirectoryContainer container;
		private static string _storagePath;
        private string _userStorage;
        private UploadToEnum _uploadTo;
        public event EventHandler<UploaderEventArgs> FileUploaded;
        static object SyncRoot = new object();

        public string FileName { get; set; }        
        public string FilePath { get; set; }

        public UploadToEnum UploadTo
        {
            get { return _uploadTo; }
            set { _uploadTo = value; }
        }

        public string UserStorage
        {
            get
            {
                if (String.IsNullOrEmpty(_userStorage))
                {
                    string userdata = ConfigurationManager.AppSettings["USER_DATA"];
                    if (String.IsNullOrEmpty(userdata)) throw new ApplicationException("Config key 'USER_DATA' not set");
                    _userStorage = HttpContext.Current.Server.MapPath(Path.Combine(userdata, User.CurrentUser.RootPath));
                }
                return _userStorage;
            }
        }
        public string StoragePath
        {
            get
            {
                if (_uploadTo == UploadToEnum.UploadRepo)
                    return Uploader._storagePath;
                else
                    return UserStorage;
            }
        }

        public bool UseUploadedFileName { get; set; }
        public bool RedirectAfterUpload { get; set; }

		static Uploader()
		{
			Uploader._storagePath = ConfigurationManager.AppSettings["UPLOADS_REPO"];
			if (string.IsNullOrEmpty(Uploader._storagePath))
                throw new ApplicationException("UPLOADS_REPO not set");
		}
		public Uploader()
		{
            _uploadTo = UploadToEnum.UploadRepo;
            this.UseUploadedFileName = false;
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			this.uploader = new AsyncFileUpload();
			this.uploader.ID = "uploader";
			this.Controls.Add(this.uploader);
            this.uploader.Width = this.Width;
			this.uploader.UploadedComplete += new EventHandler<AsyncFileUploadEventArgs>(this.uploader_UploadedComplete);
		}

        protected override void OnInit(EventArgs e)
        {
            isOnInit = true;

            base.OnInit(e);
            EnsureChildControls();

            this.FileName = uploader.FileName;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            isOnInit = false;

            if (this.RedirectAfterUpload)
                Redirect();
        }

        private void Redirect()
        {
            string url = this.Page.Request.Url.ToString();
            string script = @"function uploadComplete() { window.location.href = '" + url + @"'; }";
            ToolkitScriptManager.RegisterClientScriptBlock(this, typeof(Uploader), "redirect on upload", script, true);

            uploader.OnClientUploadComplete = "uploadComplete";
        }

        bool isOnInit = false;
		private void uploader_UploadedComplete(object sender, AsyncFileUploadEventArgs e)
        {
            if (isOnInit)
                return;

            string key = "";
            lock (SyncRoot)
            {
                if (this.UseUploadedFileName)
                {
                    this.FileName = Path.GetFileName(e.FileName);
                    key = this.FileName;
                }
                else
                    key = this.FileName + Path.GetExtension(e.FileName);

                if (!string.IsNullOrEmpty(this.FileName) && !string.IsNullOrEmpty(this.StoragePath))
                {
                    string container_path = this.FilePath;
                    if (container_path.StartsWith("/"))
                        container_path = container_path.Remove(0, 1);
                    if (!String.IsNullOrEmpty(container_path))
                        container = new DirectoryContainer(this.StoragePath, container_path);
                    else
                        container = new DirectoryContainer(null, this.StoragePath);

                    try
                    {
                        if (container.Agent.HasKey(key))
                            container.Agent.Erase(key);
                        ((FileAgent)this.container.Agent).Write(key, this.uploader.FileContent);
                    }
                    catch (Exception ex)
                    {
                        this.container.Agent.CloseStream(key);
                        throw ex;
                    }
                }
                this.uploader.ClearFileFromPersistedStore();
            }

            if (null != this.FileUploaded)
                FileUploaded(this, new UploaderEventArgs(key));

            RaiseBubbleEvent(this, new CommandEventArgs("upload complete", key));
		}
		protected override void Render(HtmlTextWriter writer)
		{
			if (!string.IsNullOrEmpty(this.FileName) || this.UseUploadedFileName)
			{
				base.Render(writer);
			}
		}
	}
}
