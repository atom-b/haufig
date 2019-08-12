module Haufig.Cli
open System

[<EntryPoint>]
let main argv =
    let infile = argv.[0]
    let model = if argv.Length > 2 then argv.[2] else @"de_core_news_md" //  @"de_pytt_bertbasecased_lg" alternative model

    let book = Haufig.Lib.loadBook infile

    printfn "Generating frequency list from %s using model %s" infile model

    let frequencyList = Haufig.Lib.getFrequencyList book model

    let outfile = IO.Path.GetFullPath( (if argv.Length > 1 then argv.[1] else book.Title + "_" + book.Author + "_freq"  + ".csv") )
    // poor man's CSV output
    printfn "Writing frequency list to %s" (outfile)
    System.IO.File.WriteAllLines (outfile, frequencyList |> Seq.map (fun w -> w.Key + "," + w.Value.ToString()), Text.Encoding.UTF8) |> ignore

    0
