# Internationalization (i18n) Guide for AppGroup

## Overview

AppGroup now supports internationalization through resource files and the `LocalizationManager` class. This allows the application to display text in different languages based on the user's system settings or user preference.

## Components

### 1. Resource Files

- **Location**: `AppGroup/Resources/`
- **Files**:
  - `Resources.resx` - Default (English) strings
  - `Resources.fr-FR.resx` - French strings
  
Each resource file contains key-value pairs for translatable strings. New languages can be added by creating additional `.resx` files with the appropriate culture suffix (e.g., `Resources.es-ES.resx` for Spanish).

### 2. LocalizationManager

**Location**: `AppGroup/Utilities/LocalizationManager.cs`

Static class that manages:
- Loading resource files
- Retrieving localized strings
- Changing cultures at runtime
- Format string with parameters

### 3. LanguageDetector

**Location**: `AppGroup/Utilities/LanguageDetector.cs`

Detects system language and provides utilities for language detection.

## Usage

### Initialization

Initialize the localization system early in your application startup (done in `Program.cs`):

```csharp
// In Program.cs Main() method:
LocalizationManager.Initialize(); // Auto-detect system language

// Or specify a culture:
LocalizationManager.Initialize("fr-FR");

// Set current thread culture
Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;
Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
```

### Retrieving Localized Strings

```csharp
// Get a simple string
string saveText = LocalizationManager.GetString("Button_Save");

// Get a formatted string with parameters
string errorMessage = LocalizationManager.GetString("Message_Error", "File not found");
```

### Changing Language at Runtime

```csharp
// By culture name
LocalizationManager.SetCulture("fr-FR");

// By CultureInfo
LocalizationManager.SetCulture(new CultureInfo("en-US"));
```

### Checking Supported Cultures

```csharp
// Get all supported cultures
CultureInfo[] cultures = LocalizationManager.GetSupportedCultures();

// Check if a specific culture is supported
bool isSupported = LocalizationManager.IsCultureSupported("es-ES");
```

### Detecting System Language

```csharp
// Get system language name (e.g., "en-US")
string languageName = LanguageDetector.GetSystemLanguageName();

// Get display name (e.g., "English (United States)")
string displayName = LanguageDetector.GetSystemLanguageDisplayName();

// Get two-letter ISO code (e.g., "en")
string isoCode = LanguageDetector.GetSystemTwoLetterLanguageName();

// Get best matching culture from supported list
CultureInfo bestMatch = LanguageDetector.GetBestMatchCulture(
    LocalizationManager.GetSupportedCultures()
);
```

## Adding a New Language

1. **Create a new resource file** in `AppGroup/Resources/`:
   - Name it `Resources.{culture-name}.resx` (e.g., `Resources.de-DE.resx`)
   - Copy all entries from `Resources.resx` and translate the values

2. **Update supported cultures** in `LocalizationManager.GetSupportedCultures()`:
   ```csharp
   public static CultureInfo[] GetSupportedCultures()
   {
       return new CultureInfo[]
       {
           CultureInfo.InvariantCulture,
           new CultureInfo("en-US"),
           new CultureInfo("fr-FR"),
           new CultureInfo("de-DE")  // Add new culture
       };
   }
   ```

3. **Update the project file** to include the new resource:
   ```xml
   <ItemGroup>
       <EmbeddedResource Include="Resources\Resources.resx" />
       <EmbeddedResource Include="Resources\Resources.fr-FR.resx" />
       <EmbeddedResource Include="Resources\Resources.de-DE.resx" />
   </ItemGroup>
   ```

## Applying to XAML

### Using Binding with Localization

In your XAML files, replace hardcoded text with bindings to localized strings:

**Before:**
```xml
<Button Content="Save" />
<TextBlock Text="Enter Title" />
```

**After (using x:Uid or custom binding):**

Option 1: Use a converter in binding (recommended for WinUI 3):

```xml
<Page.Resources>
    <local:LocalizationConverter x:Key="LocConverter" />
</Page.Resources>

<Button Content="{Binding Source={StaticResource LocConverter}, ConverterParameter=Button_Save}" />
```

Option 2: Set in code-behind:

```csharp
// In constructor or OnLoaded
SaveButton.Text = LocalizationManager.GetString("Button_Save");
TitleTextBox.PlaceholderText = LocalizationManager.GetString("EditGroup_TooltipPlaceholder");
```

### Creating a Localization Converter

Create a value converter for XAML bindings:

```csharp
public class LocalizationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string key)
        {
            return LocalizationManager.GetString(key);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
```

## Resource Key Naming Convention

Follow this convention for resource keys:

- **Buttons**: `Button_{Action}` (e.g., `Button_Save`, `Button_Cancel`)
- **Messages**: `Message_{Type}` (e.g., `Message_Error`, `Message_DeleteConfirm`)
- **Settings**: `Settings_{Category}_{Name}` (e.g., `Settings_Columns`, `Settings_ShowLabels`)
- **Labels**: `{Window}_{Element}` (e.g., `EditGroup_GroupNamePlaceholder`)
- **Layout Options**: `Layout_{Option}` (e.g., `Layout_Default`, `Layout_Card`)

## Current Translations

### English (en-US) - Default
All strings are defined in `Resources.resx`

### French (fr-FR)
Complete translation in `Resources.fr-FR.resx`

## Testing

Test your translations by:

1. Setting a specific culture:
   ```csharp
   LocalizationManager.SetCulture("fr-FR");
   ```

2. Using the system language (default behavior)

3. Verifying all UI elements display correctly

## Best Practices

1. **Externalize all user-visible strings** - Move hardcoded text to resource files
2. **Use descriptive keys** - Make keys clear and consistent
3. **Keep translations in sync** - Add new strings to all language files
4. **Test with RTL languages** - If supporting right-to-left languages like Arabic
5. **Handle missing translations gracefully** - The system returns the key if translation is missing
6. **Avoid string concatenation** - Use formatted strings with parameters instead:
   ```csharp
   // Bad: string text = "Error: " + errorMessage;
   // Good: LocalizationManager.GetString("Message_Error", errorMessage);
   ```

## Files Modified

- `AppGroup/AppGroup.csproj` - Added EmbeddedResource entries for .resx files
- `AppGroup/Program.cs` - Added LocalizationManager initialization
- `AppGroup/Utilities/LocalizationManager.cs` - New file
- `AppGroup/Utilities/LanguageDetector.cs` - New file
- `AppGroup/Utilities/LocalizationExample.cs` - Example usage
- `AppGroup/Resources/Resources.resx` - New file (English)
- `AppGroup/Resources/Resources.fr-FR.resx` - New file (French)
