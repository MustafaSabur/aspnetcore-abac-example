namespace AbacExample.Api.Authorization;

public static class BooleanClaimValues
{
    public const string True = "true";
    public const string False = "false";

    public static string FromBoolean(bool value) => value ? True : False;
}
