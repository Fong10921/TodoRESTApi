namespace TodoRESTApi.Core.ExternalHelperInterface
{
    /// <summary>
    /// Provides helper methods for converting and formatting date and time values using NodaTime.
    /// </summary>
    public interface INodaTimeHelper
    {
        /// <summary>
        /// Converts the specified <paramref name="dateTime"/> to a UTC formatted string.
        /// The output string is formatted as "yyyy-MM-dd HH:mm:ss" using the invariant culture.
        /// </summary>
        /// <param name="dateTime">The date and time value to convert.</param>
        /// <returns>
        /// A string representing the provided <paramref name="dateTime"/> in UTC,
        /// formatted as "yyyy-MM-dd HH:mm:ss".
        /// </returns>
        string ConvertToUtcStringFormatted(DateTime dateTime);

        /// <summary>
        /// Converts the specified <paramref name="dateTime"/> to UTC and returns the result as a <see cref="DateTime"/>.
        /// The resulting <see cref="DateTime"/> is marked with <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="dateTime">The date and time value to convert.</param>
        /// <returns>
        /// A <see cref="DateTime"/> representing the UTC equivalent of the input <paramref name="dateTime"/>.
        /// </returns>
        DateTime? ConvertToUtcTimeFormatted(DateTime? dateTime);

        /// <summary>
        /// Converts the specified UTC <paramref name="dateTime"/> to the local time of the given time zone.
        /// The time zone is determined by the IANA time zone identifier provided in <paramref name="timeZoneString"/>.
        /// </summary>
        /// <param name="dateTime">The UTC date and time value to convert.</param>
        /// <param name="timeZoneString">
        /// The IANA time zone identifier (for example, "Asia/Kuala_Lumpur") to which the <paramref name="dateTime"/> should be converted.
        /// </param>
        /// <returns>
        /// A <see cref="DateTime"/> representing the local time in the specified time zone.
        /// Note that the returned <see cref="DateTime"/> has a <see cref="DateTimeKind"/> of <c>Unspecified</c>.
        /// </returns>
        DateTime ConvertToLocalTimeBasedOnTimeZone(DateTime dateTime, string timeZoneString);

        /// <summary>
        /// Formats the specified <paramref name="dateTime"/> as a string.
        /// The output string is formatted as "yyyy-MM-dd HH:mm:ss" using the invariant culture.
        /// </summary>
        /// <param name="dateTime">The date and time value to format.</param>
        /// <returns>
        /// A string representing the formatted <paramref name="dateTime"/>.
        /// </returns>
        string FormatDate(DateTime? dateTime);
    }
}
