using Microsoft.AspNet.Razor.Runtime.TagHelpers;
 
namespace Img2Txt.TagHelpers
{
    [HtmlTargetElement("form", Attributes = "asp-ajax")]
    public class AjaxTagHelper : TagHelper
    {
        [HtmlAttributeName("asp-ajax")]
        public bool EnableAjax { set; get; }
        
        [HtmlAttributeName("post-success")]
        public string OnSuccessFunction { set; get; }
        
        [HtmlAttributeName("post-failed")]
        public string OnFailedFunction { set; get; }
 
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var controller = context.AllAttributes["asp-controller"]?.Value;
            var action = context.AllAttributes["asp-action"]?.Value;
            var postSuccess = context.AllAttributes["post-success"]?.Value;
            if (EnableAjax)
            {
                string script = "$.post('" + controller + "/" + action + 
                "',$(this).serializeArray(),function(data){"+ postSuccess +"},\"json\");return false;";
                output.Attributes["onsubmit"] = "javascript:" + script;
            }
        }
    }
}