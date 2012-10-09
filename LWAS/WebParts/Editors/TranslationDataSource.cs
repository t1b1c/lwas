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
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure.Storage;

namespace LWAS.WebParts.Editors
{
	public class TranslationDataSource : DataSourceControl
	{
        string translationFile;
        public IStorageAgent Agent { get; set; }

        public TranslationDataSource(string file)
        {
            if (String.IsNullOrEmpty(file)) throw new ArgumentNullException("file");

            translationFile = file;
            this.Agent = new FileAgent();

            if (!this.Agent.HasKey(translationFile)) throw new ArgumentException(String.Format("Can't find translation file at '{0}'", translationFile));
        }

        XDocument LoadDocument()
        {
            return XDocument.Parse(this.Agent.Read(translationFile));
        }

        void SaveDocument(XDocument xdoc)
        {
            if (this.Agent.HasKey(translationFile))
                this.Agent.Erase(translationFile);

            try
            {
                using (XmlTextWriter writer = new XmlTextWriter(this.Agent.OpenStream(translationFile), null))
                {
                    writer.Formatting = Formatting.Indented;
                    xdoc.WriteTo(writer);
                }
            }
            finally
            {
                this.Agent.CloseStream(translationFile);
            }
        }

        public IEnumerable<Dictionary<string, string>> ListStrings(string className)
        {
            XDocument doc = LoadDocument();
            XElement classElement = EnsureElement(doc.Root, className);

            foreach (XElement x in classElement.Elements())
            {
                foreach (XAttribute a in x.Attributes())
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("Code", x.Name.LocalName);
                    dic.Add("Language", a.Name.LocalName);
                    dic.Add("Translation", a.Value);

                    yield return dic;
                }
            }
        }

        public void AddString(string className, string code, string lang, string value)
        {
            if (String.IsNullOrEmpty(className)) throw new ArgumentNullException("className");
            if (String.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
            if (String.IsNullOrEmpty(lang)) throw new ArgumentNullException("lang");

            XDocument doc = LoadDocument();
            XElement classElement = EnsureElement(doc.Root, className);
            XElement element = EnsureElement(classElement, code);
            element.SetAttributeValue(lang, value);

            SaveDocument(doc);
        }

        public void RemoveString(string className, string code, string lang)
        {
            if (String.IsNullOrEmpty(className)) throw new ArgumentNullException("className");
            if (String.IsNullOrEmpty(code)) throw new ArgumentNullException("code");
            if (String.IsNullOrEmpty(lang)) throw new ArgumentNullException("lang");

            XDocument doc = LoadDocument();
            XElement classElement = EnsureElement(doc.Root, className);
            XElement element = EnsureElement(classElement, code);
            element.SetAttributeValue(lang, null);
            if (!element.HasAttributes)
                element.Remove();

            SaveDocument(doc);
        }

        XElement EnsureElement(XElement element, string name)
        {
            XElement result = element.Element(name);
            if (null == result)
            {
                result = new XElement(name);
                element.Add(result);
            }
            return result;
        }

        protected override DataSourceView GetView(string viewName)
        {
            if (String.IsNullOrEmpty(viewName))
                viewName = "/";

            return new TranslationClassView(this, viewName);
        }
    }

    public class TranslationClassView : DataSourceView
    {
        TranslationDataSource translationSource;
        string className;

        public TranslationClassView(TranslationDataSource source, string viewName)
            : base(source, viewName)
        {
            translationSource = source;
            className = viewName;
        }

        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            if (null == translationSource || className == "/")
                return null;
            return translationSource.ListStrings(className);
        }

        protected override int ExecuteInsert(IDictionary values)
        {
            int result;
            if (null == translationSource || className == "/")
                result = 0;
            else
            {
                translationSource.AddString(className, (string)values["Code"], (string)values["Language"], (string)values["Translation"]);
                result = 1;
            }
            return result;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            int result;
            if (null == translationSource || className == "/")
                result = 0;
            else
            {
                translationSource.AddString(className, (string)values["Code"], (string)values["Language"], (string)values["Translation"]);
                result = 1;
            }
            return result;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            int result;
            if (null == translationSource || className == "/")
                result = 0;
            else
            {
                translationSource.RemoveString(className, (string)keys["Code"], (string)keys["Language"]);
                result = 1;
            }
            return result;
        }
    }
}
