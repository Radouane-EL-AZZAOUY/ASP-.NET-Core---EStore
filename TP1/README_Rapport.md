# Guide de compilation du rapport LaTeX

## Présentation

Ce rapport académique présente le projet E-Store avec une page de garde professionnelle et une mise en page optimisée pour éviter les coupures de code entre les pages.

## Prérequis

Pour compiler le rapport LaTeX, vous devez avoir installé :

1. **LaTeX Distribution** :
   - Windows : [MiKTeX](https://miktex.org/download) ou [TeX Live](https://www.tug.org/texlive/)
   - Linux : `sudo apt-get install texlive-full` (Ubuntu/Debian)
   - macOS : [MacTeX](https://www.tug.org/mactex/)

2. **Éditeur LaTeX** (optionnel mais recommandé) :
   - [TeXstudio](https://www.texstudio.org/)
   - [Overleaf](https://www.overleaf.com/) (en ligne, **recommandé pour la simplicité**)
   - [VS Code avec extension LaTeX](https://marketplace.visualstudio.com/items?itemName=James-Yu.latex-workshop)

## Compilation

### Méthode 1 : Ligne de commande

```bash
# Compiler le document
pdflatex Rapport_Projet_E-Store.tex

# Compiler deux fois pour les références croisées
pdflatex Rapport_Projet_E-Store.tex

# Optionnel : Générer la bibliographie (si ajoutée)
bibtex Rapport_Projet_E-Store

# Compiler une dernière fois
pdflatex Rapport_Projet_E-Store.tex
```

### Méthode 2 : Avec TeXstudio

1. Ouvrir `Rapport_Projet_E-Store.tex` dans TeXstudio
2. Cliquer sur le bouton "Build & View" (F5)
3. Le PDF sera généré automatiquement

### Méthode 3 : Avec Overleaf (en ligne)

1. Aller sur [overleaf.com](https://www.overleaf.com/)
2. Créer un nouveau projet
3. Importer le fichier `Rapport_Projet_E-Store.tex`
4. Cliquer sur "Recompile"

## Améliorations apportées

✅ **Page de garde académique** avec :
- Nom de l'université (FSTM Fès)
- Titre professionnel du rapport
- Informations de l'étudiant (Radouane EL AZZAOUY, ILISI, 2025/2026)
- Design épuré et professionnel

✅ **Mise en page optimisée** :
- En-têtes et pieds de page avec numérotation
- Code source sans coupures entre les pages (`float=h!`)
- Table des matières sans bordures rouges
- Formatage cohérent et professionnel

✅ **Navigation améliorée** :
- Liens hypertextes en noir (sans surbrillance rouge)
- Table des matières interactive
- Structure hiérarchique claire

## Structure du rapport

Le rapport contient les sections suivantes :

1. **Page de garde académique** : Informations de l'étudiant et de l'université
2. **Table des matières** : Navigation interactive
3. **Introduction** : Vue d'ensemble du projet
4. **Architecture Globale** : Structure de l'application en couches
5. **Patterns de Conception** :
   - Repository Pattern
   - Unit of Work Pattern
   - DTO Pattern
   - Service Layer Pattern
6. **Système de Cache** :
   - Pourquoi implémenter un cache
   - Architecture et implémentation
   - Stratégie d'invalidation
   - Rôles et avantages
7. **Stratégie de Tests** :
   - Framework xUnit
   - Types de tests (unitaires et d'intégration)
   - Couverture des tests (25 tests)
8. **Conclusion** : Résumé et perspectives d'évolution
9. **Annexes** : Technologies et structure détaillée

## Personnalisation

Pour modifier le rapport :

- **Auteur** : Modifier `\author{...}` dans le préambule
- **Titre** : Modifier `\title{...}`
- **Sections** : Ajouter/modifier les sections avec `\section{...}`
- **Code** : Les blocs de code utilisent le package `listings` avec coloration syntaxique

## Packages LaTeX utilisés

- `geometry` : Marges du document
- `babel[french]` : Typographie française
- `listings` : Coloration syntaxique pour le code C#
- `hyperref` : Liens hypertextes (sans bordures rouges)
- `fancyhdr` : En-têtes et pieds de page personnalisés
- `float` : Positionnement des blocs de code
- `xcolor` : Couleurs pour le code

## Caractéristiques techniques

- Format A4 avec marges de 2.5cm
- Police de 12pt pour une meilleure lisibilité
- Code source avec numérotation des lignes
- Pas de coupures de code entre les pages
- Diagrammes ASCII pour illustrer l'architecture
- 25+ pages de contenu technique détaillé

