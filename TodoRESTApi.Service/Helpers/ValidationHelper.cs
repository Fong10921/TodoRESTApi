using System.ComponentModel.DataAnnotations;

namespace TodoRESTApi.Service.Helpers;

public class ValidationHelper
{
    /// <summary>
    /// Validates the given object using data annotation attributes.
    /// </summary>
    /// <param name="obj">The object to be validated.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when validation fails, containing the first validation error message.
    /// </exception>
    internal static void ModelValidation(object obj)
    {
        // Create a validation context for the given object
        ValidationContext validationContext = new ValidationContext(obj);

        // Store validation results
        List<ValidationResult> validationResults = new List<ValidationResult>();

        // Perform validation based on the object's data annotations
        bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

        // If validation fails, throw an exception with the first validation error message
        if (!isValid)
        {
            throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
        }
    }

}