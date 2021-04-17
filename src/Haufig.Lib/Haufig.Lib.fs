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

type Tag = {id:string; desc:string}
type Pos = {id:string; desc:string}
type Token = {text:string; lemma:string; pos:Pos; tag:Tag; shape:string; isPunct:bool; isSpace:bool}
type LemmaCount = {id:string; token:Token; count:int}

let getHtmlDoc html =
    let doc = HtmlDocument()
    doc.LoadHtml(html)
    doc

let getTokens model bookText =
    // initialize spaCy
    printfn "Initializing python and spaCy..."
    use gil = Py.GIL()
    let spacy = Py.Import("spacy")
    let nlp = spacy?load(model)
    nlp?max_length <- 2000000 // ugly hack to get around text length maximum in spaCy, should just process the text in chunks
    
    printfn "Analyzing text..."
    // equivalent of nlp(bookText) in python
    let doc:PyObject = nlp?__call__(bookText)

    new PyIter(doc)
    |> Seq.cast<PyObject>
    |> Seq.map(fun pt -> {
        text=(pt?text).ToString();
        lemma=(pt?lemma_).ToString();
        // TODO: set part of speech and tag descriptions using spacy/glossary.py
        pos={
            id=(pt?pos_).ToString();
            desc="this is a part of speech"};
        tag={
            id=(pt?tag_).ToString();
            desc="this is a tag"};
        shape=(pt?shape_).ToString();
        isPunct=((pt?is_punct).ToString() = "True");
        isSpace=((pt?is_space).ToString() = "True")}
     )
    // "FM" -> "foreign language material"
    // "PROPN" -> proper nouns
    // "PUNCT" -> punctuation
    // See: spacy/glossary.py
    |> Seq.filter (fun t -> not t.isPunct && not t.isSpace && not (List.contains t.pos.id ["PROPN"; "FM"; "PUNCT"; "NUM"]))

let countLemmas tokens =
    printfn "Counting lemmas..."
    tokens
    // |> Seq.groupBy (fun t ->t.lemma.ToLower() + "-" + t.pos.id + "-" + t.tag.id)
    |> Seq.groupBy (fun t ->t.lemma.ToLower() + "-" + t.pos.id)
    |> Seq.map (fun (id,tcs) -> (id, { id=id; token = tcs|> Seq.head; count = tcs |> Seq.length }))
    |> dict

// poor man's attempt to extract raw text from the full epub markup
let getText (book:EpubBook) =
    printfn "Extracting text content of %s..." book.Title
    book.ReadingOrder
    |> Seq.map ((fun s -> getHtmlDoc s.Content) >> (fun h -> h.DocumentNode.SelectSingleNode("//body").SelectNodes("//text()")) )
    |> Seq.collect (id)
    |> Seq.filter (fun n -> not (n.ParentNode.Name = "script" || n.ParentNode.Name = "style" || String.IsNullOrWhiteSpace(n.InnerText)) )
    |> Seq.map ((fun n -> n.InnerText) >> HtmlEntity.DeEntitize)
    |> Seq.fold (fun (acc:Text.StringBuilder) (t:string) -> acc.AppendLine (t)) (Text.StringBuilder())
    |> (fun sb -> sb.ToString())

let getFrequencyList (book:EpubBook) (model:string) =
    book
    |> getText
    |> getTokens model
    |> countLemmas
    |> Seq.sortByDescending (fun s -> s.Value.count)

let loadBook (path:string) =
    EpubReader.ReadBook path
