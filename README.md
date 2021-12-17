# Bank account kata and Functional Event Sourcing

F# template/POC about Functional Event Sourcing, Onion Architecture and WebAssembly.

Wanna file an issue? a suggestion? Please feel free to [create a new issue](https://github.com/akhansari/EsBankAccount/issues/new) and / or [a pull request](https://github.com/akhansari/EsBankAccount/compare).\
Or [start a new discussion](https://github.com/akhansari/EsBankAccount/discussions/new) for questions, ideas, etc.

## Why?

### F#
Empowers everyone to write succinct, robust and performant code.\
It enables you to write backend (taking advantage of .Net ecosystem) as well as frontend (transpiled to JS or compiled to Wasm) applications.

### Functional Event Sourcing
Fully embrace immutability and expressions, in addition to other more traditional ES perks.

### Onion Architecture
Leads to more maintainable applications since it emphasizes separation of concerns throughout the system.\
It's even quite natural with F#, i.e. compositions and higher-order functions.

### WebAssembly
Facilitate the development of powerful UIs and back office apps with minimal effort.\
Note that for the sake of simplicity in this demo, the view and the business logic have both been put in the same project in order to make this application "hostable" on GitHub. \
When deployed to an actual real-world production environment, they are often located in separate projects with different lifecycles.

## Setup

- Install [.Net SDK 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) (Linux / Windows / macOS)
- To [B|T]DD: `dotnet watch test -p EsBankAccount.sln`
- To watch: `dotnet watch run -p EsBankAccount/EsBankAccount.fsproj`

Editors: [Vim](https://github.com/ionide/Ionide-vim) / [VSCode](https://marketplace.visualstudio.com/items?itemName=Ionide.Ionide-fsharp) / [VS Windows](https://visualstudio.microsoft.com/vs/community/) / [VS macOS](https://visualstudio.microsoft.com/vs/mac/)

## Kata

You can simply clone the `kata-start` branch and start practicing.\
Follow the instructions in `BankAccountTests.fs` and `BankAccountTests.state.fs`.

## Decider

Deciders should have, at least, an initial state and two functions:

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
        // what should happen
        Then // assert scenario
            ( function Ok events -> Assert.NotEmpty events | _ -> () )
        Then // true or false scenario
            ( function Ok [ Withdrawn _; Closed _ ] -> true | _ -> false )
        Then // equality then structural diff scenario
            [ Withdrawn { Amount = 100m; Date = DateTime.MinValue }
              Closed DateTime.MinValue ]
    }
```

There are two kinds of them:
1. Test what has been done (mandatory).
   - We don't mind how we come up with the outcome.
   - But, we do need to make sure that the outcome has to be correct under the given condition.
2. Test how it has been done (optional).
   - We aren't too concerned about the outcome.
   - But, we need to build the state in a particular way.

It should be noted that the BDD DSL style brings more readability and neat helpers but it isn't mandatory.\
In your test files you can have different kind of unit tests. For instance a test could be as simple as [this](https://github.com/thinkbeforecoding/UnoCore/blob/solution/Uno.Tests/Tests.fs).

### Decider Structure

It's possible to organize the Decider into five sections:
1. types
1. state logic
1. decision logic
1. validation (optional)
1. decision pipeline

Keep it in one file until it hurts and then decide the best split(s) at the last responsible moment.

### Decision Outcome

There are usually, at least, two categories of Deciders:
1. System `-> 'Event list`\
   Silent, if nothing has happened, then it will return an empty list. No need for validation.
1. Frontal `-> Result<'Event list, 'Error>`\
   When validation is required. For instance called from an API.\
   Could also be `-> Validation<'Event list, 'Error list>`.

### Validations

We could have different types of validation in each layer:
1. Domain: Enforce constraints on new events, business validation.
1. Application: Anti-corruption, validate infrastructures data.
1. Startup: Secure and validate data shape.

## Onion Architecture

- _Inner_ layers "aren't aware" of _outer_ layers.
- Domain is pure (i.e. think functional programming 101).
- App only has a reference to the domain.
- Infra only has references to other infrastructures.
- Startup has references to the App and the Infra. Infra are injected to the App.
- We usually start to code from the inside to the right (i.e. output), then again from the inside to the left (i.e. input).

<img src="assets/onion.png" alt="onion architecture" />

## Resources

- [Functional Event Sourcing](https://thinkbeforecoding.com/category/Event-Sourcing) by Jérémie Chassaing
- [State from Events or Events as State?](https://verraes.net/2019/08/eventsourcing-state-from-events-vs-events-as-state/) by Mathias Verraes
- [Temporal Modelling](https://verraes.net/2019/06/talk-temporal-modelling/) by Mathias Verraes
- [Expectations for an Event Store](https://github.com/ylorph/RandomThoughts/blob/master/2019.08.09_expectations_for_an_event_store.md) by Yves Lorphelin
- [Effective F#, tips and tricks](https://gist.github.com/swlaschin/31d5a0a2c4478e82e3ed60d653c0206b) by Scott Wlaschin
- [Equinox](https://github.com/jet/equinox) by Jet and Ruben Bartelink
- [Event Sourcing in .NET tutorials](https://github.com/oskardudycz/EventSourcing.NetCore) by Oskar Dudycz
