# About
##### [**häu·fig**](https://en.wiktionary.org/wiki/h%C3%A4ufig), 1. frequent, common

Quick-and-dirty tool to generate a lemmatized vocabulary frequency list from an EPUB ebook.

# Requirements
* A clone of this repository
* A working Python 3.7 install
* A [spaCy](https://spacy.io/usage) install with the [language model(s)](https://spacy.io/models) you want to use
* Ability to build and run [.NET Core 2.0](https://dotnet.microsoft.com/download) projects

# Usage
##### From a repository clone:
    dotnet run --project .\src\Haufig.Cli\Haufig.Cli.fsproj <path to epub> <path to csv output> <spaCy model name>

##### Example:

    $> dotnet run --project .\src\Haufig.Cli\Haufig.Cli.fsproj "C:\Der Tor und der Tod.epub" "Der_Tor_Frequency_List.csv" de_pytt_bertbasecased_lg

    $> cat "Der_Tor_Frequency_List.csv" | more
    der,321
    ich,210
    und,164
    sich,151
    sein,63
    einen,56
    nicht,55
    mein,54
    -- More  --

# Caveats
* This has barely been tested.
* All sections of the ebook are processed including title page, copyright information, dedication, etc.
* NLP is an imperfect science and as such some words may not be lemmatized properly