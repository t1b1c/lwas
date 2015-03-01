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

using LWAS.Extensible.Interfaces;

namespace LWAS.Expressions
{
	public class ExpressionResult : IResult
	{
		private ResultStatus _status = ResultStatus.Successful;
		private List<Exception> _exceptions = new List<Exception>();
		public ResultStatus Status
		{
			get
			{
				return this._status;
			}
			set
			{
				this._status = value;
			}
		}
		public IList<Exception> Exceptions
		{
			get
			{
				return this._exceptions;
			}
		}
		public bool IsSuccessful()
		{
			return this.Status == ResultStatus.Successful;
		}
		public void Concatenate(IResult result)
		{
            if (null == result)
                return;

			if (this.Status == ResultStatus.Successful && !result.IsSuccessful())
				this.Status = ResultStatus.Unsuccessful;

            foreach (Exception exception in result.Exceptions)
			{
				this._exceptions.Add(exception);
			}
		}
	}
}
