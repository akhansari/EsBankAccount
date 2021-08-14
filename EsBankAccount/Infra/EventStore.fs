[<RequireQualifiedAccess>]
/// A fake Event Store database
module EsBankAccount.Infra.EventStore

open System.Collections.Concurrent
open System.Text.Json
open System.Text.Json.Serialization

type StreamKey =
    { Name: string
      Id: string }

type Connection = ConcurrentDictionary<StreamKey, ResizeArray<string>>

let createConnection () : Connection =
    ConcurrentDictionary ()

let private getOrAdd (conn: Connection) key =
    conn.GetOrAdd (key, ResizeArray ())

let read<'T> conn key =
    key
    |> getOrAdd conn
    |> Seq.map JsonSerializer.Deserialize<'T>
    |> Seq.toList
    |> async.Return

let private jso =
    let jso = JsonSerializerOptions ()
    JsonFSharpConverter JsonUnionEncoding.FSharpLuLike |> jso.Converters.Add
    jso.WriteIndented <- true
    jso

let append<'T> (conn: Connection) key (events: 'T seq) =
    let history = getOrAdd conn key
    events
    |> Seq.map (fun e -> JsonSerializer.Serialize<'T> (e, jso))
    |> history.AddRange
    |> async.Return

let print (conn: Connection) =
    for (KeyValue (key, values)) in conn do
        printfn $"Key: %A{key}"
        printfn "Events:"
        for value in values do
            printfn $"{value}"
        printfn ""
