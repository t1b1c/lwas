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
	public class Paginater : Panel, INamingContainer
	{
		private string _ofText = "/";
		private int _pageSize;
		private int _currentPage;
		private int _resultsCount;
		private TableStyle _tableStyle = new TableStyle();
		private Style _labelStyle = new Style();
		private bool _isFrozen = false;
		private UpdatePanel updatePanel;
		private Label currentLabel;
		private Label ofLabel;
		private Label totalLabel;
		private Button firstButton;
		private Button backwardButton;
		private Button forwardButton;
		private Button lastButton;
		private HiddenField hiddenResultsCount;
		private HiddenField hiddenCurrentPage;
		public event EventHandler Changed;
		public string OfText
		{
			get
			{
				return this._ofText;
			}
			set
			{
				this._ofText = value;
			}
		}
		public int PageSize
		{
			get
			{
				return this._pageSize;
			}
			set
			{
				if (value < 0)
				{
					throw new InvalidOperationException("PageSize must be a positive integer");
				}
				this._pageSize = value;
			}
		}
		public int StartIndex
		{
			get
			{
				int result;
				if (this._currentPage < 1)
				{
					result = 0;
				}
				else
				{
					result = this._pageSize * (this._currentPage - 1);
				}
				return result;
			}
		}
		public int CurrentPage
		{
			get
			{
				return this._currentPage;
			}
			set
			{
				this._currentPage = value;
				this.hiddenCurrentPage.Value = this._currentPage.ToString();
				this.currentLabel.Text = this._currentPage.ToString();
			}
		}
		public int ResultsCount
		{
			get
			{
				return this._resultsCount;
			}
			set
			{
				this._resultsCount = value;
				this.hiddenResultsCount.Value = this._resultsCount.ToString();
				if (this._resultsCount == 0)
				{
					this.CurrentPage = 0;
				}
				else
				{
					if (this._resultsCount > 0 && this._currentPage == 0)
					{
						this.CurrentPage = 1;
					}
				}
				this.totalLabel.Text = this.PagesCount.ToString();
			}
		}
		public int PagesCount
		{
			get
			{
				int result;
				if (this._pageSize == 0)
				{
					if (this._resultsCount == 0)
					{
						result = 0;
					}
					else
					{
						result = 1;
					}
				}
				else
				{
					result = (int)Math.Ceiling((double)this._resultsCount / (double)this._pageSize);
				}
				return result;
			}
		}
		private TableStyle TableStyle
		{
			get
			{
				return this._tableStyle;
			}
			set
			{
				this._tableStyle = value;
			}
		}
		public Style LabelStyle
		{
			get
			{
				return this._labelStyle;
			}
			set
			{
				this._labelStyle = value;
			}
		}
		public virtual bool IsFrozen
		{
			get
			{
				return this._isFrozen;
			}
			set
			{
				this._isFrozen = value;
				this.firstButton.Visible = !this._isFrozen;
				this.backwardButton.Visible = !this._isFrozen;
				this.forwardButton.Visible = !this._isFrozen;
				this.lastButton.Visible = !this._isFrozen;
			}
		}
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			this.updatePanel = new UpdatePanel();
			this.updatePanel.ID = "updatePanel";
			Table table = new Table();
			table.ApplyStyle(this._tableStyle);
			TableRow tableRow = new TableRow();
			TableCell tableCell = new TableCell();
			this.hiddenResultsCount = new HiddenField();
			this.hiddenResultsCount.ID = "hiddenResultsCount";
			tableCell.Controls.Add(this.hiddenResultsCount);
			this.hiddenCurrentPage = new HiddenField();
			this.hiddenCurrentPage.ID = "hiddenCurrentPage";
			tableCell.Controls.Add(this.hiddenCurrentPage);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.firstButton = new Button();
			this.firstButton.CssClass = "paginater_first";
			this.firstButton.Click += new EventHandler(this.firstButton_Click);
			tableCell.Controls.Add(this.firstButton);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.backwardButton = new Button();
			this.backwardButton.CssClass = "paginater_left";
			this.backwardButton.Click += new EventHandler(this.backwardButton_Click);
			tableCell.Controls.Add(this.backwardButton);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.currentLabel = new Label();
			this.currentLabel.ApplyStyle(this._labelStyle);
			this.currentLabel.Text = "0";
			tableCell.Controls.Add(this.currentLabel);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.ofLabel = new Label();
			this.ofLabel.ApplyStyle(this._labelStyle);
			this.ofLabel.Text = this._ofText;
			tableCell.Controls.Add(this.ofLabel);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.totalLabel = new Label();
			this.totalLabel.ApplyStyle(this._labelStyle);
			this.totalLabel.Text = "0";
			tableCell.Controls.Add(this.totalLabel);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.forwardButton = new Button();
			this.forwardButton.CssClass = "paginater_right";
			this.forwardButton.Click += new EventHandler(this.forwardButton_Click);
			tableCell.Controls.Add(this.forwardButton);
			tableRow.Controls.Add(tableCell);
			tableCell = new TableCell();
			this.lastButton = new Button();
			this.lastButton.CssClass = "paginater_last";
			this.lastButton.Click += new EventHandler(this.lastButton_Click);
			tableCell.Controls.Add(this.lastButton);
			tableRow.Controls.Add(tableCell);
			table.Rows.Add(tableRow);
			this.updatePanel.ContentTemplateContainer.Controls.Add(table);
			this.Controls.Add(this.updatePanel);
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!string.IsNullOrEmpty(this.hiddenCurrentPage.Value))
			{
				int.TryParse(this.hiddenCurrentPage.Value, out this._currentPage);
				Label arg_49_ = this.currentLabel;
				int num = this.CurrentPage;
				arg_49_.Text = num.ToString();
			}
			if (!string.IsNullOrEmpty(this.hiddenResultsCount.Value))
			{
				int.TryParse(this.hiddenResultsCount.Value, out this._resultsCount);
				Label arg_90_ = this.totalLabel;
				int num = this.PagesCount;
				arg_90_.Text = num.ToString();
			}
		}
		private void firstButton_Click(object sender, EventArgs e)
		{
			this.OnFirst();
		}
		private void forwardButton_Click(object sender, EventArgs e)
		{
			this.OnForward();
		}
		private void backwardButton_Click(object sender, EventArgs e)
		{
			this.OnBackward();
		}
		private void lastButton_Click(object sender, EventArgs e)
		{
			this.OnLast();
		}
		public virtual void OnFirst()
		{
			if (!this._isFrozen)
			{
				this.CurrentPage = 1;
				if (null != this.Changed)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
        public virtual void OnForward()
		{
			if (!this._isFrozen)
			{
				if (this.PagesCount > 1)
				{
					if (this._currentPage < this.PagesCount)
					{
						this.CurrentPage++;
					}
					if (null != this.Changed)
					{
						this.Changed(this, EventArgs.Empty);
					}
				}
			}
		}
        public virtual void OnBackward()
		{
			if (!this._isFrozen)
			{
				if (this.PagesCount > 1)
				{
					if (this._currentPage > 0)
					{
						this.CurrentPage--;
					}
					if (null != this.Changed)
					{
						this.Changed(this, EventArgs.Empty);
					}
				}
			}
		}
        public virtual void OnLast()
		{
			if (!this._isFrozen)
			{
				if (this.PagesCount > 1)
				{
					this.CurrentPage = this.PagesCount;
					if (null != this.Changed)
					{
						this.Changed(this, EventArgs.Empty);
					}
				}
			}
		}
		public virtual void Reset()
		{
			this.ResultsCount = 0;
		}
	}
}
