namespace Annium.Extensions.Jobs;

public static class Interval
{
    public const string Secondly = "* * * * *";
    public const string Minutely = "0 * * * *";
    public const string Hourly = "0 0 * * *";
    public const string Daily = "0 0 0 * *";

    public static string InSeconds(uint seconds) => $"*/{seconds} * * * *";
    public static string InMinutes(uint minutes) => $"0 */{minutes} * * *";
    public static string InHours(uint hours) => $"0 0 */{hours} * *";
}