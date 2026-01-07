using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AyendeBlog.Web.TagHelpers;

[HtmlTargetElement("recaptcha-script", TagStructure = TagStructure.WithoutEndTag)]
public class RecaptchaScriptTagHelper : TagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "script";
        output.Attributes.SetAttribute("src", "https://www.google.com/recaptcha/api.js");
        output.Attributes.SetAttribute("async", null);
        output.Attributes.SetAttribute("defer", null);
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}

[HtmlTargetElement("recaptcha-widget", TagStructure = TagStructure.WithoutEndTag)]
public class RecaptchaWidgetTagHelper : TagHelper
{
    private readonly IConfiguration _config;

    public RecaptchaWidgetTagHelper(IConfiguration config)
    {
        _config = config;
    } 
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var siteKey = _config["Recaptcha:SiteKey"];

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "g-recaptcha");
        output.Attributes.SetAttribute("data-sitekey", siteKey);
        output.TagMode = TagMode.StartTagAndEndTag;
    }
}