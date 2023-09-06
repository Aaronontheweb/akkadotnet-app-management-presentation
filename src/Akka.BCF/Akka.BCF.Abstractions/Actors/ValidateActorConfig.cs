using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Akka.BCF.Abstractions.Actors;

public class ValidateActorConfig : IValidateOptions<ActorConfig>
{
    public ValidateOptionsResult Validate(string name, ActorConfig options)
    {
        // run DataAnnotations validation against the options
        var context = new ValidationContext(options, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(options, context, results, true);
        
        if (!isValid)
        {
            var errors = results.Select(x => x.ErrorMessage);
            return ValidateOptionsResult.Fail(errors);
        }
        else
        {
            return ValidateOptionsResult.Success;
        }
    }
}