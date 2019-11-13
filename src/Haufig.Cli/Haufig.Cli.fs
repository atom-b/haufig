module Haufig.Cli
open System
open System.Collections.Generic
open FSharpPlus
open Argu

type CLIArguments =
    | [<ExactlyOnce>] Books of book:string list
    | [<Unique>] Model of model:string
    | [<Unique>] Output_Dir of path:string
    | [<Unique>] Book_CSVs
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Model _ -> "Name of the spaCy language model to use"
            | Books _ -> "Space-separated list of .epub files and/or directories to search for .epub files"
            | Output_Dir _ -> "Directory where the output CSV(s) will be written"
            | Book_CSVs _ -> "Output individual CSVs for each book in addition to the merged output CSV"

let searchDir endings path = 
    endings
    |> Seq.collect ( fun e -> IO.Directory.GetFiles(path, e, IO.SearchOption.AllDirectories))

let findFiles patterns paths = 
    paths
    |> Seq.map (fun p -> 
        if (IO.Directory.Exists(p)) then 
            p |> searchDir patterns
        else 
            seq {yield p}
    )
    |> Seq.collect id

let processBook model infile = 
    let book = Haufig.Lib.loadBook infile
    printfn "Generating frequency list from %s using model %s" infile model
    (book, Haufig.Lib.getFrequencyList book model)

let writeCSV path items =
    // poor man's CSV output
    let lines = items |> Seq.map (fun (w:KeyValuePair<string,int>) -> w.Key + "," + w.Value.ToString())
    System.IO.File.WriteAllLines (path, lines, Text.Encoding.UTF8)

[<EntryPoint>]
let main argv =
    let argParser = ArgumentParser.Create<CLIArguments>(programName = "haufig.exe")
    if argv.Length <= 0 then printfn "%s" (argParser.PrintUsage()); exit 1;

    let args = argParser.Parse argv
    let model = args.GetResult ( Model, defaultValue = @"de_core_news_md" )//  @"de_pytt_bertbasecased_lg" alternative model

    let csvOutDir = args.GetResult (Output_Dir, defaultValue = IO.Path.GetFullPath("."))
    let fullCsvPath = IO.Path.Combine(csvOutDir, "results.csv")
    
    if not (IO.Directory.Exists(csvOutDir)) then 
        printfn "output directory %s doesn't exist, creating..." csvOutDir
        IO.Directory.CreateDirectory csvOutDir |> ignore

    let multipleCSVs = args.Contains Book_CSVs

    let freqList = 
        args.GetResult Books
        |> findFiles ["*.epub"]
        |> Seq.map (
            (processBook model)
            >> ( fun (book, fl) -> 
                if multipleCSVs then 
                    let bookCsvPath = IO.Path.Combine(csvOutDir, book.Title + "_" + book.Author + "_freq"  + ".csv")
                    printfn "Writing %s results to %s..." book.Title bookCsvPath
                    writeCSV bookCsvPath fl

                fl |> Seq.map (fun i -> (i.Key, i.Value)) |> dict
            )
        )
        |> Seq.fold(fun acc bfl -> bfl |> Dict.unionWith (+) acc ) (dict Seq.empty<string * int>)
        |> Seq.sortByDescending (fun s -> s.Value)
    
    printfn "Writing full frequency list to %s" fullCsvPath
    writeCSV fullCsvPath freqList

    0
