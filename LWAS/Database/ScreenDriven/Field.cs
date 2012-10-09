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

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Database.ScreenDriven
{
    public class Field
    {
        string _name;
        public string Name
        {
            get { return _name; }
        }

        string _type;
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        string _control;
        public string Control
        {
            get { return _control; }
        }

        string _member;
        public string Member
        {
            get { return _member; }
        }

        string _linkedScreen;
        public string LinkedScreen
        {
            get { return _linkedScreen; }
            set { _linkedScreen = value; }
        }

        string _linkedContainer;
        public string LinkedContainer
        {
            get { return _linkedContainer; }
            set { _linkedContainer = value; }
        }

        string _linkedField;
        public string LinkedField
        {
            get { return _linkedField; }
            set { _linkedField = value; }
        }

        bool _isReference;
        public bool IsReference
        {
            get { return _isReference; }
            set { _isReference = value; }
        }

        bool _isExcluded = false;
        public bool IsExcluded
        {
            get { return _isExcluded; }
            set { _isExcluded = value; }
        }

        bool _isUnique = false;
        public bool IsUnique
        {
            get { return _isUnique; }
            set { _isUnique = value; }
        }

        public Field(string name, string type, string control, string member)
        {
            _name = name;
            _type = type;
            _control = control;
            _member = member;
        }

        public Field(string name, string type, string control, string member, string linkedScreen, string linkedContainer, string linkedField, bool isReference, bool isExcluded, bool isUnique)
            : this(name, type, control, member)
        {
            _linkedScreen = linkedScreen;
            _linkedContainer = linkedContainer;
            _linkedField = linkedField;
            _isReference = isReference;
            _isExcluded = isExcluded;
            _isUnique = isUnique;
        }

        public string ToSql()
        {
            string sql = null;
            string sqltype = null;
            switch (_type)
            {
                case "Text":
                    sqltype = "varchar(max)";
                    break;
                case "Date":
                    sqltype = "datetime";
                    break;
                case "Number":
                    sqltype = "decimal(18,4)";
                    break;
                case "Boolean":
                    sqltype = "bit";
                    break;
                case "Identifier":
                    sqltype = "int";
                    break;
                default:
                    sqltype = _type;
                    break;
            }

            sql = _name + " " + sqltype;
            return sql;
        }
    }
}
