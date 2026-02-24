# Contributing

## Setup

1. Clone the repository with submodules:
   ```bash
   git clone --recurse-submodules <repo-url>
   # or after cloning:
   git submodule update --init
   ```

2. Open the project in **Unity 6000.0.59f2** (or later).

## Project structure

```
Assets/Talo Game Services/Talo/
├── Runtime/
│   ├── APIs/              # One file per API (extend BaseAPI or DebouncedAPI)
│   ├── Entities/          # Data models (Player, GameSave, LeaderboardEntry, etc.)
│   ├── Requests/          # Request payload classes
│   ├── Responses/         # Response payload classes
│   ├── SocketRequests/    # WebSocket message types (outbound)
│   ├── SocketResponses/   # WebSocket message types (inbound)
│   ├── Utils/             # Internal managers and helpers
│   ├── Vendor/            # Third-party dependencies (WebSocket client)
│   ├── Talo.cs            # Main static entry point
│   ├── TaloManager.cs     # MonoBehaviour lifecycle manager
│   ├── TaloSocket.cs      # WebSocket connection handler
│   └── TaloSettings.cs    # Add new settings here
├── Tests/                 # NUnit test suite
├── Samples/               # Demo scenes (Leaderboards, Saves, Auth, Chat, etc.)
```

## Code style

Follow the patterns established in existing API and entity files:

- All API classes inherit from `BaseAPI` (or `DebouncedAPI<TOperation>` when debouncing is needed)
- Use `async`/`await` for all network operations
- Request and response types live in their respective `Requests/` and `Responses/` folders

## Testing your changes

Tests use the **Unity Test Framework** (NUnit). Run them inside the editor via **Window > General > Test Runner**.

Consider adding tests where relevant.

## Submitting a PR

- Keep PRs focused — one feature or fix per PR
- Target the `develop` branch
- Use the Playground sample scene (`Samples/Playground`) to test your changes interactively.
