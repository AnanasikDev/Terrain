using UnityEngine;

public static class WillowDebug
{
    public const string Green = "#00FF00FF";
    public const string Yellow = "#FFFF00FF";
    public static void Log(string message, string color)
    {
        if (WillowTerrainSettings.debugMode)
            Debug.Log(FormatLog(message, color));
    }
    public static void Log(string message, string color, System.Action<string> logFunction)
    {
        if (WillowTerrainSettings.debugMode) 
            logFunction(FormatLog(message, color));
    }
    public static string FormatLog(string message, string color)
    {
        return LogPrefix() + ": " + $"<b><color={color}>{message}</color></b>";
    }
    public static string LogPrefix()
    {
        string c0 = "#00FF00FF";
        string c1 = "#00FE00FF";
        string c2 = "#00EE00FF";
        string c3 = "#00DE00FF";
        string c4 = "#00CE00FF";
        string c5 = "#00BE00FF";
        string c6 = "#00AE00FF";
        string c7 = "#00AA00FF";

        return $"" +
            $"<color={c0}>[</color>" +
            $"<color={c1}>W</color>" +
            $"<color={c2}>i</color>" +
            $"<color={c3}>l</color>" +
            $"<color={c4}>l</color>" +
            $"<color={c5}>o</color>" +
            $"<color={c6}>w</color>" +
            $"<color={c7}>]</color>";
    }
}
