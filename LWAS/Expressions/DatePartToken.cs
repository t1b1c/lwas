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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Expressions
{
    public class DatePartToken : BasicToken
    {
        public enum DatePartEnum { None, Year, Month, Day, Hour, Minute, Second, Millisecond, DayOfYear, DayOfWeek }

        public override string Key
        {
            get { return "date part token"; }
        }

        public DatePartEnum DatePart { get; private set; }

        public override object Value
        {
            get 
            {
                if (this.DatePart == DatePartEnum.None)
                    return null;
                else
                    return (object)this.DatePart; 
            }
            set
            {
                string stringvalue = null;
                if (null != value)
                    stringvalue = value.ToString();

                base.Value = ((DatePartEnum)Enum.Parse(typeof(DatePartEnum), stringvalue));

                if (base.Value is DatePartEnum)
                    this.DatePart = (DatePartEnum)base.Value;
            }
        }

        public override bool IsValid
        {
            get { return this.Value is DatePartEnum; }
        }

        public override IResult Evaluate()
        {
            IResult result = base.Evaluate();
            if (this.Value is DatePartEnum)
                result.Status = ResultStatus.Successful;
            else
                result.Status = ResultStatus.Unsuccessful;
            return result;
        }
    }
}
