module Haufig.Cli
open System
open System.Collections.Generic
open FSharpPlus
open Argu

type CLIArguments =
    | [<ExactlyOnce>] Books of book:string list
    | [<Unique>] Model of model:string
    | [<Unique>] Output_File of path:string
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Model _ -> "Name of the spaCy language model to use"
            | Books _ -> "Space-separated list of .epub files to process."
            | Output_File _ -> "Path where the output CSV will be written"

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
    let model =
        match args.TryGetResult Model with
        | Some m -> m
        | _ -> @"de_core_news_md" //  @"de_pytt_bertbasecased_lg" alternative model
    
    let csvOut = 
        match args.TryGetResult Output_File with
        | Some p -> IO.Path.GetFullPath(p)
        | _ -> IO.Path.GetFullPath("results.csv")

    let freqList = 
        args.GetResult Books
        |> Seq.map (
            (processBook model)
            >> ( fun (book, fl) -> 
                writeCSV (IO.Path.GetFullPath(book.Title + "_" + book.Author + "_freq"  + ".csv")) fl
                fl |> Seq.map (fun i -> (i.Key, i.Value)) |> dict
            )
        )
        |> Seq.fold(fun acc bfl -> bfl |> Dict.unionWith (+) acc ) (dict Seq.empty<string * int>)
        |> Seq.sortByDescending (fun s -> s.Value)
    
    printfn "Writing frequency list to %s" (csvOut)
    writeCSV csvOut freqList

    0
