module Haufig.Lib

open System
open FSharp.Interop.Dynamic

open VersOne.Epub
open HtmlAgilityPack
open Python.Runtime

// operators for case insensitive comparison, should be moved to their own library
let (=~) a b  = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) = 0
let (<>~) a b = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) <> 0
let (<~) a b  = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) < 0
let (<=~) a b = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) <= 0
let (>~) a b  = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) > 0
let (>=~) a b = System.String.Compare(a, b, StringComparison.OrdinalIgnoreCase) >= 0

let getHtmlDoc html =
    let doc = HtmlDocument()
    doc.LoadHtml(html)
    doc

let getLemmas model bookText =
    // initialize spaCy
    use gil = Py.GIL()
    let spacy = Py.Import("spacy")
    let nlp = spacy?load(model)
    printfn "Analyzing text..."
    // equivalent of nlp(bookText) in python
    let doc:PyObject = nlp?__call__(bookText)

    new PyIter(doc)
    |> Seq.cast<PyObject>
    |> Seq.filter (fun t -> (((t?is_punct).ToString() = "False") && ((t?is_space).ToString() = "False")))
    |> Seq.map (fun t -> ((t?lemma_).ToString()) )

let countStrings strings =
    printfn "Analyzing lemma frequency..."
    strings
    |> Seq.map (fun (s:string) -> s.ToLower())
    |> Seq.fold (fun (dict:Collections.Generic.Dictionary<string, int>) (s:string) ->
        if dict.ContainsKey(s) then
            dict.[s] <- dict.[s] + 1
        else
            dict.Add(s, 1)
        dict
        ) (Collections.Generic.Dictionary<string, int>())

// poor man's attempt to extract raw text from the full epub markup
let getText (book:EpubBook) =
    printfn "Extracting text content of %s..." book.Title
    book.ReadingOrder
    |> Seq.map ((fun s -> getHtmlDoc s.Content) >> (fun h -> h.DocumentNode.SelectSingleNode("//body").SelectNodes("//text()")) )
    |> Seq.collect (id)
    |> Seq.filter (fun n -> not (n.ParentNode.Name = "script" || n.ParentNode.Name = "style" || String.IsNullOrWhiteSpace(n.InnerText)) )
    |> Seq.fold (fun (acc:Text.StringBuilder) (n:HtmlNode) -> acc.AppendLine (n.InnerText) ) (Text.StringBuilder())
    |> (fun sb -> sb.ToString())

let getFrequencyList (book:EpubBook) (model:string) =
    book
    |> getText
    |> getLemmas model
    |> countStrings
    |> Seq.sortByDescending (fun s -> s.Value)

let loadBook (path:string) =
    EpubReader.ReadBook path