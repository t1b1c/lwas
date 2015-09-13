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
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;

using LWAS.CustomControls;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Templating
{
	public class ControlFactory : IReporter
	{
		public class PropertyDescriptor
		{
			public string Name;
			public string Value;
			public string[] Options;
			public string ValueType;
			public bool IsList = false;
			public PropertyDescriptor(string name, string value, string options, string valueType, bool isList)
			{
				this.Name = name;
				this.Value = value;
				if (string.IsNullOrEmpty(options))
				{
					this.Options = null;
				}
				else
				{
					this.Options = options.Split(new char[]
					{
						','
					});
				}
				this.ValueType = valueType;
				this.IsList = isList;
			}
		}
		public class ControlDescriptor
		{
			public string Key;
			public string AssemblyQualifiedName;
			public string ReadOnlyProperty;
			public string ReadOnlyValue;
            public string WatermarkProperty;
			public Dictionary<string, ControlFactory.PropertyDescriptor> EditableProperties = new Dictionary<string, ControlFactory.PropertyDescriptor>();
			public ControlDescriptor(string key, string name, string readOnly, string readOnlyValue, string watermarkProperty)
			{
				this.Key = key;
				this.AssemblyQualifiedName = name;
				this.ReadOnlyProperty = readOnly;
				this.ReadOnlyValue = readOnlyValue;
                this.WatermarkProperty = watermarkProperty;
			}
		}
		
        private IStorageAgent _agent;
		private bool areTypesLoaded = false;
		private Dictionary<string, ControlFactory.ControlDescriptor> _knownTypes = new Dictionary<string, ControlFactory.ControlDescriptor>();
		private string _title = "ControlFactory";
		private IMonitor _monitor;
		public static ControlFactory Instance;
        private bool? _isDesignEnabled;

		public IStorageAgent Agent
		{
			get { return this._agent; }
			set { this._agent = value; }
		}
		public Dictionary<string, ControlFactory.ControlDescriptor> KnownTypes
		{
			get
			{
				if (!this.areTypesLoaded)
				{
					this.LoadTypes();
					this.areTypesLoaded = true;
				}
				return this._knownTypes;
			}
		}
		public string Title
		{
			get { return this._title; }
			set { this._title = value; }
		}
		public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}

        public bool IsDesignEnabled 
        {
            get
            {
                if (!_isDesignEnabled.HasValue)
                {
                    bool configValue = false;
                    bool.TryParse(ConfigurationManager.AppSettings["DESIGN"], out configValue);
                    _isDesignEnabled = configValue;
                    
                }
                return _isDesignEnabled.Value;
            }
        }

		static ControlFactory()
		{
			ControlFactory.Instance = new ControlFactory();
		}

		public void LoadTypes()
		{
			if (null == this._agent) { throw new InvalidOperationException("Agent not set"); }
			string key = ConfigurationManager.AppSettings["CONTROL_TYPES"];
			if (string.IsNullOrEmpty(key)) { throw new ApplicationException("'CONTROL_TYPES' web.config key not set"); }
			
            XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(this._agent.OpenStream(key));
			}
			finally
			{
				this._agent.CloseStream(key);
			}
			XmlNode root = doc.SelectSingleNode("types");
			if (null == root) { throw new InvalidOperationException("Control types file has no types node"); }

			foreach (XmlNode node in root.ChildNodes)
			{
				ControlFactory.ControlDescriptor descriptor = new ControlFactory.ControlDescriptor(node.Attributes["key"].Value, 
                                                                                                   node.Attributes["name"].Value, 
                                                                                                   (node.Attributes["readOnly"] == null) ? string.Empty : node.Attributes["readOnly"].Value, 
                                                                                                   (node.Attributes["readOnlyValue"] == null) ? string.Empty : node.Attributes["readOnlyValue"].Value,
                                                                                                   (node.Attributes["watermark"] == null) ? string.Empty : node.Attributes["watermark"].Value);
				if (node.HasChildNodes)
				{
					foreach (XmlNode child in node.ChildNodes)
					{
						ControlFactory.PropertyDescriptor propertyDescriptor = new ControlFactory.PropertyDescriptor(child.Attributes["name"].Value, (child.Attributes["value"] == null) ? string.Empty : child.Attributes["value"].Value, (child.Attributes["options"] == null) ? string.Empty : child.Attributes["options"].Value, (child.Attributes["ValueType"] == null) ? string.Empty : child.Attributes["ValueType"].Value, child.Attributes["isList"] != null && bool.Parse(child.Attributes["isList"].Value));
						descriptor.EditableProperties.Add(propertyDescriptor.Name, propertyDescriptor);
					}
				}
				this._knownTypes.Add(descriptor.Key, descriptor);
			}
		}
		public Control CreateControl(IConfigurationElement controlElement, string index, IBinder binder, ITemplatingItem item, WebControl container, TableItemStyle invalidStyle, Dictionary<string, Control> registry, WebPartManager manager)
		{
			if (null == this._monitor)
			{
				throw new ApplicationException("Monitor not set");
			}
			Control result;
			if (!controlElement.Attributes.ContainsKey("proxy") && !controlElement.Attributes.ContainsKey("type"))
			{
				result = null;
			}
			else
			{
				Control cellControl = null;
				string cellControlName = null;
				if (controlElement.Attributes.ContainsKey("proxy"))
				{
					object proxyIDValue = controlElement.GetAttributeReference("proxy").Value;
					string proxyID = null;
					if (null != proxyIDValue)
					{
						proxyID = proxyIDValue.ToString();
					}
					if (string.IsNullOrEmpty(proxyID))
					{
						result = null;
						return result;
					}
					Control proxy = ReflectionServices.FindControlEx(proxyID, manager);
					if (null == proxy)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("create proxy error", null, new ApplicationException("can't find proxy " + proxyID), EVENT_TYPE.Error));
					}
					string proxyMember = null;
					if (!controlElement.Attributes.ContainsKey("proxyMember"))
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("create proxy error", null, new ApplicationException("proxyMember not found for proxy " + proxyID), EVENT_TYPE.Error));
					}
					else
					{
						proxyMember = controlElement.GetAttributeReference("proxyMember").Value.ToString();
					}
					try
					{
						cellControl = (ReflectionServices.ExtractValue(proxy, proxyMember) as Control);
						cellControlName = proxy + "." + proxyMember;
					}
					catch (Exception ex)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("create proxy error", null, ex, EVENT_TYPE.Error));
					}
					if (cellControl == null && null != proxy)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("create proxy error", null, new ApplicationException(string.Format("member {0} of proxy type {1} is not a Control", proxyMember, proxy.GetType().FullName)), EVENT_TYPE.Error));
					}
				}
				else
				{
					Control scope = container;
					if (container is TableCell)
					{
						scope = container.Parent.Parent;
					}
					try
					{
						cellControl = this.CreateControl(controlElement.GetAttributeReference("type").Value.ToString(), new bool?(item == null || item.IsReadOnly), scope);
						cellControlName = controlElement.GetAttributeReference("type").Value.ToString();
					}
					catch (Exception ex)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("create control error", null, ex, EVENT_TYPE.Error));
					}
				}
				if (null == cellControl)
				{
					result = null;
				}
				else
				{
                    string type = null;
					if (controlElement.Attributes.ContainsKey("type"))
					{
						type = controlElement.GetAttributeReference("type").Value.ToString();
						if (cellControl is IButtonControl)
						{
							IButtonControl btn = cellControl as IButtonControl;
							if (("Commander" == type || "LinkCommander" == type) && !controlElement.Attributes.ContainsKey("command"))
							{
								this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("create {0} error", type), null, new ApplicationException("command not found"), EVENT_TYPE.Error));
							}
							else
							{
								if (controlElement.Attributes.ContainsKey("command"))
								{
									btn.CommandName = controlElement.GetAttributeReference("command").Value.ToString();
								}
							}
							btn.CommandArgument = index;
						}
                        if (cellControl is StatelessDropDownList)
                            ((StatelessDropDownList)cellControl).CommandArgument = index;
					}
					if (null != registry)
					{
						registry.Add(controlElement.ConfigKey, cellControl);
					}
                    var properties = controlElement.Elements.Values;
					foreach (IConfigurationElement controlPropertyElement in properties)
					{
						if (!controlPropertyElement.Attributes.ContainsKey("for") || !("cell" == controlPropertyElement.GetAttributeReference("for").Value.ToString()))
						{
							string propertyName = null;
							if (!controlPropertyElement.Attributes.ContainsKey("member"))
							{
								this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("'{0}' property set error", cellControlName), null, new ApplicationException(string.Format("member not found for '{0}'", controlPropertyElement.ConfigKey)), EVENT_TYPE.Error));
							}
							else
							{
								propertyName = controlPropertyElement.GetAttributeReference("member").Value.ToString();
                            }

                            IExpression expression = null;
                            LWAS.Extensible.Interfaces.IResult expressionResult = null;
                            if (controlPropertyElement.Elements.ContainsKey("expression"))
                            {
                                IConfigurationElement expressionElement = controlPropertyElement.GetElementReference("expression");
                                Manager sysmanager = manager as Manager;
                                if (null != sysmanager && null != sysmanager.ExpressionsManager && expressionElement.Attributes.ContainsKey("type"))
                                {
                                    expression = sysmanager.ExpressionsManager.Token(expressionElement.GetAttributeReference("type").Value.ToString()) as IExpression;
                                    if (null != expression)
                                    {
                                        try
                                        {
                                            expression.Make(expressionElement, sysmanager.ExpressionsManager);
                                            expressionResult = expression.Evaluate();
                                        }
                                        catch (ArgumentException ax)
                                        {
                                        }
                                    }
                                }
                            }

							object defaultValue = null;
							if (!string.IsNullOrEmpty(propertyName) && controlPropertyElement.Attributes.ContainsKey("value"))
							{
								defaultValue = controlPropertyElement.GetAttributeReference("value").Value;
                                if (null == expression || null == binder)
                                {
                                    try
                                    {
                                        if (this.KnownTypes.ContainsKey(cellControlName))
                                        {
                                            if (string.IsNullOrEmpty(cellControlName) ||
                                                propertyName != this.KnownTypes[cellControlName].ReadOnlyProperty ||
                                                (propertyName == this.KnownTypes[cellControlName].ReadOnlyProperty && defaultValue != null && !string.IsNullOrEmpty(defaultValue.ToString())))
                                            {
                                                if (propertyName == "Watermark")
                                                {
                                                    if (null != defaultValue && !String.IsNullOrEmpty(defaultValue.ToString()))
                                                        ReflectionServices.SetValue(cellControl, propertyName, defaultValue);
                                                }
                                                else
                                                    ReflectionServices.SetValue(cellControl, propertyName, defaultValue);
                                            }
                                        }
                                        else
                                            ReflectionServices.SetValue(cellControl, propertyName, defaultValue);
                                    }
                                    catch (Exception ex)
                                    {
                                        this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("'{0}'.'{1}'='{2}' error", cellControlName, propertyName, controlPropertyElement.GetAttributeReference("value").Value), null, ex, EVENT_TYPE.Error));
                                    }
                                }
                                else
                                {
                                    IBindingItem bindingItem = binder.NewBindingItemInstance();
                                    bindingItem.Source = new Dictionary<string, object>() { { "0", defaultValue} };
                                    bindingItem.SourceProperty = "0";
                                    bindingItem.Target = cellControl;
                                    bindingItem.TargetProperty = propertyName;
                                    bindingItem.Expression = expression;
                                    bindingItem.ExpressionEvaluationResult = expressionResult;
                                    binder.BindingItems.Add(bindingItem);
                                }
							}

							if (controlPropertyElement.Attributes.ContainsKey("isList") && (bool)controlPropertyElement.GetAttributeReference("isList").Value)
							{
								object list = null;
								try
								{
									list = ReflectionServices.ExtractValue(cellControl, propertyName);
									if (null == list) throw new InvalidOperationException(string.Format("Member '{0}' is empty", propertyName));
                                    if (!(list is ListItemCollection) && !(list is IDictionary<string, object>)) throw new InvalidOperationException(String.Format("Unsupported list type '{0}'", list.GetType().Name));
								}
								catch (Exception ex)
								{
									this._monitor.Register(this, this._monitor.NewEventInstance("failed to retrive the list", null, ex, EVENT_TYPE.Error));
								}
								if (null != list)
								{
									if (controlPropertyElement.Attributes.ContainsKey("hasEmpty") && (bool)controlPropertyElement.GetAttributeReference("hasEmpty").Value)
									{
                                        if (list is ListItemCollection)
                                            ((ListItemCollection)list).Add("");
                                        else if (list is Dictionary<string, object>)
                                            ((Dictionary<string, object>)list).Add("", "");
									}
									foreach (IConfigurationElement listItemElement in controlPropertyElement.Elements.Values)
									{
										string value = null;
										string text = null;
                                        string pull = null;
										if (listItemElement.Attributes.ContainsKey("value") && null != listItemElement.GetAttributeReference("value").Value)
											value = listItemElement.GetAttributeReference("value").Value.ToString();
										if (listItemElement.Attributes.ContainsKey("text") && null != listItemElement.GetAttributeReference("text").Value)
											text = listItemElement.GetAttributeReference("text").Value.ToString();
                                        if (listItemElement.Attributes.ContainsKey("pull") && null != listItemElement.GetAttributeReference("pull").Value)
                                            pull = listItemElement.GetAttributeReference("pull").Value.ToString();

                                        if (list is ListItemCollection)
                                        {
                                            ListItem li = new ListItem(text, value);
                                            ((ListItemCollection)list).Add(li);
                                            if (null != binder && !String.IsNullOrEmpty(pull))
                                            {
                                                IBindingItem bindingItem = binder.NewBindingItemInstance();
                                                bindingItem.Source = item.Data;
                                                bindingItem.SourceProperty = pull;
                                                bindingItem.Target = li;
                                                bindingItem.TargetProperty = "Text";
                                                bindingItem.Expression = expression;
                                                binder.BindingItems.Add(bindingItem);
                                            }
                                        }
                                        else if (list is Dictionary<string, object>)
                                        {
                                            ((Dictionary<string, object>)list).Add(value, text);
                                            if (null != binder && !String.IsNullOrEmpty(pull))
                                            {
                                                IBindingItem bindingItem = binder.NewBindingItemInstance();
                                                bindingItem.Source = item.Data;
                                                bindingItem.SourceProperty = pull;
                                                bindingItem.Target = list;
                                                bindingItem.TargetProperty = value;
                                                bindingItem.Expression = expression;
                                                binder.BindingItems.Add(bindingItem);
                                            }
                                        }
									}
								}
							}
							if (controlPropertyElement.Attributes.ContainsKey("property"))
							{
								if (container.Page != null && null != binder)
								{
									IBindingItem bindingItem = binder.NewBindingItemInstance();
									bindingItem.Source = WebPartManager.GetCurrentWebPartManager(container.Page);
									bindingItem.SourceProperty = "WebParts." + controlPropertyElement.GetAttributeReference("property").Value.ToString();
									bindingItem.Target = cellControl;
									bindingItem.TargetProperty = propertyName;
									if (string.IsNullOrEmpty(cellControlName) || propertyName != this.KnownTypes[cellControlName].ReadOnlyProperty)
										bindingItem.DefaultValue = defaultValue;
                                    bindingItem.Expression = expression;
									binder.BindingItems.Add(bindingItem);
								}
							}
							if (controlPropertyElement.Attributes.ContainsKey("pull"))
							{
                                string pull = controlPropertyElement.GetAttributeReference("pull").Value.ToString();
								if (binder != null && null != item)
								{
									IBindingItem bindingItem = binder.NewBindingItemInstance();
									bindingItem.Source = item.Data;
									bindingItem.SourceProperty = pull;
									bindingItem.Target = cellControl;
									bindingItem.TargetProperty = propertyName;
                                    if (string.IsNullOrEmpty(cellControlName) ||
                                        (this.KnownTypes.ContainsKey(cellControlName) &&
                                            propertyName != this.KnownTypes[cellControlName].ReadOnlyProperty))
                                    {
                                        bindingItem.DefaultValue = defaultValue;
                                    }
                                    bindingItem.Expression = expression;
									binder.BindingItems.Add(bindingItem);
									if (item.InvalidMember == bindingItem.SourceProperty)
										container.ApplyStyle(invalidStyle);
								}
                                if (this.IsDesignEnabled && propertyName == this.KnownTypes[cellControlName].WatermarkProperty)
                                {
                                     // StyleTextBox has its own Watermark feature
                                    if (typeof(StyledTextBox).IsInstanceOfType(cellControl))
                                    {
                                        ReflectionServices.SetValue(cellControl, "Watermark", pull);
                                    }
                                    else
                                        container.Attributes.Add("watermark", pull);
                                }
							}
						}
					}
					result = cellControl;
				}
			}
			return result;
		}
		public Control CreateControl(string type, bool? readOnly, Control scope)
		{
			if (null == this._monitor)
			{
				throw new ApplicationException("Monitor not set");
			}
			Control ctrl = null;
			if (readOnly.HasValue && readOnly.Value)
			{
				try
				{
					ctrl = this.CreateReadOnlyControl(type, scope);
				}
				catch (Exception ex)
				{
					this._monitor.Register(this, this._monitor.NewEventInstance("create readonly control error", null, ex, EVENT_TYPE.Error));
				}
			}
			else
			{
				try
				{
					ctrl = this.CreateEditableControl(type, scope);
				}
				catch (Exception ex)
				{
					this._monitor.Register(this, this._monitor.NewEventInstance("create editable control error", null, ex, EVENT_TYPE.Error));
				}
			}
			return ctrl;
		}
		public Control CreateEditableControl(string key, Control scope)
		{
			if (null == this._monitor)
			{
				throw new ApplicationException("Monitor not set");
			}
			if (!this.KnownTypes.ContainsKey(key))
			{
				throw new InvalidOperationException(string.Format("Unknown control type '{0}'", key));
			}
			Control control = null;
			try
			{
				control = (ReflectionServices.CreateInstance(this.KnownTypes[key].AssemblyQualifiedName) as Control);
			}
			catch (Exception ex)
			{
				this._monitor.Register(this, this._monitor.NewEventInstance("create editable control error", null, ex, EVENT_TYPE.Error));
			}
			if (control is NumberTextBox)
			{
				((NumberTextBox)control).AutoIncrementScope = scope;
			}
			return control;
		}
		public Control CreateReadOnlyControl(string key, Control scope)
		{
			if (null == this._monitor)
			{
				throw new ApplicationException("Monitor not set");
			}
			if (!this.KnownTypes.ContainsKey(key))
			{
				throw new InvalidOperationException(string.Format("Unknown control type '{0}'", key));
			}
			ControlFactory.ControlDescriptor descriptor = this.KnownTypes[key];
			Control control = null;
			try
			{
				control = (ReflectionServices.CreateInstance(descriptor.AssemblyQualifiedName) as Control);
			}
			catch (Exception ex)
			{
				this._monitor.Register(this, this._monitor.NewEventInstance("create readonly control error", null, ex, EVENT_TYPE.Error));
			}
			if (null != control)
			{
				if (!string.IsNullOrEmpty(descriptor.ReadOnlyProperty))
				{
					try
					{
						ReflectionServices.SetValue(control, descriptor.ReadOnlyProperty, descriptor.ReadOnlyValue);
					}
					catch (Exception ex)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance(string.Format("set readonly property error '{0}'.'{1}'='{2}'", key, descriptor.ReadOnlyProperty, descriptor.ReadOnlyValue), null, ex, EVENT_TYPE.Error));
					}
				}
			}
			if (control is NumberTextBox)
			{
				((NumberTextBox)control).AutoIncrementScope = scope;
			}
			return control;
		}
	}
}
