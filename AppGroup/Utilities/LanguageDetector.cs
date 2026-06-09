using AppGroup.Logging;
using System;
using System.Globalization;

namespace AppGroup.Utilities
{
    /// <summary>
    /// Detects the system language and provides language-related utilities.
    /// </summary>
    public static class LanguageDetector
    {
        private static CultureInfo _detectedCulture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Detects the system's preferred UI language.
        /// </summary>
        /// <returns>The detected CultureInfo.</returns>
        public static CultureInfo DetectSystemLanguage()
        {
            try
            {
                // Try to get the user's Windows display language (installed UI language)
                var userLanguage = CultureInfo.InstalledUICulture;
                Logger.Debug("Detected user UI language: " + userLanguage.Name);
                return userLanguage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to detect system UI language");
            }

            try
            {
                // Fallback to current UI culture
                var currentUiCulture = CultureInfo.CurrentUICulture;
                Logger.Debug("Fallback to CurrentUICulture: " + currentUiCulture.Name);
                return currentUiCulture;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get CurrentUICulture");
            }

            // Ultimate fallback
            Logger.Warn("Using invariant culture as fallback");
            return CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Gets the best matching culture from supported cultures based on system language.
        /// </summary>
        /// <param name="supportedCultures">Array of supported cultures.</param>
        /// <returns>The best matching culture, or the first supported culture if none match.</returns>
        public static CultureInfo GetBestMatchCulture(CultureInfo[] supportedCultures)
        {
            if (supportedCultures == null || supportedCultures.Length == 0)
                return CultureInfo.InvariantCulture;

            var systemCulture = DetectSystemLanguage();
            
            // Try exact match
            foreach (var culture in supportedCultures)
            {
                if (culture.Name.Equals(systemCulture.Name, StringComparison.OrdinalIgnoreCase))
                    return culture;
            }

            // Try language-only match (e.g., "fr" matches "fr-FR")
            string systemLanguage = systemCulture.TwoLetterISOLanguageName;
            foreach (var culture in supportedCultures)
            {
                if (culture.TwoLetterISOLanguageName.Equals(systemLanguage, StringComparison.OrdinalIgnoreCase))
                    return culture;
            }

            // Return first supported culture as fallback
            Logger.Info("No exact language match found. Using fallback: " + supportedCultures[0].Name);
            return supportedCultures[0];
        }

        /// <summary>
        /// Gets the system language name (e.g., "en-US", "fr-FR").
        /// </summary>
        public static string GetSystemLanguageName()
        {
            return DetectSystemLanguage().Name;
        }

        /// <summary>
        /// Gets the system language display name (e.g., "English (United States)", "Français (France)").
        /// </summary>
        public static string GetSystemLanguageDisplayName()
        {
            return DetectSystemLanguage().DisplayName;
        }

        /// <summary>
        /// Gets the two-letter ISO language name (e.g., "en", "fr").
        /// </summary>
        public static string GetSystemTwoLetterLanguageName()
        {
            return DetectSystemLanguage().TwoLetterISOLanguageName;
        }
    }
}
