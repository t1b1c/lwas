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
using System.Linq;

namespace LWAS.Database
{
    public enum SortingOptions { None, Up, Down }

    public class ViewSorting
    {
        public View View { get; private set; }
        public List<FieldSorting> SortedFields { get; private set; }

        public ViewSorting(View view)
        {
            if (null == view) throw new ArgumentNullException("view");

            this.View = view;
            this.SortedFields = new List<FieldSorting>();
        }

        public FieldSorting this[string fieldNameOrAlias]
        {
            get
            {
                if (String.IsNullOrEmpty(fieldNameOrAlias)) throw new ArgumentNullException("fieldNameOrAlias");
                
                // find by alias
                var tableFieldAliases = this.View.Aliases.Where(kvp => kvp.Key is TableField);
                var alias = tableFieldAliases
                    .SingleOrDefault(kvp => kvp.Value == fieldNameOrAlias)
                    .Key;
                TableField field = alias as TableField;

                // find by field name
                if (field == null)
                {
                    var listAliases = tableFieldAliases.Select(kvp => kvp.Key as TableField);
                    field = this.View.Fields
                        .Except(listAliases)
                        .SingleOrDefault(f => f.Name == fieldNameOrAlias);
                }

                if (null == field) throw new ArgumentException(String.Format("View '{0}' doesn't have a field '{1}'", this.View.Name, fieldNameOrAlias));

                FieldSorting sortedField = this.SortedFields.SingleOrDefault(sf => sf.Field == field);
                if (null == sortedField)
                {
                    sortedField = new FieldSorting(field);
                    this.SortedFields.Add(sortedField);
                }
                return sortedField;
            }
        }
    }
}
