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
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Infrastructure;
using LWAS.WebParts.Templating;

namespace LWAS.WebParts.Zones
{
	public class TableZone : BaseZone
	{
		public const string TABLE_SUFIX = "Table";
		public const string CONTAINER_SUFIX = "Container";
		public bool EnableConfigurationProvider = true;
		private Dictionary<string, string> _containersParts = new Dictionary<string, string>();
		private RenderlessTable _table;
		private TableStyle _tableStyle = new TableStyle();
		private TableItemStyle _rowStyle = new TableItemStyle();
		private TableItemStyle _cellStyle = new TableItemStyle();
		private Style _containerStyle = new Style();
		public event EventHandler ConfigurationLoading;
		public event EventHandler ConfigurationLoaded;
		public event EventHandler ConfigurationSaving;
		public event EventHandler ConfigurationSaved;
		public event EventHandler<TableZoneEventArgs> MoveEvent;
		public event EventHandler<TableZoneEventArgs> SwapEvent;
		public event EventHandler<TableZoneEventArgs> AddEvent;
		public event EventHandler<TableZoneEventArgs> ListEvent;
		public event EventHandler<TableZoneEventArgs> CreateContainerEvent;
		public event EventHandler<TableZoneEventArgs> RemoveEvent;
		public event EventHandler<TableZoneEventArgs> EditEvent;
		public event EventHandler<TableZoneEventArgs> InsertEvent;
		protected Dictionary<string, string> ContainersParts
		{
			get
			{
				return this._containersParts;
			}
		}
		public RenderlessTable Table
		{
			get
			{
				if (null == this._table)
				{
					this.InitTable();
				}
				return this._table;
			}
		}
		public override List<WebPart> PartsNotShown
		{
			get
			{
				List<WebPart> list = new List<WebPart>();
				foreach (WebPart part in base.WebParts)
				{
					if (!this._containersParts.ContainsValue(part.ID))
					{
						list.Add(part);
					}
				}
				return list;
			}
		}
		[NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
		public TableStyle TableStyle
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
		[NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
		public TableItemStyle RowStyle
		{
			get
			{
				return this._rowStyle;
			}
			set
			{
				this._rowStyle = value;
			}
		}
		[NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty)]
		public TableItemStyle CellStyle
		{
			get
			{
				return this._cellStyle;
			}
			set
			{
				this._cellStyle = value;
			}
		}
		[PersistenceMode(PersistenceMode.InnerProperty), NotifyParentProperty(true)]
		public Style ContainerStyle
		{
			get
			{
				return this._containerStyle;
			}
			set
			{
				this._containerStyle = value;
			}
		}
		protected override ControlCollection CreateControlCollection()
		{
			return new ControlCollection(this);
		}
		public virtual string CreateContainerID(string parentID)
		{
			return parentID + "Container";
		}
		protected virtual IConfigurationSection GetConfigurationReference()
		{
			if (string.IsNullOrEmpty(base.PersonalizationProviderID))
			{
				throw new ApplicationException("TableZone needs a personalization provider");
			}
			ConfigurableWebPart personalizationProvider = ReflectionServices.FindControlEx(base.PersonalizationProviderID, base.WebPartManager) as ConfigurableWebPart;
			if (null == personalizationProvider)
			{
				throw new InvalidOperationException(string.Format("Can't find personalization provider {0}", base.PersonalizationProviderID));
			}
			IConfigurationSection result;
			if (personalizationProvider.Configuration.Sections.ContainsKey(this.ID))
			{
				result = personalizationProvider.Configuration.GetConfigurationSectionReference(this.ID);
			}
			else
			{
				result = personalizationProvider.Configuration.AddSection(this.ID);
			}
			return result;
		}
		public virtual void InitTable()
		{
			if (this.Controls.Contains(this._table))
			{
				this.Controls.Remove(this._table);
			}
			this._table = new RenderlessTable();
			this._table.ID = this.ID + "Table";
			this.Controls.Add(this._table);
		}
		protected virtual void LoadConfiguration()
		{
			if (null != this.ConfigurationLoading)
			{
				this.ConfigurationLoading(this, new EventArgs());
			}
			if (this.EnableConfigurationProvider)
			{
				this.InitTable();
				this._containersParts = new Dictionary<string, string>();
				IConfigurationSection configSection = this.GetConfigurationReference();
				if (null != configSection)
				{
					foreach (IConfigurationElement configRow in configSection.Elements.Values)
					{
						TableRow row = new TableRow();
						int count = this._table.Rows.Count;
                        row.ID = this._table.ID + "-" + count.ToString();
						row.Attributes["key"] = configRow.ConfigKey;
						this._table.Rows.Add(row);
						foreach (IConfigurationElement configCell in configRow.Elements.Values)
						{
							TableCell cell = new TableCell();
							count = row.Cells.Count;
                            cell.ID = row.ID + "-" + count.ToString();
							cell.Attributes["key"] = configCell.ConfigKey;
							row.Cells.Add(cell);
							if (configCell.Attributes.ContainsKey("span"))
							{
								int span = 1;
								int.TryParse(configCell.GetAttributeReference("span").Value.ToString(), out span);
								cell.ColumnSpan = span;
							}
                            if (configCell.Attributes.ContainsKey("rowspan"))
                            {
                                int rowspan = 1;
                                int.TryParse(configCell.GetAttributeReference("rowspan").Value.ToString(), out rowspan);
                                if (0 == rowspan)
                                    rowspan = 1;
                                cell.RowSpan = rowspan;
                            }
                            string id = configCell.GetAttributeReference("part").Value.ToString();
							WebPart part = base.WebParts[id];
							if (null != part)
							{
								this._containersParts.Add(this.CreateContainerID(cell.ID), id);
							}
							else
							{
								this._containersParts.Add(this.CreateContainerID(cell.ID), string.Empty);
							}
							this.LoadCellProperties(cell, configCell);
						}
					}
				}
			}
			if (null != this.ConfigurationLoaded)
			{
				this.ConfigurationLoaded(this, new EventArgs());
			}
		}
		protected virtual void LoadCellProperties(TableCell cell, IConfigurationElement configCell)
		{
			if (ControlFactory.Instance.KnownTypes.ContainsKey("TableCell"))
			{
				ControlFactory.ControlDescriptor descriptor = ControlFactory.Instance.KnownTypes["TableCell"];
				foreach (string name in descriptor.EditableProperties.Keys)
				{
					if (configCell.Attributes.ContainsKey(name))
					{
						ReflectionServices.SetValue(cell, name, configCell.GetAttributeReference(name).Value);
					}
				}
			}
		}
		protected virtual void SaveConfiguration()
		{
			if (null != this.ConfigurationSaving)
			{
				this.ConfigurationSaving(this, new EventArgs());
			}
			if (this.EnableConfigurationProvider)
			{
				if (null == this._table)
				{
					throw new InvalidOperationException("Table is null");
				}
				IConfigurationSection configSection = this.GetConfigurationReference();
				if (null != configSection)
				{
					configSection.Elements.Clear();
					foreach (TableRow row in this._table.Rows)
					{
						int num = this._table.Rows.GetRowIndex(row);
						IConfigurationElement configRow = configSection.AddElement(num.ToString());
						foreach (TableCell cell in row.Cells)
						{
							num = row.Cells.GetCellIndex(cell);
							IConfigurationElement configCell = configRow.AddElement(num.ToString());
							configCell.AddAttribute("span").Value = ((cell.ColumnSpan == 0) ? 1 : cell.ColumnSpan);
                            configCell.AddAttribute("rowspan").Value = ((cell.RowSpan == 0) ? 1 : cell.RowSpan);
                            string containerID = this.CreateContainerID(cell.ID);
							if (this._containersParts.ContainsKey(containerID))
							{
								configCell.AddAttribute("part").Value = this._containersParts[containerID];
							}
							else
							{
								configCell.AddAttribute("part").Value = string.Empty;
							}
							this.SaveCellProperties(cell, configCell);
						}
					}
				}
			}
			if (null != this.ConfigurationSaved)
			{
				this.ConfigurationSaved(this, new EventArgs());
			}
		}
		protected virtual void SaveCellProperties(TableCell cell, IConfigurationElement configCell)
		{
			if (ControlFactory.Instance.KnownTypes.ContainsKey("TableCell"))
			{
				ControlFactory.ControlDescriptor descriptor = ControlFactory.Instance.KnownTypes["TableCell"];
				foreach (string name in descriptor.EditableProperties.Keys)
				{
					configCell.AddAttribute(name).Value = ReflectionServices.ToString(ReflectionServices.ExtractValue(cell, name));
				}
			}
		}
		protected override void OnLoad(EventArgs e)
		{
			this.LoadConfiguration();
			base.OnLoad(e);
		}
		protected override void RaisePostBackEvent(string eventArgument)
		{
			if (!string.IsNullOrEmpty(eventArgument))
			{
				if (eventArgument.Contains("lwas"))
				{
					string[] args = eventArgument.Split(new char[]
					{
						':'
					});
					string verb = args[0];
					TableZoneEventArgs tzea = new TableZoneEventArgs(args, 1);
					if ("lwas.move" == verb)
					{
						this.OnMove(tzea);
					}
					else
					{
						if ("lwas.swap" == verb)
						{
							this.OnSwap(tzea);
						}
						else
						{
							if ("lwas.add" == verb)
							{
								this.OnAdd(tzea);
							}
							else
							{
								if ("lwas.list" == verb)
								{
									if (null != this.ListEvent)
									{
										this.ListEvent(this, tzea);
									}
								}
								else
								{
									if ("lwas.remove" == verb)
									{
										this.OnRemove(tzea);
									}
									else
									{
										if ("lwas.create" == verb)
										{
											this.OnCreateContainer(tzea);
										}
										else
										{
											if ("lwas.edit" == verb)
											{
												this.OnEdit(tzea);
											}
											else
											{
												if ("lwas.insert" == verb)
												{
													this.OnInsert(tzea);
												}
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					base.RaisePostBackEvent(eventArgument);
				}
			}
		}
		protected virtual void OnMove(TableZoneEventArgs tzea)
		{
			if (null != this.MoveEvent)
			{
				this.MoveEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Move(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer, tzea.SecondPart, tzea.SecondRow, tzea.SecondCell, tzea.SecondContainer);
			}
		}
		public void Move(string firstPart, string firstRow, string firstCell, string firstContainer, string secondPart, string secondRow, string secondCell, string secondContainer)
		{
			if (!string.IsNullOrEmpty(secondRow) && string.IsNullOrEmpty(secondCell))
			{
				secondContainer = this.CreateContainer(secondPart, secondRow, secondCell, secondContainer);
			}
			WebPart webPart = base.WebPartManager.WebParts[firstPart];
			if (null != webPart)
			{
				if (webPart.Zone is BaseZone)
				{
					BaseZone zone = webPart.Zone as BaseZone;
					zone.EmptyContainer(zone.PartContainer(webPart.ID));
				}
				base.WebPartManager.MoveWebPart(webPart, this, base.WebParts.Count);
				this._containersParts[secondContainer] = firstPart;
			}
		}
		protected virtual void OnSwap(TableZoneEventArgs tzea)
		{
			if (null != this.SwapEvent)
			{
				this.SwapEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Swap(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer, tzea.SecondPart, tzea.SecondRow, tzea.SecondCell, tzea.SecondContainer);
			}
		}
		public void Swap(string firstPart, string firstRow, string firstCell, string firstContainer, string secondPart, string secondRow, string secondCell, string secondContainer)
		{
			WebPart webPart = base.WebPartManager.WebParts[firstPart];
			if (null != webPart)
			{
				if (webPart.Zone is TableZone)
				{
					TableZone sourceZone = webPart.Zone as TableZone;
					this.Move(firstPart, firstRow, firstCell, firstContainer, secondPart, secondRow, secondCell, secondContainer);
					sourceZone.Move(secondPart, secondRow, secondCell, secondContainer, firstPart, firstRow, firstCell, firstContainer);
				}
				else
				{
					WebPart targetPart = base.WebParts[secondPart];
					WebPartZoneBase sourceZone2 = webPart.Zone;
					base.WebPartManager.MoveWebPart(webPart, this, targetPart.ZoneIndex);
					base.WebPartManager.MoveWebPart(targetPart, sourceZone2, webPart.ZoneIndex);
				}
			}
		}
		public void OnAdd(TableZoneEventArgs tzea)
		{
			if (null != this.AddEvent)
			{
				this.AddEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Add(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer, tzea.SecondPart, tzea.SecondRow, tzea.SecondCell, tzea.SecondContainer);
			}
		}
		protected virtual void Add(string firstPart, string firstRow, string firstCell, string firstContainer, string secondPart, string secondRow, string secondCell, string secondContainer)
		{
			if (!string.IsNullOrEmpty(secondRow) && string.IsNullOrEmpty(secondCell))
			{
				secondContainer = this.CreateContainer(secondPart, secondRow, secondCell, secondContainer);
			}
			WebPart webPart = base.WebPartManager.WebParts[firstPart];
			WebPart oldPart = base.WebParts[secondPart];
			if (null != oldPart)
			{
				if (!(oldPart is SymbolWebPart))
				{
					throw new InvalidOperationException(string.Format("Unwanted deletion of {0}", oldPart.Title));
				}
				base.WebPartManager.DeleteWebPart(oldPart);
			}
			WebPart newPart = null;
			if (webPart is SymbolWebPart)
			{
				newPart = ((SymbolWebPart)webPart).Instantiate(this, base.WebParts.Count);
			}
			else
			{
				newPart = base.WebPartManager.AddWebPart(webPart, this, base.WebParts.Count);
			}
			if (null == newPart)
			{
				throw new InvalidOperationException("Failed to lwas.add");
			}
			this._containersParts[secondContainer] = newPart.ID;
		}
		public virtual void OnCreateContainer(TableZoneEventArgs tzea)
		{
			if (null != this.CreateContainerEvent)
			{
				this.CreateContainerEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.CreateContainer(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer);
			}
		}
		public string CreateContainer(string part, string row, string cell, string container)
		{
			if (null == this._table)
			{
				throw new InvalidOperationException("Table not initialized");
			}
			string ret = null;
			if (string.IsNullOrEmpty(row) && string.IsNullOrEmpty(cell))
			{
				TableRow tr = new TableRow();
				int num = this._table.Rows.Count;
                tr.ID = this._table.ID + "-" + num.ToString();
				this._table.Rows.Add(tr);
				num = this._table.Rows.GetRowIndex(tr);
                tr.Attributes["key"] = num.ToString();
			}
			else
			{
				if (!string.IsNullOrEmpty(row) && string.IsNullOrEmpty(cell))
				{
					TableRow tr2 = null;
					foreach (TableRow tr in this.Table.Rows)
					{
						if (row == tr.Attributes["key"])
						{
							tr2 = tr;
							break;
						}
					}
					if (null != tr2)
					{
						TableCell tc = new TableCell();
						int num = tr2.Cells.Count;
                        tc.ID = tr2.ID + "-" + num.ToString();
						tr2.Cells.Add(tc);
						num = tr2.Cells.GetCellIndex(tc);
                        tc.Attributes["key"] = num.ToString();
						ret = this.CreateContainerID(tc.ID);
						this._containersParts.Add(ret, string.Empty);
					}
				}
			}
			return ret;
		}
		protected virtual void OnRemove(TableZoneEventArgs tzea)
		{
			if (null != this.RemoveEvent)
			{
				this.RemoveEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Remove(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer);
			}
		}
		public void Remove(string part, string row, string cell, string container)
		{
			if (!string.IsNullOrEmpty(row) || !string.IsNullOrEmpty(cell))
			{
				if (!string.IsNullOrEmpty(row) && string.IsNullOrEmpty(cell))
				{
					foreach (TableRow tr in this._table.Rows)
					{
						if (row == tr.Attributes["key"])
						{
							foreach (TableCell tc in tr.Cells)
							{
								this.EmptyContainer(this.CreateContainerID(tc.ID));
							}
							tr.Cells.Clear();
							this._table.Rows.Remove(tr);
							break;
						}
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(row) && !string.IsNullOrEmpty(cell))
					{
						TableRow targetRow = null;
						TableCell targetCell = null;
						foreach (TableRow tr in this._table.Rows)
						{
							foreach (TableCell tc in tr.Cells)
							{
								if (row == tr.Attributes["key"] && cell == tc.Attributes["key"])
								{
									targetCell = tc;
									break;
								}
							}
							if (null != targetCell)
							{
								targetRow = tr;
								break;
							}
						}
						if (targetRow != null && null != targetCell)
						{
							this.EmptyContainer(this.CreateContainerID(targetCell.ID));
							targetRow.Cells.Remove(targetCell);
						}
					}
				}
				this.SaveConfiguration();
				this.LoadConfiguration();
			}
		}
		public virtual void OnEdit(TableZoneEventArgs tzea)
		{
			if (null != this.EditEvent)
			{
				this.EditEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Edit(tzea.FirstPart, tzea.FirstRow, tzea.FirstCell, tzea.FirstContainer);
			}
		}
		public void Edit(string part, string row, string cell, string container)
		{
		}
		public virtual void OnInsert(TableZoneEventArgs tzea)
		{
			if (null != this.InsertEvent)
			{
				this.InsertEvent(this, tzea);
			}
			if (!tzea.Cancel)
			{
				this.Insert(tzea.FirstRow, tzea.FirstCell);
			}
		}
		public void Insert(string row, string cell)
		{
			TableRow targetRow = null;
			TableCell targetCell = null;
			if (string.IsNullOrEmpty(row))
			{
				if (this._table.Rows.Count > 0)
				{
					targetRow = this._table.Rows[0];
				}
			}
			else
			{
				foreach (TableRow tr in this._table.Rows)
				{
					if (row == tr.Attributes["key"])
					{
						targetRow = tr;
						break;
					}
				}
			}
			if (targetRow != null && !string.IsNullOrEmpty(cell))
			{
				foreach (TableCell tc in targetRow.Cells)
				{
					if (cell == tc.Attributes["key"])
					{
						targetCell = tc;
						break;
					}
				}
			}
			if (null != targetCell)
			{
				TableCell newCell = new TableCell();
				newCell.ID = targetRow.ID + "-" + targetRow.Cells.Count;
				targetRow.Cells.AddAt(targetRow.Cells.GetCellIndex(targetCell), newCell);
				int num = targetRow.Cells.GetCellIndex(newCell);
                newCell.Attributes["key"] = num.ToString();
			}
			else
			{
				if (null != targetRow)
				{
					TableRow newRow = new TableRow();
					newRow.ID = this._table.ID + "-" + this._table.Rows.Count;
					this._table.Rows.AddAt(this._table.Rows.GetRowIndex(targetRow), newRow);
					int num = this._table.Rows.GetRowIndex(newRow);
                    newRow.Attributes["key"] = num.ToString();
				}
				else
				{
					TableRow newRow = new TableRow();
					newRow.ID = this._table.ID + "-" + this._table.Rows.Count;
					this._table.Rows.AddAt(0, new TableRow());
					int num = this._table.Rows.GetRowIndex(newRow);
                    newRow.Attributes["key"] = num.ToString();
				}
			}
			this.SaveConfiguration();
			this.LoadConfiguration();
		}
		public override void EmptyContainer(string containerID)
		{
			if (this._containersParts.ContainsKey(containerID))
			{
				this._containersParts[containerID] = string.Empty;
			}
		}
		public override void FillContainer(string containerID, string partID)
		{
			string oldContainer = this.PartContainer(partID);
			if (!string.IsNullOrEmpty(oldContainer))
			{
				this.EmptyContainer(oldContainer);
			}
			this._containersParts[containerID] = partID;
		}
		public override string PartContainer(string partID)
		{
			string result;
			if (!this._containersParts.ContainsValue(partID))
				result = string.Empty;
			else
			{
				foreach (string key in this._containersParts.Keys)
				{
					if (this._containersParts[key] == partID)
					{
						result = key;
						return result;
					}
				}
				result = string.Empty;
			}

            return result;
		}
		public override string ContainerPart(string containerID)
		{
			string result;
			if (!this._containersParts.ContainsKey(containerID))
				result = string.Empty;
			else
				result = this._containersParts[containerID];

            return result;
		}
		protected override void OnPreRender(EventArgs e)
		{
			this.SaveConfiguration();
			base.OnPreRender(e);
		}
		protected override void RenderBody(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this._table.ID);
			this._tableStyle.AddAttributesToRender(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			if (this._table.Rows.Count == 0)
			{
				this._rowStyle.AddAttributesToRender(writer);
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);
				this._cellStyle.AddAttributesToRender(writer);
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				if (base.WebPartManager.DisplayMode.AllowPageDesign)
				{
					base.EmptyZoneTextStyle.AddAttributesToRender(writer);
					writer.RenderBeginTag(HtmlTextWriterTag.Div);
					writer.Write(this.EmptyZoneText);
					writer.RenderEndTag();
				}
				writer.RenderEndTag();
				writer.RenderEndTag();
				writer.RenderEndTag();
			}
			else
			{
				foreach (TableRow row in this._table.Rows)
				{
					this._rowStyle.AddAttributesToRender(writer);
					foreach (string key in row.Attributes.Keys)
					{
						writer.AddAttribute(key, row.Attributes[key]);
					}
					writer.AddAttribute(HtmlTextWriterAttribute.Id, row.ID);
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);
					foreach (TableCell cell in row.Cells)
                    {
                        bool isMarkerRow = !String.IsNullOrEmpty(cell.Attributes["isMarker"]);
						this._cellStyle.AddAttributesToRender(writer);
						cell.ControlStyle.AddAttributesToRender(writer);
						foreach (string key in cell.Attributes.Keys)
						{
							writer.AddAttribute(key, cell.Attributes[key]);
						}
						writer.AddAttribute(HtmlTextWriterAttribute.Colspan, (cell.ColumnSpan == 0 ? 1 : cell.ColumnSpan).ToString());
                        writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, (cell.RowSpan == 0 ? 1 : cell.RowSpan).ToString());
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, cell.ID);
						writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        if (!isMarkerRow)
                        {
                            WebPart webPart = null;
                            string containerID = this.CreateContainerID(cell.ID);
                            if (this._containersParts.ContainsKey(containerID))
                            {
                                webPart = base.WebParts[this._containersParts[containerID]];
                            }
                            this._containerStyle.AddAttributesToRender(writer);
                            writer.AddAttribute(HtmlTextWriterAttribute.Id, containerID);
                            this.RenderContainerClass(writer, webPart);
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                            if (null != webPart)
                            {
                                base.WebPartChrome.RenderWebPart(writer, webPart);
                            }
                            foreach (Control control in cell.Controls)
                            {
                                control.RenderControl(writer);
                            }
                            if (!String.IsNullOrEmpty(cell.Attributes["watermark"]))
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.I);
                                writer.WriteEncodedText(cell.Attributes["watermark"]);
                                writer.RenderEndTag();
                            }
                            writer.RenderEndTag();
                        }
						writer.RenderEndTag();
					}
					writer.RenderEndTag();
				}
				writer.RenderEndTag();
			}
		}
		protected virtual void RenderContainerClass(HtmlTextWriter writer, WebPart webPart)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "tablezone_container");
		}
	}
}
