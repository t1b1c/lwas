/*
 * Copyrigt 2013-2015 Tiberiu Craciun, tiberiu.craciun@gmail.com
 * All rights reserved
*/

using System;
using System.Collections;
using System.Web.UI;

namespace LWAS.WebParts.Editors
{
	public class NoEditView : CRUDDataSourceView
	{
		public NoEditView(IDataSource owner, string viewName) : base(owner, viewName)
		{
		}
		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			return null;
		}
		protected override int ExecuteInsert(IDictionary values)
		{
			return 0;
		}
		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return 0;
		}
		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
		{
			return 0;
		}
	}
}
