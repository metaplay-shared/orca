#!/bin/sh

if [ -z "$1" ]
then
    echo "Usage: run-single.sh <test case>..."
    echo "Examples:"
    echo "  run-single.sh CanMoveFrom"
    echo "  run-single.sh CanMoveFrom VipPassTest.DailyReward"
    echo "  run-single.sh GameLogic.HeroesModelTest.UnlocksHero"
    exit 0
fi
filter=$(printf "|FullyQualifiedName~%s" $*)
dotnet test --filter "${filter:1}"
