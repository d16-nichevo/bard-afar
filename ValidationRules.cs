using System;
using System.IO;
using System.Windows.Controls;

namespace BardAfar
{
    // Validation rules for WPF controls.

    /// <summary>
    /// A validator for the "Host or IP Address" field.
    /// </summary>
    /// <remarks>
    /// The value must be a valid URI, or "*" or "+".
    /// For more information about the "*" or "+", see:
    /// https://learn.microsoft.com/en-gb/windows/win32/http/urlprefix-strings
    /// </remarks>
    internal class ValidationRuleHostOrIp : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult r = new ValidationResult(true, null);
            string host = (string)value;
            if (Uri.CheckHostName(host) == UriHostNameType.Unknown
                && host != "*" && host != "+")
            {
                return new ValidationResult(false, "Invalid host or IP address.");
            }
            return r;
        }
    }

    /// <summary>
    /// A validator for the "Audio Files Directory" field.
    /// </summary>
    /// <remarks>
    /// It must not be null, empty, or whitespace.
    /// It must be a path to a directory that exists.
    /// </remarks>
    internal class ValidationRuleAudioDir : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            ValidationResult r = new ValidationResult(true, null);
            if (String.IsNullOrWhiteSpace((string)value) || !Directory.Exists((string)value))
            {
                return new ValidationResult(false, "Directory does not exist.");
            }
            return r;
        }
    }
}
