using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class LabelEx : Label
    {
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Label;
            }
        }
    }
}
