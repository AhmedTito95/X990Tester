using System;

namespace X990TesterCore
{
    public static class PcNcFrame
    {
        public static string Wrap(string json)
        {
            return $"~PCNC~{json.Length:D4}~{json}";
        }

        public static string Unwrap(string framed)
        {
            if (string.IsNullOrEmpty(framed))
                throw new ArgumentException("Frame cannot be null or empty", nameof(framed));

            int firstBrace = framed.IndexOf('{');
            int lastBrace = framed.LastIndexOf('}');
            
            if (firstBrace == -1 || lastBrace == -1 || firstBrace >= lastBrace)
                throw new FormatException($"Invalid JSON structure in frame: {framed}");
           
            return framed.Substring(firstBrace, (lastBrace - firstBrace) + 1);
        }
    }
}
