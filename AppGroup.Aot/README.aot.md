# AppGroup.Aot — Native AOT CLI

Version **Native AOT** d'AppGroup : un exécutable Windows **autonome** (aucun .NET
Runtime requis sur la machine cible) qui gère des groupes de raccourcis via une
interface en ligne de commande.

La configuration est lue/écrite dans `%LOCALAPPDATA%\AppGroup\groups.json`, au
**même format** que la version WPF → coexistence et migration transparentes.

---

## Prérequis de build

Native AOT ne se limite pas au compilateur C# : l'étape finale invoque l'éditeur
de liens natif **`link.exe` de MSVC** pour produire l'exécutable. Il faut donc :

- Le **SDK .NET 8** (`dotnet`).
- **Visual Studio 2022/2026** ou les **Build Tools**, avec la charge de travail
  *« Développement Desktop en C++ »* (composant `VC.Tools.x86.x64`).

> Sans ces outils C++, le build s'arrête à `Generating native code` avec une erreur
> du linker (`link.exe`, code 123).

---

## Construire l'exécutable

### Méthode recommandée — script fourni

```bat
build-aot.bat
```

Le script :
1. localise l'installation Visual Studio via `vswhere.exe` (chemin absolu) ;
2. initialise l'environnement développeur x64 (`vcvars64.bat`) ;
3. lance `dotnet publish` avec les bons paramètres.

### Méthode manuelle

Depuis une **« x64 Native Tools Command Prompt for VS »** (ou après avoir appelé
`vcvars64.bat`) :

```bat
dotnet publish -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true -p:SelfContained=true
```

> ⚠️ Utiliser la forme `-p:Propriété=valeur` (et **non** `/p:`). Sous bash/PowerShell,
> le `/` initial de `/p:` est interprété comme un chemin et provoque l'erreur
> `MSB1008: Un seul projet peut être spécifié`.

Le binaire produit :

```
bin\x64\Release\net8.0\win-x64\publish\AppGroup.Aot.exe   (~2,5 Mo, autonome)
```

---

## Utilisation

```
AppGroup.Aot                                   Mode interactif
AppGroup.Aot list                              Liste les groupes
AppGroup.Aot add <group>                       Crée un groupe vide
AppGroup.Aot additem <group> <path> [name] [args] [icon]   Ajoute un raccourci
AppGroup.Aot edititem <group> <path> [--name <n>] [--args <a>] [--icon <i>]  Modifie un raccourci
AppGroup.Aot moveitem <group> <path> <index>   Réordonne un raccourci (index 0-based)
AppGroup.Aot removeitem <group> <path>         Supprime un raccourci
AppGroup.Aot listitems <group>                 Liste les raccourcis d'un groupe
AppGroup.Aot export <group> <file>             Exporte un groupe en JSON
AppGroup.Aot import <file> [name]              Importe un groupe depuis un JSON
AppGroup.Aot launch <group>                    Lance tous les raccourcis du groupe
AppGroup.Aot --help                            Aide
```

### Exemple

```bat
AppGroup.Aot add "MesOutils"
AppGroup.Aot additem "MesOutils" "C:\Windows\System32\notepad.exe" "Bloc-notes"
AppGroup.Aot additem "MesOutils" "C:\Windows\System32\calc.exe" "Calculatrice"
AppGroup.Aot moveitem "MesOutils" "C:\Windows\System32\calc.exe" 0
AppGroup.Aot listitems "MesOutils"
AppGroup.Aot launch "MesOutils"
```

---

## Limites du Native AOT (et comment elles sont gérées ici)

### Sérialisation JSON — pas de réflexion

En Native AOT, la **sérialisation JSON par réflexion est désactivée** :

```
System.InvalidOperationException: Reflection-based serialization has been disabled
for this application. Either use the source generator APIs ...
```

➡️ La solution appliquée est le **source generator** de `System.Text.Json` :

- [`Json/AppJsonContext.cs`](Json/AppJsonContext.cs) — contexte source-gen déclarant
  les types sérialisables (`[JsonSerializable(typeof(...))]`). Le code de
  (dé)sérialisation est généré à la compilation : **zéro réflexion**, trim-safe.
- [`Json/AppGroupConfigJsonConverter.cs`](Json/AppGroupConfigJsonConverter.cs) —
  converter custom qui reproduit le format « à plat » du WPF (chaque groupe est une
  propriété racine indexée par son ID). `[JsonExtensionData]` **n'est pas utilisable**
  en AOT (ni avec un dictionnaire fortement typé), d'où ce converter manuel piloté par
  `Utf8JsonReader`/`Utf8JsonWriter`.

**Règle d'or :** tout nouveau type (dé)sérialisé doit être ajouté à `AppJsonContext`
avec `[JsonSerializable]`, sinon crash à l'exécution.

### Réflexion générale

Toute API reposant sur la réflexion dynamique (`Type.GetType(string)`,
`Activator.CreateInstance` sur type non statique, sérialiseurs réflexifs, etc.) peut
être supprimée par le *trimmer* ou échouer en AOT. Le compilateur émet des
avertissements **IL2026 / IL3050 / IL2104 / IL3053** : ils doivent être traités, pas
ignorés — ils annoncent des plantages à l'exécution.

### Trimming

`PublishTrimmed=true` supprime le code non référencé statiquement. Du code atteint
uniquement par réflexion peut donc disparaître silencieusement. Privilégier les
chemins statiquement analysables (source generators, appels directs).

---

## Dépannage

| Symptôme | Cause | Correctif |
|----------|-------|-----------|
| `MSB1008: Un seul projet...` | `/p:` interprété comme un chemin | Utiliser `-p:` |
| `'vswhere.exe' n'est pas reconnu` puis `link.exe` code 123 | Dossier `...\Installer` absent du PATH | Lancer `build-aot.bat` (appelle vswhere par chemin absolu) |
| `error CS0579: attribut ... en double` | Fichiers générés d'un sous-projet ramassés par le projet parent | Le sous-projet `temp/` est exclu dans le `.csproj` |
| `Reflection-based serialization has been disabled` | Type JSON absent du contexte source-gen | Ajouter `[JsonSerializable(typeof(...))]` dans `AppJsonContext` |
