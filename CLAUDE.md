# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the **Talo Unity Package**, a self-hostable game development SDK providing leaderboards, player authentication, event tracking, game saves, stats, live config, channels, and more. The package is distributed via Unity Asset Store, GitHub releases, and itch.io.

The codebase uses git submodules for dependency management (run `git submodule update --init` after cloning).

## Project Structure

All Talo code is located in `Assets/Talo Game Services/Talo/`:
- `Runtime/` - Core SDK implementation
  - `APIs/` - API client classes for each service (Events, Players, Leaderboards, Saves, Stats, etc.)
  - `Entities/` - Data models (Player, GameSave, LeaderboardEntry, etc.)
  - `Requests/` - Request payload classes
  - `Responses/` - Response payload classes
  - `SocketRequests/` & `SocketResponses/` - WebSocket message types
  - `Utils/` - Internal utilities (ContinuityManager, CryptoManager, SavesManager, etc.)
  - `Vendor/` - Third-party dependencies (WebSocket client)
  - `Talo.cs` - Main static entry point and API access
  - `TaloManager.cs` - MonoBehaviour managing lifecycle and timers
  - `TaloSocket.cs` - WebSocket connection handler
  - `TaloSettings.cs` - ScriptableObject configuration
- `Tests/` - NUnit test suite
- `Samples/` - Demo scenes (Leaderboards, Saves, Authentication, Chat, etc.)

## Architecture Patterns

### Singleton Pattern
`Talo.cs` is the main static singleton providing access to all services (e.g., `Talo.Players`, `Talo.Events`, `Talo.Saves`). It initializes a `TaloManager` MonoBehaviour on first access, which persists across scenes.

### API Architecture
All API classes inherit from `BaseAPI.cs`, which handles HTTP requests via Unity's `UnityWebRequest`. Each API class (e.g., `PlayersAPI`, `SavesAPI`) exposes public async methods returning typed responses.

### Continuity System
The `ContinuityManager` queues failed network requests (POST/PUT/PATCH/DELETE) to disk and replays them when connectivity is restored. It excludes certain endpoints like `/health-check` and `/players/auth`.

### Offline Mode
The SDK supports offline operation via `TaloSettings.offlineMode`. When offline, requests are queued for continuity replay. The `HealthCheckAPI` monitors connectivity and triggers connection lost/restored events.

### WebSocket Integration
`TaloSocket.cs` manages WebSocket connections for real-time features (channels, presence). It uses the `com.mikeschweitzer.websocket` package (vendored in `Runtime/Vendor/`).

### Event Flushing
Events are batched and flushed on application quit/pause/focus loss. On WebGL, events flush every `webGLEventFlushRate` seconds (default 30s) due to platform limitations.

### Debouncing
Player updates and save updates are debounced to prevent excessive API calls during rapid property changes. APIs that need debouncing inherit from `DebouncedAPI<TOperation>` (a generic base class) and define a `DebouncedOperation` enum for type-safe operation keys. The base class uses a dictionary to track multiple debounced operations independently.

To add debouncing to an API:
1. Define a public `enum DebouncedOperation` with your debounced operations
2. Inherit from `DebouncedAPI<YourAPI.DebouncedOperation>`
3. Call `Debounce(DebouncedOperation.YourOperation)` to queue an operation
4. Implement `ExecuteDebouncedOperation(DebouncedOperation operation)` with a switch statement
5. The base class's `ProcessPendingUpdates()` is called by `TaloManager.Update()` every frame

Example: `PlayersAPI` defines `enum DebouncedOperation { Update }` and inherits from `DebouncedAPI<PlayersAPI.DebouncedOperation>`. When `Player.SetProp()` is called, it calls `Debounce(DebouncedOperation.Update)`, which queues the update to be executed after `debounceTimerSeconds` (default: 1s). Multiple property changes within the debounce window result in a single API call.

## Common Development Commands

### Running Tests
Tests use Unity Test Framework with NUnit. Run via Unity Editor:
- **Test Runner Window**: Window > General > Test Runner
- **Run All Tests**: Select "PlayMode" or "EditMode" tab and click "Run All"

CI runs tests via GitHub Actions (`.github/workflows/ci.yml`) using Unity 6000.0.59f2.

### Building the Package
The package is built via GitHub Actions (`.github/workflows/create-release.yml`) on tagged releases:
- Creates `talo.unitypackage` from `Assets/Talo Game Services/**/*`
- Uploads to itch.io and GitHub releases

### Version Management
Version is stored in `Assets/Talo Game Services/Talo/VERSION`. A pre-commit hook (`.git/hooks/pre-commit`) automatically updates `ClientVersion` in `BaseAPI.cs` to match the VERSION file.

## Key Configuration

### TaloSettings
Settings are managed via a ScriptableObject asset (`Resources/Talo Settings`):
- `accessKey` - API authentication key
- `apiUrl` - Backend URL (default: `https://api.trytalo.com`)
- `socketUrl` - WebSocket URL (default: `wss://api.trytalo.com`)
- `autoStartSession` - Auto-authenticate with saved session token
- `autoConnectSocket` - Auto-connect WebSocket on startup
- `continuityEnabled` - Enable failed request replay
- `offlineMode` - Simulate offline for testing
- `debounceTimerSeconds` - Debounce interval for player updates, saves, and health checks (default: 1s)

### Assembly Definitions
- `Talo.Runtime` (TaloRuntime.asmdef) - Main runtime assembly
- `Talo.Tests` (TaloTests.asmdef) - Test assembly with NUnit references

## Important Implementation Details

### Identity & Sessions
Players must be identified via `Talo.Players.Identify()` before using most APIs. The SDK stores the current alias in `Talo.CurrentAlias` and player in `Talo.CurrentPlayer`. Sessions are managed by `SessionManager` with tokens stored in PlayerPrefs.

### Test Mode Detection
The SDK detects when running under NUnit (checks for `nunit.framework` assembly) and enables test mode, which uses `RequestMock` instead of real HTTP calls.

### Error Handling
- `RequestException` - HTTP errors
- `PlayerAuthException` - Authentication-specific errors with error codes
- `SocketException` - WebSocket errors
- `ContinuityReplayException` - Continuity replay failures

### Saves System
Game saves support local caching and offline operation via `SavesManager`. The manager handles loading, creating, updating, and deleting saves with automatic continuity support.

## Git Workflow

- Main branch: `develop`
- Submodules: Required for WebSocket dependency
- Pre-commit hook: Auto-updates version in BaseAPI.cs
- CI: Runs tests on every push
- Releases: Manual workflow dispatch creates Unity package and GitHub release
