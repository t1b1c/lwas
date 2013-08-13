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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace LWAS.Infrastructure
{
	public static class SerializationServices
	{
		public static void Serialize(object source, XmlWriter writer)
		{
			if (null == source) throw new ArgumentNullException("source");
			if (null == writer) throw new ArgumentNullException("writer");

			StringBuilder serializationBuilder = new StringBuilder();
			using (StringWriter stringWriter = new StringWriter(serializationBuilder))
			{
				using (XmlTextWriter textWriter = new XmlTextWriter(stringWriter))
				{
					textWriter.Formatting = Formatting.Indented;
					XmlSerializer serializer = new XmlSerializer(source.GetType());
					serializer.Serialize(textWriter, source);
					string serialization = serializationBuilder.ToString();
					using (StringReader stringReader = new StringReader(serialization))
					{
						using (XmlTextReader textReader = new XmlTextReader(stringReader))
						{
							textReader.WhitespaceHandling = WhitespaceHandling.None;
							textReader.MoveToContent();
							writer.WriteNode(textReader, false);
						}
					}
				}
			}
		}
		public static string BinarySerialize(object source)
		{
			string result;
			using (MemoryStream stream = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(stream, source);
                }
                catch {}
				result = Convert.ToBase64String(stream.ToArray());
			}
			return result;
		}
		public static object Deserialize(Type type, XmlReader reader)
		{
			return SerializationServices.Deserialize(type, reader, true);
		}
		public static object Deserialize(Type type, XmlReader reader, bool readInnerXml)
		{
			if (null == type) throw new ArgumentNullException("type");
			if (null == reader) throw new ArgumentNullException("reader");
			if (reader.NodeType != XmlNodeType.Element) throw new ArgumentException("reader is not positioned on an element");

			object value = null;
			StringBuilder serializationBuilder = new StringBuilder();
			serializationBuilder.Append("<?xml version=\"1.0\"?>");
			if (readInnerXml)
			{
				serializationBuilder.AppendLine(reader.ReadInnerXml());
			}
			else
			{
				serializationBuilder.AppendLine(reader.ReadOuterXml());
			}
			string serialization = serializationBuilder.ToString();
			using (StringReader stringReader = new StringReader(serialization))
			{
				XmlSerializer serializer = new XmlSerializer(type);
				value = serializer.Deserialize(stringReader);
			}
			return value;
		}
		public static object BinaryDeserialize(string content)
		{
			object result;
			using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(content)))
			{
				IFormatter formatter = new BinaryFormatter();
				result = formatter.Deserialize(stream);
			}
			return result;
		}

        public static string ShortAssemblyQualifiedName(string assemblyQualifiedName)
        {
            string ret = assemblyQualifiedName;
            if (ret.StartsWith("LWAS"))
            {
                ret = Regex.Replace(assemblyQualifiedName, @", Version=\d+.\d+.\d+.\d+", string.Empty);
                ret = Regex.Replace(ret, @", Culture=\w+", string.Empty);
                ret = Regex.Replace(ret, @", PublicKeyToken=\w+", string.Empty);
            }
            return ret;
        }
	}
}
