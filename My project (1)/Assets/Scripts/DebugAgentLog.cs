using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>Debug mode: NDJSON log to workspace debug-ca9fc4.log for agent analysis.</summary>
public static class DebugAgentLog
{
    const string LogFileName = "debug-ca9fc4.log";
    const string SessionId = "ca9fc4";

    static string GetLogPath()
    {
        try
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", LogFileName));
        }
        catch { return null; }
    }

    static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s)) return "\"\"";
        return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
    }

    public static void Log(string location, string message, string dataJson = null, string hypothesisId = null)
    {
        try
        {
            string path = GetLogPath();
            if (string.IsNullOrEmpty(path)) return;
            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sb = new StringBuilder();
            sb.Append("{\"sessionId\":").Append(Escape(SessionId)).Append(",\"location\":").Append(Escape(location))
                .Append(",\"message\":").Append(Escape(message)).Append(",\"timestamp\":").Append(ts);
            if (!string.IsNullOrEmpty(dataJson)) sb.Append(",\"data\":").Append(dataJson);
            if (!string.IsNullOrEmpty(hypothesisId)) sb.Append(",\"hypothesisId\":").Append(Escape(hypothesisId));
            sb.Append("}").AppendLine();
            File.AppendAllText(path, sb.ToString());
        }
        catch { }
    }
}
