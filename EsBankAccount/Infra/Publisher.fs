namespace EsBankAccount.Infra

type Publisher<'T> () =
    let event = Event<'T> ()

    let report msg =
        try
            event.Trigger msg
        with e ->
            printfn $"Publisher failed: %A{e}"

    let agent = MailboxProcessor<'T>.Start(fun inbox ->
        async {
            while true do
                let! msg = inbox.Receive()
                report msg
        })

    member _.Subscribe callback = event.Publish.Add callback
    member _.Post = agent.Post
