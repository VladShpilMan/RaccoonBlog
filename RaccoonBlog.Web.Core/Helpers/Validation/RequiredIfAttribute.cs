using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RaccoonBlog.Web.Core.Helpers.Validation;

public class RequiredIfAttribute : ValidationAttribute,  IClientModelValidator
{
    private readonly RequiredAttribute _innerAttribute = new RequiredAttribute();

    private string DependentProperty { get; set; }
    private object TargetValue { get; set; }
    
    public RequiredIfAttribute(string dependentProperty, object targetValue)
    {
        DependentProperty = dependentProperty;
        TargetValue = targetValue;
    }
    
    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        
        var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
        
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-requiredif", errorMessage);
        
        var depProp = "*." + DependentProperty;
        MergeAttribute(context.Attributes, "data-val-requiredif-dependentproperty", depProp);
        
        string targetValue = (TargetValue ?? "").ToString();
        
        if (TargetValue != null && TargetValue is bool)
        {
            targetValue = targetValue.ToLower();
        }

        MergeAttribute(context.Attributes, "data-val-requiredif-targetvalue", targetValue);
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var containerType = validationContext.ObjectInstance.GetType();
        var field = containerType.GetProperty(this.DependentProperty);

        if (field != null)
        {
            var dependentvalue = field.GetValue(validationContext.ObjectInstance, null);
            
            if ((dependentvalue == null && TargetValue == null) ||
                (dependentvalue != null && dependentvalue.Equals(this.TargetValue)))
            {
                if (!_innerAttribute.IsValid(value))
                {
                    return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });
                }
            }
        }

        return ValidationResult.Success;
    }
    
    private void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
        {
            attributes.Add(key, value);
        }
    }
}