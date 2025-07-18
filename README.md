# Talo Unity package: self-hostable game dev tools

Talo is the easiest way to add leaderboards, player authentication, socket-based multiplayer and more to your game. Using the Talo Dashboard, you can visualise and analyse your game data to make data-driven decisions.

## Project structure

You can find the code, samples and tests in the `Assets/Talo Game Services/Talo` folder.

## Submodules

This project uses git submodules to manage dependencies (Unity's dependency management is a [nightmare](https://trytalo.com/blog/building-unity-packages)).

You'll need to init the submodules after cloning: `git submodule update --init`.

## Get the package

- [Unity Asset Store](https://assetstore.unity.com/packages/slug/292832)
- [GitHub releases](https://github.com/TaloDev/unity/releases)
- [itch.io](https://sleepystudios.itch.io/talo-unity)

## Features

- 👥 [Player management](https://trytalo.com/players): Persist player data across sessions, create segments and handle authentication.
- ⚡️ [Event tracking](https://trytalo.com/events): Track in-game player actions individually and globally.
- 🕹️ [Leaderboards](https://trytalo.com/leaderboards): Highly customisable leaderboards that can sync with Steamworks.
- 💾 [Game saves](https://trytalo.com/saves): A simple and flexible way to load/save game state; also works offline.
- 📊 [Game stats](https://trytalo.com/stats): Track global or per-player stats across your game; also syncs with Steamworks.
- 💬 [Game channels](https://trytalo.com/channels): Send real-time messages between players subscribed to specific topics.
- ⚙️ [Live config](https://trytalo.com/live-config): Update game settings from the web with zero downtime.
- 🔧 [Steamworks integration](https://trytalo.com/steamworks-integration): Hook into Steamworks for authentication and ownership checks.
- 🗣️ [Game feedback](https://trytalo.com/feedback): Collect and manage feedback from your players.
- 🛡️ [Continuity](https://trytalo.com/continuity): Keep your data in-sync even when your players are offline.
- 🔔 [Player presence](https://trytalo.com/players#presence): See if players are online and set custom statuses.

## Samples included with the package

- 🕹️ **Leaderboards**: a basic leaderboard UI, allowing you to add and update entries.
- 💾 **Game saves**: an end to end example allowing you to load, create and update saves across multiple levels.
- 🔒 **Authentication**: a registration/login flow, showing how to create player accounts and authenticate them.
- 🎮 **Playground**: a text-based playground allowing you to test identifying, events, stats and leaderboards.
- 💬 **Chat**: showing how to send messages between players in a chat room using channels.
- 🤝 **Channel storage**: showing how to store data that can be accessed by other players using channels.

## Links

- [Website](https://trytalo.com)
- [Unity package docs](https://docs.trytalo.com/docs/unity/install)
- [Self-hosting examples](https://github.com/talodev/hosting)

## Contributing

Thinking about contributing to Talo? We’d love the help! Head over to our [contribution guide](CONTRIBUTING.md) to learn how to set up the project, run tests, and start adding new features.

## Join the community

Have questions, want to share feedback or show off your game? [Join us on Discord](https://trytalo.com/discord) to connect with other developers and get help from the Talo team.
