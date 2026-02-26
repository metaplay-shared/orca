# SDKs in the project
 - Facebook Unity SDK 11.0.0

## Working with the MetaplaySDK dashboard
First of all see the documentation provided by Metaplay
* [Implementing MetaRewards](https://www.notion.so/Implementing-MetaRewards-7f9a5a93e98646099e5ce520ed34c482)
* [Getting Started with Customizing the LiveOps Dashboard](https://www.notion.so/Getting-Started-with-Customizing-the-LiveOps-Dashboard-5b39f8c32f754fe8993921c8ffd54e9e)
* [Customizing the LiveOps Dashboard Frontend](https://www.notion.so/Customizing-the-LiveOps-Dashboard-Frontend-6cf210b0aecf425880c213ceccbcb837)

### Installing the required tools (OSX)
```sh
# Install nvm (Node Version Manager)
brew install nvm
```
Add `nvm` to your path. For example, if using bash append the following lines to your `~/.bashrc`
```sh
export NVM_DIR=~/.nvm
. $(brew --prefix nvm)/nvm.sh
```
Then set up npm. At the time of writing (4th July 2022):
```
nvm install 16
nvm use v16.15.1
```
### Building and running
Then in `<Project Orca>/MetaplaySDK/Backend/Dashboard`
```
# Install the project
npm ci
# To run the dashboard locally during development run
npm run serve
# ...and then browse to localhost:5551. Note that you need
# to have the server running (see "Running from Rider" below).

# To build the dashboard (to be able to access it by simply running the server from within Rider)
npm run build
```

### Running Metaplay server from Rider
The results of `npm run build` can be committed to the repository to allow
other developers to get the changes made to the dashboard.
That is, one can run the Metaplay server from Rider
* Open Orca-Server solution in Rider
* Run the server by clicking "Run" in the upper right corner of the IDE
* Access the dashboard in `localhost:5550` (**notice the port!**).

To get the client to connect to the locally running server when run from Unity:
* In Unity, open `Assets/Scenes/Start`
* Right-click `Application Manager` and selet `Properties`
* Change `Active Environment` to `Localhost`

## Building game configs
The game configs can be built (locally) by opening the Unity project and
clicking menu items under `Config Builder` menu.

The configs can also be built from the command line:
```sh
cd Project-Orca/Backend/CloudCore.Tests
# Build primary config OR...
./build-primary-config.sh
# ...build unit testing config
./build-unit-test-config.sh
```

There's also a tool for comparing game configs:
```sh
# Print the usage instructions
./diff-configs.sh -h
...
# Compare primary and alternative configs
./diff-configs.sh primary alternative conf-diff.tmp
```

## Unit testing
Unit tests for the game logic are placed in `Backend/CloudCore.Tests`
(see especially `GameLogic` directory).
When starting to write unit tests
* see `Backend/CloudCore.Tests/GameLogic/ExampleTest.cs` for a template that you
  can copy-paste to get started quickly
* browse through `Backend/CloudCore.Tests/GameLogic/Utils/TestModel.cs` to get
  familiar with frequently needed utility methods which help one to write
  more compact test cases

The tests can be run in two ways:
either from (Rider) IDE or from command line.

### Running from Rider
First, in Rider open the `Orca-Server` project i.e. `<Orca repo>/Backend/Orca-Server.sln`

Now running the tests is straightforward. Right-click a test file
(in the file explorer), test class or test method (in the editor) and
select `Run Unit Tests` (or `Debug Unit Tests`).

In the "Unit Tests" view you might want to group the tests by "Project
Structure" (click the icon with four small squares) to separate Orca
tests from Metaplay tests.

### Running from command line
The tests are run with `<ORCA ROOT DIR>/Backend/CloudCore.Tests` as the working directory.
To quickly run all test cases use
```sh
./run-all.sh
```
and to run one or more explicitly specified test cases use
```sh
./run-single.sh CanMoveFrom VipPassTest.DailyReward
```
For more advanced (and rarely used) way of selecting which tests to run, keep reading.

Under the hood `dotnet` command is used to run the tests. It is assumed
that `dotnet` is found in `$PATH`. For example, Unity installation
seems to place it under `~/.dotnet`.

The simplest way to run only a set of tests
is probably using `~` aka "contains" operator in the `--filter` option.
To run all Orca tests:
```sh
dotnet test --filter "FullyQualifiedName~CloudCore.Tests.GameLogic"
```
To run only the tests in a single class (e.g. `MergeBoardTest`):
```sh
dotnet test --filter "FullyQualifiedName~CloudCore.Tests.GameLogic.MergeBoardTest"
# ...or more simply
dotnet test --filter "FullyQualifiedName~MergeBoardTest"
# ...or even more simply (filter value without an operator is taken as
# # a contains on FullyQualifiedName property).
dotnet test --filter MergeBoardTest
```
To run only a single test method (`ItemSpawner`):
```sh
dotnet test --filter "FullyQualifiedName~CloudCore.Tests.GameLogic.MergeBoardTest.ItemSpawner"
# ...or simply
dotnet test --filter "ItemSpawner"
```
To run two test classes (using `|` aka "OR" operator):
```sh
dotnet test --filter "FullyQualifiedName~MergeBoardTest|FullyQualifiedName~ItemDiscoveryTest"
```

See [dotnet test documentation](https://docs.microsoft.com/en-gb/dotnet/core/tools/dotnet-test)
for more information about filtering and other options.
