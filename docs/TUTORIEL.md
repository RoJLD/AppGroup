# Tutoriel AppGroup

AppGroup permet de regrouper plusieurs applications/raccourcis dans un **groupe**
unique, que l'on épingle à la barre des tâches ou à la zone de notification.
Un clic sur l'icône du groupe ouvre un petit **popup-lanceur** affichant toutes les
applications du groupe — comme un « dossier d'applications » sur un téléphone.

Deux interfaces existent :

- **GUI** (`AppGroup.exe`) — l'application graphique principale (gestion + lancement).
- **CLI** (`AppGroup.Aot.exe`) — un outil en ligne de commande autonome (Native AOT).

Les deux lisent/écrivent la **même** configuration :
`%LOCALAPPDATA%\AppGroup\appgroups.json`. Un groupe créé dans l'une apparaît dans
l'autre.

---

## Partie A — L'interface graphique (GUI)

### A.1 Démarrer l'application

Lancez `AppGroup.exe`. La fenêtre principale **« App Group »** s'ouvre.
L'application reste résidente : selon les paramètres, elle peut démarrer avec
Windows et afficher une icône dans la zone de notification.

### A.2 La fenêtre principale

| Élément | Rôle |
|---------|------|
| Barre de recherche « Search groups… » | Filtre les groupes en temps réel |
| Bouton `+` | Crée un nouveau groupe |
| Bouton rafraîchir (⟳) | Recharge la liste depuis la configuration |
| Menu `…` (en haut) | Import / Export / Force Taskbar Update / Settings |
| Liste des groupes | Un groupe par ligne : icône, nom, aperçu des raccourcis |
| Bouton crayon (par groupe) | Ouvre le groupe en **édition** |
| Menu `…` (par groupe) | Open File Location / Duplicate / Delete |

Tant qu'aucun groupe n'existe, la fenêtre affiche **« No Groups Found! — Click + to
create a Group »**.

> Note : cliquer/sélectionner un groupe dans la liste l'ouvre en **édition**, pas en
> lancement. Pour *lancer* un groupe, voir la section **A.7**.

### A.3 Créer un groupe

1. Cliquez sur **`+`**.
2. La fenêtre **« Edit Group »** s'ouvre sur un nouveau groupe vide.
3. Saisissez un nom dans le champ **Group Name** (en haut).

### A.4 Ajouter des raccourcis à un groupe

Dans la fenêtre d'édition, section des items, deux méthodes :

- **Bouton `+` (add)** : ouvre un sélecteur de fichiers. Formats acceptés :
  `.exe`, `.lnk`, `.url`.
- **Glisser-déposer** : faites glisser un ou plusieurs fichiers (`.exe`, `.lnk`,
  `.url`) directement dans la liste.

L'icône de chaque application est extraite automatiquement.

### A.5 Personnaliser un raccourci

Cliquez sur le bouton **crayon** d'un item pour ouvrir sa boîte d'édition :

| Champ | Effet |
|-------|-------|
| **Title** | Le libellé/tooltip affiché sous l'icône |
| **Argument** | Arguments de ligne de commande passés au lancement |
| **Icône** | Icône personnalisée (bouton parcourir) ; **Reset** revient à l'icône d'origine |

Pour **retirer** un raccourci : cliquez sur le **`X`** de la ligne.
Pour **réordonner** : glissez-déposez les items dans la liste.

### A.6 Personnaliser l'apparence du groupe

Bouton **icône du groupe** (en haut) : choisir une image (`.png`, `.jpg`, `.ico`,
`.exe`, `.url`) **ou** générer automatiquement une **icône-grille** composée des
icônes des applications du groupe.

Bouton **« customize » (⚙)** — options du popup-lanceur :

| Option | Valeurs | Effet |
|--------|---------|-------|
| **Columns** | 1 … N | Nombre de colonnes dans le popup |
| **Layout** | Default, Card | Style de fond de la zone |
| **Header** | on/off | Affiche le nom du groupe en en-tête du popup |
| Header Position | Top, Bottom | Position de l'en-tête (désactivé si 1 colonne) |
| **Labels** | on/off | Affiche le libellé sous chaque icône |
| Label Size | 8–14 | Taille de police du libellé |
| Label Position | Right, Bottom | Position du libellé (désactivé si Labels off) |
| **Show on Tray** | on/off | Place une icône du groupe dans la zone de notification |

Les options dépendantes se grisent automatiquement (ex. Label Size/Position si
Labels est désactivé).

### A.7 Lancer un groupe (le cœur d'AppGroup)

Le lancement ne se fait **pas** depuis la fenêtre principale, mais via une icône de
groupe que vous activez :

1. **Épingler à la barre des tâches / au bureau** : dans la fenêtre principale,
   **glissez l'icône d'un groupe** vers le bureau ou la barre des tâches. Cela crée
   un raccourci (`.lnk`) du groupe.
2. **Cliquez sur ce raccourci** : un **popup** s'ouvre, affichant toutes les
   applications du groupe (selon vos réglages de colonnes/labels/header).
3. **Cliquez sur une application** dans le popup pour la lancer (avec ses
   arguments). Le popup se referme ensuite.

