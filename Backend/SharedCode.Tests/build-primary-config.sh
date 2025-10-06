#!/bin/sh

export ORCA_UNIT_TESTING_CONFIG_BUILD_ENABLED=true
dotnet test --filter "FullyQualifiedName~GameLogic.Utils.ConfigBuilder.BuildPrimaryGameConfig"
