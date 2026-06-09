using AppGroup.Utilities;
using System;
using System.Globalization;

namespace AppGroup
{
    /// <summary>
    /// Example class showing how to use the localization system in AppGroup.
    /// 
    /// Usage patterns:
    /// 
    /// 1. Basic string lookup:
    ///    string saveButtonText = LocalizationManager.GetString("Button_Save");
    /// 
    /// 2. Formatted strings:
    ///    string message = LocalizationManager.GetString("Message_Error", errorMessage);
    /// 
    /// 3. Change language at runtime:
    ///    LocalizationManager.SetCulture("fr-FR");
    /// 
    /// 4. Get current culture:
    ///    CultureInfo current = LocalizationManager.CurrentCulture;
    /// 
    /// 5. Check if language is supported:
    ///    bool isSupported = LocalizationManager.IsCultureSupported("es-ES");
    /// 
    /// 6. Initialize with specific language (call this early in app startup):
    ///    LocalizationManager.Initialize("fr-FR");
    ///    
    /// 7. Initialize with system language (default):
    ///    LocalizationManager.Initialize();
    /// </summary>
    public static class LocalizationExample
    {
        public static void DemonstrateLocalization()
        {
            // Example 1: Get localized strings
            string saveText = LocalizationManager.GetString("Button_Save");
            string cancelText = LocalizationManager.GetString("Button_Cancel");
            
            Console.WriteLine($"Save: {saveText}, Cancel: {cancelText}");

            // Example 2: Use formatted strings
            string error = LocalizationManager.GetString("Message_Error", "File not found");
            Console.WriteLine(error);

            // Example 3: Switch languages
            LocalizationManager.SetCulture("fr-FR");
            saveText = LocalizationManager.GetString("Button_Save");
            Console.WriteLine($"French Save: {saveText}");

            // Example 4: Switch back to English
            LocalizationManager.SetCulture("en-US");
            saveText = LocalizationManager.GetString("Button_Save");
            Console.WriteLine($"English Save: {saveText}");

            // Example 5: Get all supported cultures
            var cultures = LocalizationManager.GetSupportedCultures();
            Console.WriteLine("Supported cultures:");
            foreach (var culture in cultures)
            {
                Console.WriteLine($"  - {culture.Name} ({culture.DisplayName})");
            }

            // Example 6: Detect system language
            string systemLang = LanguageDetector.GetSystemLanguageName();
            string systemLangDisplay = LanguageDetector.GetSystemLanguageDisplayName();
            Console.WriteLine($"System language: {systemLang} ({systemLangDisplay})");
        }
    }
}
