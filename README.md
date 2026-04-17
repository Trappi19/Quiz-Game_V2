# Quiz-Game_V2

Projet de quiz développé sous Unity, orienté **apprentissage / démonstration technique** autour :
- d’un système de thèmes et de scoring,
- d’un système de rôles avec bonus,
- d’un système de sauvegarde/reprise,
- d’un historique de parties exportable en PDF.

---

## ⚠️ Limitation importante (état actuel)

Ce projet **n’est pas directement jouable sur une autre machine** que celle de développement actuelle.

Raison principale :
- la base est en **MariaDB locale** (`localhost:3306`),
- la chaîne de connexion est codée en dur dans les scripts (ex. `QuizManager`, `RoleSystem`) avec des paramètres locaux (`quizgame`, `root`, etc.).

Sans reproduire exactement cette base et sa configuration, le jeu ne peut pas charger ses données.

---

## Objectif du projet

Le but est de proposer un quiz multi-thèmes avec une couche “jeu” via des rôles :
- progression par thèmes,
- bonus/effets uniques selon le rôle choisi,
- gestion des scores par thème et score total,
- persistance des parties et reprise exacte.

---

## Fonctionnement global

1. **Menu principal**
   - Nouvelle partie (saisie du pseudo)
   - Chargement de sauvegarde (3 slots)
   - Historique des runs

2. **Sélection du rôle**
   - Les rôles sont chargés depuis la table `role`
   - Sauvegarde du rôle choisi (`PlayerPrefs`)
   - Transition visuelle puis chargement du premier thème

3. **Quiz par thème**
   - 5 thèmes (`Culture générale`, `Musique`, `Cinéma`, `Sport`, `Géographie`)
   - questions chargées depuis MariaDB
   - 4 choix par question
   - progression question par question
   - logique “boss” sur fin de thème (bonus de points selon règles actuelles)

4. **Fin de thème / fin de run**
   - affichage score du thème
   - passage au thème suivant
   - total calculé sur les 5 thèmes

---

## Système de rôles (bouton d’action unique)

Le projet utilise un **bouton d’action unique** (`skipButton`) dont le texte et l’effet changent selon le rôle (`IRoleEffect`, `RoleEffectFactory`).

### Rôles implémentés

- **Débugueur** (`id=1`)  
  Passe une question, donne +1 (1 fois / thème)

- **Hacker** (`id=2`)  
  Grise 2 mauvaises réponses (2 fois / thème)

- **Compilateur** (`id=3`)  
  Affiche un indice (limité par thème, et anti double-usage sur la même question)

- **Cache** (`id=4`)  
  Rejoue une question déjà répondue (tous thèmes confondus), 1 fois / thème  
  Si bonne réponse : +1 (même si c’était une ex-boss)

- **DevOps** (`id=5`)  
  Remplace la question par une question “difficile” (`question_difficile`), 1 fois / thème  
  Bonne réponse : +3  
  Mauvaise réponse : -1

- **Multi Language** (`id=6`)  
  Remplace la question courante par une question de secours (`question_secoure`), 1 fois / thème

- **Fullstack** (`id=9`)  
  Passe une question pour +2, 1 fois / run

---

## Sauvegarde / reprise

Gestion via `PlayerPrefs` :
- 3 slots de sauvegarde,
- sauvegarde du joueur, rôle, thème, index question, ordre des questions, scores, usages de bonus,
- reprise exacte (incluant états de bonus en cours selon le rôle).

---

## Historique & export PDF

- Historique des runs (`HistorySystem`, `HistoryManager`)
- Affichage des détails (score total + détail par thème)
- Export PDF via `iTextSharp` (`PDFGenerator`)

---

## Transitions & outils dev

- Transitions de scène via Animator (`Fade`, et transition ajoutée aussi sur la scène de rôle)
- Logs de score de dev entre les questions (`[DevScore] ...`)
- Backdoor de test : `SkipThemeForDebug()` dans `QuizManager` (skip thème sans points ni historique)

---

## Stack technique

- **Moteur** : Unity (projet `6000.3.9f1`)
- **Langage** : C#
- **Target** : `.NET Standard 2.1`
- **BDD** : MariaDB
- **Accès BDD** : `MySqlConnector`
- **UI** : Unity UI (+ TextMeshPro)
- **Persistence locale** : `PlayerPrefs`
- **PDF** : `iTextSharp`

---

## Structure principale

- `Assets/Scripts/StartGame/`
  - menu, chargement, historique, PDF
- `Assets/Scripts/Rôles/`
  - système de rôle, effets, factory
- `Assets/Scripts/QnA/`
  - gestion quiz, scoring, questions/réponses, game state
- `Assets/Scripts/bdd/`
  - docs de structure SQL

---

## Schéma de données utilisé (principales tables)

- `role`
- `quiz_questions` / `quiz_question`
- `quiz_choices`
- `question_secoure`
- `question_difficile`
- `themes`

---

## Rendre le projet jouable sur une autre machine

Minimum requis :
1. Installer MariaDB
2. Créer la base `quizgame`
3. Importer toutes les tables et données attendues
4. Adapter les chaînes de connexion (`host`, `port`, `user`, `password`) dans les scripts
5. Vérifier les scènes Unity et les références Inspector (boutons, animator, panels)

---

## État du projet

Projet en évolution continue, avec ajout progressif de rôles et de fonctionnalités de test/debug.
```