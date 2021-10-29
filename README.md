# Bank account kata and functional Event Sourcing

__Work in progress__

## Setup

- Install [.Net SDK 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) (Linux / Windows / macOS)
- To TDD: `dotnet watch test -p EsBankAccount.sln`
- To watch: `dotnet watch run -p EsBankAccount/EsBankAccount.fsproj`

Editors:
- [Vim](https://github.com/ionide/Ionide-vim)
- [VSCode](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp)
- VisualStudio [Windows](https://visualstudio.microsoft.com/vs/community/) / [macOS](https://visualstudio.microsoft.com/vs/mac/)

## Kata

You can clone the `kata-start` branch and start practicing.\
Follow the instructions in `BankAccountTests.fs` and `BankAccountTests.state.fs`.

## Decider

Deciders should have at least an initial state and two functions:

- `evolve: 'State -> 'Event -> 'State`\
  Given the current state and what happened, evolve to a new state.

  - From new events: `fold evolve currentState newEvents`
  - From the history: `fold evolve initialState history`

- `decide: 'Command -> 'State -> 'Outcome`\
  Given what has been requested and the current state, decide what should happen.

They are composable:

<img src="assets/decider.png" alt="decider" />

### Decider Tests

It's very convenient to create [Given-When-Then](EsBankAccount.Tests/Domain/BankAccountTests.fs) tests.

```fsharp
[<Fact>]
let ``close the account and withdraw the remaining amount`` () =
    spec {
        Given // history
            [ Deposited { Amount = 100m; Date = DateTime.MinValue } ]
        When  // command
            ( Close DateTime.MinValue )
        Then  // what should happen
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue }
              Closed DateTime.MinValue ]
    }
```

There are two kinds of them:
1. Test what is done (mandatory).\
   We don't mind how we come up with the outcome, except the outcome has to be correct under the given condition.
2. Test how it is done (optional).\
   We are not concerned about the outcome. The only thing needed is to build the state in a particular way.

### Decider Structure

It's possible to organize the Decider into five sections.\
So it will be easier to split it into separate files if it starts to grow.

|   | Section               | Filename
|---|-----------------------|----------
| 1 | types                 | BankAccount.types.fs
| 2 | state logic           | BankAccount.state.fs
| 3 | decision logic        | BankAccount.decisions.fs (could be one file per decision)
| 4 | validation (optional) | BankAccount.validations.fs
| 5 | decision pipeline     | BankAccount.pipeline.fs

### Decision Outcome

Usually there is at least two kind of Deciders:
1. System `-> 'Event list`\
   Silent, if nothing has happened then it will return an empty list. No need for validation.
2. Frontal `-> Result<'Event list, 'Error>`\
   When validation is required. For instance called from an API.\
   Could also be `-> Validation<'Event list, 'Error list>`.

## Onion Architecture

- Internal layers doesn't know about outside layers.
- Domain is pure.
- App only has a reference to the domain.
- Infra only has references to other infrastructures.
- Startup has references to the App and the Infra. Infra are injected to the App.
- We usually start to code from the inside to the right (output), then again from the inside to the left (input).

<img src="assets/onion.png" alt="onion architecture" />

## Resources

- [Functional Event Sourcing](https://thinkbeforecoding.com/category/Event-Sourcing) by Jérémie Chassaing
- [State from Events or Events as State?](https://verraes.net/2019/08/eventsourcing-state-from-events-vs-events-as-state/) by Mathias Verraes
- [Temporal Modelling](https://verraes.net/2019/06/talk-temporal-modelling/) by Mathias Verraes
- [Expectations for an Event Store](https://github.com/ylorph/RandomThoughts/blob/master/2019.08.09_expectations_for_an_event_store.md) by Yves Lorphelin
- [Effective F#, tips and tricks](https://gist.github.com/swlaschin/31d5a0a2c4478e82e3ed60d653c0206b) by Scott Wlaschin
- [Equinox](https://github.com/jet/equinox) by Jet and Ruben Bartelink
