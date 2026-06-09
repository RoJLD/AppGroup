using AppGroup.Logging;
using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace AppGroup.Utilities
{
    /// <summary>
    /// Manages application localization using resource files.
    /// Supports dynamic language switching and system language detection.
    /// </summary>
    public static class LocalizationManager
    {
        private static ResourceManager _resourceManager = null!;
        private static CultureInfo? _currentCulture;

        /// <summary>
        /// Gets the current UI culture.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => _currentCulture ??= Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value != _currentCulture)
                {
                    _currentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    Thread.CurrentThread.CurrentCulture = value;
                    Logger.Info("Culture changed to: " + value.Name);
                }
            }
        }

        /// <summary>
        /// Gets the ResourceManager for the application resources.
        /// </summary>
        private static ResourceManager ResourceManager
        {
            get
            {
                if (_resourceManager == null)
                {
                    _resourceManager = new ResourceManager("AppGroup.Resources.Resources", typeof(LocalizationManager).Assembly);
                }
                return _resourceManager;
            }
        }

        /// <summary>
        /// Initializes the LocalizationManager with the system's default language.
        /// </summary>
        public static void Initialize()
        {
            Initialize((string?)null);
        }

        /// <summary>
        /// Initializes the LocalizationManager with a specific culture.
        /// </summary>
        /// <param name="cultureName">The culture name (e.g., "en-US", "fr-FR"). If null, uses system language.</param>
        public static void Initialize(string? cultureName)
        {
            try
            {
                if (string.IsNullOrEmpty(cultureName))
                {
                    // Detect system language
                    var systemCulture = LanguageDetector.DetectSystemLanguage();
                    SetCulture(systemCulture);
                }
                else
                {
                    var culture = new CultureInfo(cultureName);
                    SetCulture(culture);
                }
                Logger.Info("LocalizationManager initialized with culture: " + CurrentCulture.Name);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize LocalizationManager");
                // Fallback to invariant culture
                SetCulture(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Sets the current culture for the application.
        /// </summary>
        /// <param name="culture">The culture to use.</param>
        public static void SetCulture(CultureInfo culture)
        {
            CurrentCulture = culture;
        }

        /// <summary>
        /// Sets the current culture by name.
        /// </summary>
        /// <param name="cultureName">The culture name (e.g., "en-US", "fr-FR").</param>
        public static void SetCulture(string cultureName)
        {
            try
            {
                var culture = new CultureInfo(cultureName);
                SetCulture(culture);
            }
            catch (CultureNotFoundException ex)
            {
                Logger.Warn("Culture not found: " + cultureName + " — " + ex.Message);
            }
        }

        /// <summary>
        /// Gets a localized string by its resource key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string, or the key if not found.</returns>
        public static string GetString(string key)
        {
            try
            {
                string? value = ResourceManager.GetString(key, CurrentCulture);
                return value ?? key; // Return key if not found
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to get localized string for key: " + key);
                return key;
            }
        }

        /// <summary>
        /// Gets a localized string with format parameters.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted localized string, or the key if not found.</returns>
        public static string GetString(string key, params object[] args)
        {
            string format = GetString(key);
            if (args == null || args.Length == 0)
                return format;
            
            try
            {
                return string.Format(CurrentCulture, format, args);
            }
            catch (FormatException ex)
            {
                Logger.Error(ex, "Format error for key: " + key);
                return format;
            }
        }

        /// <summary>
        /// Gets all supported cultures.
        /// </summary>
        public static CultureInfo[] GetSupportedCultures()
        {
            return new CultureInfo[]
            {
                CultureInfo.InvariantCulture,
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR")
            };
        }

        /// <summary>
        /// Checks if a culture is supported.
        /// </summary>
        /// <param name="cultureName">The culture name to check.</param>
        /// <returns>True if the culture is supported.</returns>
        public static bool IsCultureSupported(string cultureName)
        {
            try
            {
                var culture = new CultureInfo(cultureName);
                return Array.Exists(GetSupportedCultures(), c => c.Name.Equals(culture.Name, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}
