#!/bin/sh

export ORCA_UNIT_TESTING_CONFIG_BUILD_ENABLED=true
export GOOGLE_SHEET_ID=alternative
dotnet test --filter "FullyQualifiedName~GameLogic.Utils.ConfigBuilder.BuildLocalizations"
