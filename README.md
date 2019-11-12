# About
#### [**häu·fig**](https://en.wiktionary.org/wiki/h%C3%A4ufig), 1. frequent, common

Quick-and-dirty tool to generate a lemmatized vocabulary frequency list from an EPUB ebook.

# Requirements
* A clone of this repository
* A working Python 3.7 install
* A [spaCy](https://spacy.io/usage) install with the [language model(s)](https://spacy.io/models) you want to use
* Ability to build and run [.NET Core 2.0](https://dotnet.microsoft.com/download) projects

# Usage
##### From a repository clone:
    dotnet run --project .\src\Haufig.Cli\Haufig.Cli.fsproj --books [<book>...] [--model <model>] [--output-file <path>]

    OPTIONS:

        --books [<book>...]   Space-separated list of .epub files to process.
        --model <model>       Name of the spaCy language model to use
        --output-file <path>  Path where the output CSV will be written
        --help                display this list of options.

##### Example:

    $> dotnet run --project .\src\Haufig.Cli\Haufig.Cli.fsproj --books "C:\Der Tor und der Tod.epub" "C:\Sidsel Langröckchen.epub" --output-file "frequency_list.csv" --model de_pytt_bertbasecased_lg

    $> cat "frequency_list.csv" | more
    der,2598
    ich,2272
    und,1446
    sein,779
    sich,601
    in,453
    mein,426
    zu,406
    haben,393
    -- More  --

# Caveats
* This has barely been tested.
* All sections of the ebook are processed including title page, copyright information, dedication, etc.
* NLP is an imperfect science and as such some words may not be lemmatized properly
