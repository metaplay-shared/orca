#!/bin/sh

print_usage() {
    cat <<EOF
Usage: ./diff-localizations.sh <config-1> <config-2> [<output dir>]

Examples:
> ./diff-configs.sh primary alternative
> ./diff-configs.sh primary ABKq5zdgds32rffBWf diff-out

An existing output directory needs to be empty; nonexistent is created.
As a parameter one can use
* Google Sheet ID
* keyword "primary"
* keyword "alternative"
* keyword "test" (unit testing config)
EOF
}

if [ "$1" = "-h" ] || [ "$1" = "--help" ]
then
    print_usage
    exit 0
fi

if [ $# -lt 2 ] || [ $# -gt 3 ]
then
    print_usage
    exit 1
fi

config1="$1"
config2="$2"
output_dir="${3:-localizations-diff.tmp}"

if [ -d "${output_dir}" ]
then
    if [ -z "$(ls "${output_dir}")" ]
    then
        echo "Using existing directory for output: ${output_dir}"
    else
        echo "ERROR: output directory not empty: ${output_dir}"
        exit 1
    fi
else
    echo "Creating output dir: ${output_dir}"
    mkdir "${output_dir}" || exit 1
fi

export ORCA_UNIT_TESTING_CONFIG_BUILD_ENABLED=true
for config in "${config1}" "${config2}"
do
    export GOOGLE_SHEET_ID="${config}"
    export GAME_CONFIG_OUTPUT_DIR="${output_dir}"
    dotnet test --filter "FullyQualifiedName~GameLogic.Utils.ConfigBuilder.PrintLocalizations" || exit 1
done

diff_file="${output_dir}"/diff.txt
git diff --no-index "${output_dir}/${config1}" "${output_dir}/${config2}" | tee "${diff_file}"

echo "The diff can be found in ${diff_file}"
