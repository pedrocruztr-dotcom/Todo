# Todo

A small todo list, built the way a real system is built: a separate backend and frontend,
layers that the compiler enforces, and SQL written by hand with Dapper.

It is deliberately small. Every project here exists because something would be wrong without
it, not because a template suggested it.

## Layout

```
src/
├── Todo.Domain           the types and their invariants, depends on nothing
├── Todo.Application      what may be done, and the port the database must satisfy
├── Todo.Infrastructure   Dapper and SQL Server, implements that port
├── Todo.Contracts        the shapes that cross HTTP
├── Todo.Api              backend: endpoints, mapping, error responses
└── Todo.Web              frontend: Blazor, talks to the API over HTTP
tests/
├── Todo.Domain.Tests
└── Todo.Infrastructure.Tests
```

The reference graph is the design, and the compiler holds it:

| Project | References |
|---|---|
| `Todo.Domain` | nothing |
| `Todo.Application` | Domain |
| `Todo.Infrastructure` | Application |
| `Todo.Contracts` | nothing |
| `Todo.Api` | Application, Infrastructure, Contracts |
| `Todo.Web` | **Contracts only** |

`Todo.Web` cannot reach the domain or the database, because it cannot see them. That is what
makes the separation real rather than a folder convention.

`Todo.Application` declares `ITodoRepository` and `Todo.Infrastructure` implements it, so the
dependency points inward: the database serves the application, never the reverse.

## Running it

Requires the .NET 10 SDK and SQL Server LocalDB, which ships with Visual Studio. The database
and its schema are created on first start, so there is nothing to set up.

Two terminals:

```
dotnet run --project src/Todo.Api
dotnet run --project src/Todo.Web
```

The frontend reads the backend address from `TodoApi:BaseAddress` in `src/Todo.Web/appsettings.json`.

```
dotnet test
```

## Decisions worth knowing

**Invariants live in types.** `TodoTitle` cannot hold a blank or over-long string, so nothing
below the parse has to check again. `TodoItem` has no `IsDone` flag: it has a nullable
`CompletedAt`, which makes "done but with no completion time" impossible to represent.

**Toggling is one statement.** A read followed by a write is a race the database eventually
loses. `ToggleAsync` is a single conditional `UPDATE` with an `OUTPUT` clause, so it is atomic
and still returns the new row. There is a test that races two callers and asserts each applied
exactly once.

**Every Dapper call carries a `CommandDefinition`.** The short overloads take no
`CancellationToken`, so a token threaded carefully through every layer would stop dead at the
database. The `TodoTitle` type handler also sets an explicit parameter size, because a plain
string parameter reaches SQL Server as `nvarchar(4000)` and loses the index on a `varchar`
column.

**Prerendering is off on the page.** An interactive component prerenders by default and runs
`OnInitializedAsync` twice, which here would mean two calls to the API. Turning prerendering
off costs first paint and buys a single fetch; persisting the prerendered state would be the
alternative.

**Retries skip unsafe methods.** The standard resilience handler retries every HTTP method by
default, including `POST`. On a write that means duplicates, so `DisableForUnsafeHttpMethods`
is on.

**The exception handler logs.** From .NET 10 the exception handling middleware no longer records
diagnostics when a handler returns `true`, so the handler writes the log entry itself. It also
declines cancelled requests, because a caller who walked away is not a failure.

## What is missing

No authentication, no paging, no soft delete, no multi-user separation. Each of those changes
the shape of the domain, and adding them speculatively is how a small codebase stops being
small.
