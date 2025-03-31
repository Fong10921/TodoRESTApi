using NodaTime;
using NodaTime.Text;
using TodoRESTApi.Core.ExternalHelperInterface;

namespace TodoRESTApi.Infrastructure.ExternalHelper;

public class NodaTimeHelper: INodaTimeHelper
{
    public string ConvertToUtcStringFormatted(DateTime dateTime)
    {
        return FormatDate(dateTime);
    }
    
    public DateTime? ConvertToUtcTimeFormatted(DateTime? dateTime)
    {
        if (dateTime == null)
        {
            return null;
        }
        
        var pattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");

        string formattedDate = FormatDate(dateTime);
        
        var parseResult = pattern.Parse(formattedDate);
        
        if (!parseResult.Success)
        {
            throw new FormatException("The provided date string is not in the correct format.");
        }
        
        Instant instant = parseResult.Value;
        
        return instant.ToDateTimeUtc();
    }
    
    public DateTime ConvertToLocalTimeBasedOnTimeZone(DateTime dateTime, string timeZoneString)
    {
        DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timeZoneString];
        
        var utcDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        
        Instant instant = Instant.FromDateTimeUtc(utcDateTime);
        
        ZonedDateTime zonedDateTime = instant.InZone(timeZone);

        return zonedDateTime.ToDateTimeUnspecified();
    }
    
    public string FormatDate(DateTime? dateTime)
    {
        if (dateTime == null)
            return string.Empty;

        // Convert to UTC if necessary.
        DateTime utcDateTime = dateTime.Value.Kind == DateTimeKind.Utc 
            ? dateTime.Value 
            : dateTime.Value.ToUniversalTime();

        var pattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");
        Instant instant = Instant.FromDateTimeUtc(utcDateTime);
    
        return pattern.Format(instant);
    }

}