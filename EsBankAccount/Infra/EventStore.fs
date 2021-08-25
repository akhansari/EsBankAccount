[<RequireQualifiedAccess>]
/// A fake Event Store database
module EsBankAccount.Infra.EventStore

open System.Collections.Concurrent
open System.Text.Json
open System.Text.Json.Serialization

type StreamKey =
    { Name: string
      Id: string }

type Publisher<'T> () =
    let event = Event<'T> ()
    let report msg =
        try
            event.Trigger msg
        with e ->
            printfn $"Publisher failed: %A{e}"
    let agent = MailboxProcessor<'T>.Start(fun inbox -> async {
        while true do
            let! msg = inbox.Receive()
            report msg
        })
    member _.Subscribe callback = event.Publish.Add callback
    member _.Post = agent.Post

    type Connection =
        { Db: ConcurrentDictionary<StreamKey, ResizeArray<string>>
          Publisher: Publisher<StreamKey * string> }

let createConnection () =
    { Db = ConcurrentDictionary ()
      Publisher = Publisher () }

let private getOrAdd (conn: Connection) key =
    conn.Db.GetOrAdd (key, ResizeArray ())

let private jso =
    let jso = JsonSerializerOptions ()
    JsonFSharpConverter JsonUnionEncoding.FSharpLuLike |> jso.Converters.Add
    jso.WriteIndented <- true
    jso

let read<'T> conn key =
    key
    |> getOrAdd conn
    |> Seq.map (fun json -> JsonSerializer.Deserialize<'T> (json, jso))
    |> Seq.toList
    |> async.Return

let append<'T> conn key (events: 'T seq) =
    let history = getOrAdd conn key
    for event in events do
        let json = JsonSerializer.Serialize<'T> (event, jso)
        conn.Publisher.Post (key, json)
        history.Add json
    async.Return ()
