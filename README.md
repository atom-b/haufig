# About
#### [**häu·fig**](https://en.wiktionary.org/wiki/h%C3%A4ufig), 1. frequent, common

Quick-and-dirty tool to generate a lemmatized vocabulary frequency list from an EPUB ebook.

# Setup
## Docker - Recommended

Using the Docker development container is the most straightforward way to run Haufig until there is a turn-key solution.

1. Clone this repository
1. [Install Visual Studio Code](https://code.visualstudio.com)
1. Follow [Microsoft's installation instructions](https://code.visualstudio.com/docs/remote/containers) to configure VS Code for development within a Docker container.
1. Start VS Code, then run the `Remote-Containers: Open Folder in Container` command and open the project directory.
   * For more detailed instructions see the section titled **Quick start: Open an existing folder in a container** in the Microsoft documentation.
1. In `.devcontainer/Dockerfile` change the value of `SPACY_MODEL=<...>` to whatever [spaCy model](https://spacy.io/models) you'd like to use for your language.

## Standalone

You will need: 
* A clone of this repository
* A working Python 3.8 install
* A [spaCy](https://spacy.io/usage) install with the [language model(s)](https://spacy.io/models) you want to use
* Ability to build and run [.NET 5.0](https://dotnet.microsoft.com/download) projects

# Usage
##### From within the project directory:
    dotnet run --project ./src/Haufig.Cli/Haufig.Cli.fsproj --books [<book>...] [--model <model>] [--output-dir <path>] [--book-csvs]

    OPTIONS:

        --books [<book>...]   Space-separated list of .epub files and/or directories to search for .epub files
        --model <model>       Name of the spaCy language model to use
        --output-dir <path>   Directory where the output CSV(s) will be written
        --book-csvs           Output individual CSVs for each book in addition to the merged output CSV
        --help                display this list of options.

##### Examples:

    $> dotnet run --project ./src/Haufig.Cli/Haufig.Cli.fsproj --books "ebooks/Der Tor und der Tod by Hugo von Hofmannsthal.epub" --output-dir "outputs/gutenberg/de" --model de_core_news_sm

    $> dotnet run --project ./src/Haufig.Cli/Haufig.Cli.fsproj --books "ebooks/Der Tor und der Tod.epub" "ebooks/Sidsel Langröckchen.epub" "ebooks/de" --output-dir "outputs/gutenberg/de" --model de_core_news_sm

    $> cat "output/de/gutenberg/results.csv" | more
    count,lemma,part of speech
    2270,ich,PRON
    2146,der,DET
    1446,und,CONJ
    775,sein,AUX
    553,sich,PRON
    451,der,PRON
    441,in,ADP
    393,haben,AUX
    -- More  --

# Caveats
* This has barely been tested.
* All sections of the ebook are processed including title page, copyright information, dedication, etc.
* NLP is an imperfect science and as such some words may not be lemmatized properly
