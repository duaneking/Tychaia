//
// This source code is licensed in accordance with the licensing outlined
// on the main Tychaia website (www.tychaia.com).  Changes to the
// license on the website apply retroactively.
//
using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Tychaia.Website
{
    public static class MenuExtensions
    {
        public static MvcHtmlString MenuItem(
            this HtmlHelper htmlHelper,
            string text,
            string action,
            string controller
        )
        {
            var li = new TagBuilder("li");
            var routeData = htmlHelper.ViewContext.RouteData;
            var currentAction = routeData.GetRequiredString("action");
            var currentController = routeData.GetRequiredString("controller");
            if (string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(currentController, controller, StringComparison.OrdinalIgnoreCase))
            {
                li.AddCssClass("active");
            }
            li.InnerHtml = htmlHelper.ActionLink(text, action, controller).ToHtmlString();
            return MvcHtmlString.Create(li.ToString());
        }
    }
}

