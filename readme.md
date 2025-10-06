# Orca

The Orca sample is an example project of what a project might look like using Metaplay, both for simple and more complex use cases. This is a derivative of [Zaibatsu](https://zaibatsu.fi/)'s Secret Shores project - a real F2P game made by a professional game studio. We've since taken it in use as a guided demo which can be found on our [website](https://mtply.co/trydemomenu).

Orca is a story driven merge 2 game, where players explore 7 different islands by merging items and constructing buildings, and meet 4 different heroes. Each island and hero has its own set of missions to earn rewards and level up your heroes.

## Key Takeaways

### Data Driven Design for Live Service Games

The Orca sample makes very heavy use of [Game Configs](https://docs.metaplay.io/feature-cookbooks/game-configs/working-with-game-configs.html) to create a data-first architecture. In fact, almost every part of the game can be changed over-the-air by updating the game config.

Game Configs are one of the reasons why game studios love using Metaplay. By using a data-first architecture, non-programmers can modify and design complex game flows and enable A-B testing of almost every aspect of the game without having to make new builds.

You might have used a variation of remote configs before - but what makes Metaplay's Game Configs unique is that they're fully version-controlled, with diffing and rollback capabilities, and thoughtfully designed to be used by anyone on your game team, from programmers to product managers, to designers and even support staff. You can use Metaplay's Game Configs feature to shape your entire game, as some of our customers indeed do.

For example, Orca's is built on a "trigger" system driven by the game config. Specific player actions and events (e.g. unlocking a new merge item, earning resources, completing dialog) create a trigger, which you can then connect to different types of reactions (e.g. highlighting items, triggering dialog, unlocking heroes/islands). Chaining sets of triggers can create more complex systems. For example, the whole demo tutorial linked above is set up using triggers.

### Metaplay Best Practices

Orca is a good project to use as a reference while working on your own Metaplay integration. We put Metaplay’s best practices into action by using Metaplay's deterministic programming model to keep gameplay cheat-proof, letting the server stay in charge while the client predicts actions for a smooth experience. Content updates happen over-the-air, so you never have to wait for app store releases to see new features or fixes. Plus, in-built analytics and player support tools come as standard to help you understand and support your players every step of the way.

## How to Explore?

We recommend starting by taking a look at the [Architecture Overview](https://docs.metaplay.io/introduction/orca/orca-arch.html), which gives key context on how Orca was built and the main components of the project. Alternatively, you're free to clone the repo and explore the source code, and use the [Architecture Overview](https://docs.metaplay.io/introduction/orca/orca-arch.html) as a reference for when you run into different systems.

## Setup Instructions

1. Clone the repository `git clone git@github.com:metaplay-shared/orca.git`
1. Install the [Metaplay CLI](https://github.com/metaplay/cli)
1. Initialize the MetaplaySDK with `python init-sdk.py`
1. Run the server with `metaplay dev server`
1. Go to [localhost:5550](http://localhost:5550/) to explore the dashboard
1. You can also run the bot client with `metaplay dev botclient -- -MaxBots=10`
1. Explore the bot's Player Model at [localhost:5550/players](http://localhost:5550/players)

## License

The code in the [repository](https://github.com/metaplay-shared/orca) is licensed under [Apache-2.0](./CODE-LICENSE). The other assets (e.g. images, 3d models, etc...) in the project can only be used within the context of this sample, and are **not** available for distribution, modification, and commercial or private use. For commercial inquiries related to the original project, please contact [Zaibatsu Interactive](https://zaibatsu.fi/) as the owners of the project.
