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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace LWAS.Infrastructure.Configuration
{
	public class ConfigurationTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
		}
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			return base.IsValid(context, value);
		}
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			object result;
			if (value is string)
			{
				string serialization = value as string;
				Configuration configuration = new Configuration();
				if (!string.IsNullOrEmpty(serialization))
				{
					using (StringReader stringReader = new StringReader((string)value))
					{
						using (XmlTextReader reader = new XmlTextReader(stringReader))
						{
							reader.Read();
							SerializationServices.Deserialize(configuration.GetType(), reader, false);
						}
					}
				}
				result = configuration;
			}
			else
			{
				result = base.ConvertFrom(context, culture, value);
			}
			return result;
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			Configuration configuration = value as Configuration;
			object result;
			if (null != configuration)
			{
				if (destinationType == typeof(string))
				{
					StringBuilder serializationBuilder = new StringBuilder();
					using (StringWriter stringWriter = new StringWriter(serializationBuilder))
					{
						using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
						{
							SerializationServices.Serialize(configuration, writer);
						}
						result = serializationBuilder.ToString();
						return result;
					}
				}
			}
			result = base.ConvertTo(context, culture, value, destinationType);
			return result;
		}
	}
}
