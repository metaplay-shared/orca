# Orca

Orca is a story-driven merge-2 game, where players explore 7 different islands by merging items and constructing buildings, and meet 4 different heroes. Each island and hero has its own set of missions to earn rewards and level up your heroes. It is a version of the Secret Shores mobile free-to-play game owned by [Zaibatsu Interactive](https://zaibatsu.fi/) and modified by us to serve as a [live demo of Metaplay](https://mtply.co/trydemomenu).

<!-- ![Orca screenshot](./images/orca.png) -->

We loved how the team behind Secret Shores used Metaplay to build a very solid technical foundation for their game, and worked together with Zaibatsu to open-source the code to the wider developer community. It is a great reference of how to structure a live-service game regardless of the genre you might be working on, and we are excited to see what you will build with it.

## Key Takeaways

Orca demonstrates how Metaplay's architecture enables you to build a data-driven, live-service game where designers control progression and content without code changes.

### About Metaplay's Architecture in Orca

- **Deterministic client-server model** - The same `PlayerModel` code runs on both client and server, using fixed-point math and seeded RNG to guarantee identical results. Actions execute optimistically on the client for responsiveness while the server validates and maintains authoritative state, leading to a cheat-proof economy.
- **Type-safe game configs** - Configuration authored in designer-friendly sources (Google Sheets in this case) compiles into shared, type-safe classes that both client and server consume, enabling over-the-air updates without client releases.
- **Event-driven UI updates** - Client and server listeners bridge player model changes to in-game UI, allowing game logic to notify UI code precisely when changes occur.
- **Custom LiveOps Dashboard components** - The stock Metaplay dashboard has been extended with custom components for rich visualization of player state and progression for easier player management.
- **LiveOps events and offers** - Orca uses Metaplay's segmentation, targeting and scheduling features to deliver in-game events and offers to players, allowing designers to work on their content and balancing without client releases.

### Orca's Specific Design Decisions

In addition to just using (and extending) Metaplay's built-in features, Orca implements a few specific design decisions of their own:

- **Trigger system as progression engine** - Orca implements an "if this happens, do that" reaction system entirely in game configs. Triggers fire on events (discoveries, level-ups, unlocks) and execute designer-authored actions (dialogues, UI highlights, rewards), making the entire tutorial and progression flow data-driven.
- **Separation of behavior from content** - Game systems validate behavior (merge rules, economy constraints) while triggers and configs define content (what unlocks when, which dialogue plays). This separation lets designers iterate on progression, tutorials, and story without touching code.
- **Tutorial and dialogue as data** - The entire tutorial system, including the live demo, runs through trigger-driven dialogues authored in spreadsheets. Designers script conversations, chain them to game events, and update the first time user experience pacing over-the-air.

When put all together, Orca is a great real-life example of how to both leverage Metaplay's built-in features and implement your own custom solutions to build a live service game.

## How to Explore?

We recommend starting with the [live demo](https://mtply.co/trydemomenu) of Orca to get a feel for how the game plays and what the Metaplay admin tools look like for this project.

Next, have a look at the [Orca Architecture Overview](https://docs.metaplay.io/introduction/orca/) page in our technical documentation, which gives key context on how Orca was built and the main components of the project.

Because the source code is public, you can also use third-party AI tools like your favorite coding agents or DeepWiki to [quickly deep dive into the technical implementation](https://deepwiki.com/metaplay-shared/orca).

## Usage in your own project

Because the Orca sample's code is released under Apache 2.0 license, you can freely use it as a template for your own project or extract code snippets from it. Please note that the sample assets (e.g. images, 3d models, etc...) are not part of the open-source license and have been included here for demo purposes only.

## Setup Instructions

1. Clone the repository `git clone git@github.com:metaplay-shared/orca.git`
2. Install the [Metaplay CLI](https://github.com/metaplay/cli)
3. Initialize the MetaplaySDK with `python init-sdk.py`
4. Run the server with `metaplay dev server`

The server will start up and you can access the dashboard at [localhost:5550](http://localhost:5550/). You can also run the bot client with `metaplay dev botclient -- -MaxBots=10` and inspect the bot players at [localhost:5550/players](http://localhost:5550/players).

## License

The code in this repository is licensed under [Apache-2.0](https://github.com/metaplay-shared/orca/blob/main/CODE-LICENSE). The other assets (e.g. images, 3d models, etc...) in the project can only be used within the context of this sample, and are **not** available for distribution, modification, and commercial or private use. For commercial inquiries related to the original project, please contact [Zaibatsu Interactive](https://zaibatsu.fi/), as they are the owners of the project.
