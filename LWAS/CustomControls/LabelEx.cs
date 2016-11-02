using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
    public class LabelEx : Label
    {
        public bool HasData { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (HasData)
                    return HtmlTextWriterTag.Span;
                else
                    return HtmlTextWriterTag.Label;
            }
        }

    }
}