Autres points d'entrée de lancement :

- **Zone de notification (tray)** : si **Show on Tray** est activé, le groupe
  apparaît près de l'horloge ; un clic ouvre le popup.
- **Liste de raccourcis (jump list)** : clic droit sur l'icône d'AppGroup épinglée
  → entrées **« Edit this Group »** et **« Launch All »** (lance toutes les
  applications du groupe d'un coup).

### A.8 Gérer les groupes

Menu `…` d'un groupe (fenêtre principale) :

- **Open File Location** — ouvre le dossier de stockage du groupe.
- **Duplicate** — duplique le groupe.
- **Delete** — supprime le groupe (avec confirmation).

Réordonner les groupes : glissez-déposez les lignes dans la liste.

### A.9 Importer / Exporter

Menu `…` (en haut de la fenêtre) :

- **Export** — sauvegarde la configuration dans un fichier **`.agz`**.
- **Import → .agz** — restaure depuis un fichier `.agz`.
- **Import → TaskbarGroups** — importe des groupes depuis l'application
  *TaskbarGroups*.
- **Force Taskbar Update** — force le rafraîchissement des icônes de la barre des
  tâches.

### A.10 Paramètres (Settings)

Menu `…` → **Settings** :

- **Comportement** : démarrer avec Windows ; afficher l'icône de la zone de
  notification.
- **Animations** : animation de la fenêtre (glissement) ; animation du contenu.
- **Apparence** : thème de l'application (Light / Dark / System) ; thème du popup ;
  fond en couleur d'accentuation ; icônes en niveaux de gris.
- **À propos** : version, vérifier les mises à jour au démarrage, bouton
  « Check Now ».

Les réglages sont enregistrés dans `%LOCALAPPDATA%\AppGroup\settings.json`.

---

## Partie B — La ligne de commande (CLI Native AOT)

`AppGroup.Aot.exe` est un exécutable **autonome** (aucun .NET requis) qui gère les
mêmes groupes que la GUI. Pour le construire, voir
[`AppGroup.Aot/README.aot.md`](../AppGroup.Aot/README.aot.md).

### B.1 Commandes

```
AppGroup.Aot                                   Mode interactif
AppGroup.Aot list                              Liste les groupes
AppGroup.Aot add <groupe>                      Crée un groupe vide
AppGroup.Aot additem <groupe> <chemin> [nom] [args] [icone]   Ajoute un raccourci
AppGroup.Aot edititem <groupe> <chemin> [--name <n>] [--args <a>] [--icon <i>]  Modifie un raccourci
AppGroup.Aot moveitem <groupe> <chemin> <index>   Réordonne un raccourci (index 0-based)
AppGroup.Aot removeitem <groupe> <chemin>      Supprime un raccourci
AppGroup.Aot listitems <groupe>                Liste les raccourcis d'un groupe
AppGroup.Aot export <groupe> <fichier>         Exporte un groupe en JSON
AppGroup.Aot import <fichier> [nom]            Importe un groupe depuis un JSON
AppGroup.Aot launch <groupe>                   Lance toutes les applications du groupe
AppGroup.Aot --help                            Aide
```

### B.2 Exemple complet

```bat
AppGroup.Aot add "MesOutils"
AppGroup.Aot additem "MesOutils" "C:\Windows\System32\notepad.exe" "Bloc-notes"
AppGroup.Aot additem "MesOutils" "C:\Windows\System32\cmd.exe" "Invite" "/k echo salut"
AppGroup.Aot moveitem "MesOutils" "C:\Windows\System32\cmd.exe" 0
AppGroup.Aot listitems "MesOutils"
AppGroup.Aot launch "MesOutils"
```

### B.3 Coexistence GUI ↔ CLI

La CLI et la GUI partageant `appgroups.json`, un groupe créé en ligne de commande
apparaît dans la GUI après un rafraîchissement (bouton ⟳), et inversement.

---

## Où sont stockées les données

| Chemin | Contenu |
|--------|---------|
| `%LOCALAPPDATA%\AppGroup\appgroups.json` | Configuration des groupes (partagée GUI/CLI) |
| `%LOCALAPPDATA%\AppGroup\settings.json` | Paramètres de l'application (GUI) |
| `%LOCALAPPDATA%\AppGroup\Groups\<nom>\` | Icônes et raccourci `.lnk` de chaque groupe |
| `%LOCALAPPDATA%\AppGroup\Icons\` | Icônes extraites des applications |
| `%LOCALAPPDATA%\AppGroup\lastEdit` | Dernier identifiant de groupe édité (interne) |

---

## Cas d'usage typique

1. Ouvrez `AppGroup.exe`.
2. `+` → nommez le groupe (« Dev », « Jeux », « Bureautique »…).
3. Glissez-y vos applications (ou via le bouton `+`).
4. Réglez les colonnes/labels via **customize**, et activez **Show on Tray** si
   souhaité.
5. Glissez l'icône du groupe sur la **barre des tâches** pour l'épingler.
6. Cliquez sur l'icône épinglée : le popup s'ouvre, cliquez sur l'application
   voulue.

Vous avez ainsi un « dossier » d'applications accessible en un clic depuis la barre
des tâches.
