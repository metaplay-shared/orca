#!/usr/bin/env python3
"""Fetch the Metaplay SDK this project was built against and apply its patch.

This repository ships the project's modifications to the Metaplay SDK as a
patch instead of the SDK itself. Run this once after cloning:

    python init-sdk.py

It reads the exact SDK version from the 'sdk-version' file, downloads that SDK
with the Metaplay CLI (https://github.com/metaplay/cli), and applies every
patch in 'patches/'. The version is pinned exactly on purpose: a patch only
fits the SDK release it was generated from.
"""

import argparse
import glob
import logging
import os
import subprocess
import sys

logging.basicConfig(
    level=os.getenv("LOG_LEVEL", "INFO"), format="%(levelname)s: %(message)s"
)
logger = logging.getLogger(__name__)

SDK_DIR = "./MetaplaySDK"
VERSION_FILE = "sdk-version"


def read_sdk_version(path: str) -> str:
    """Read the pinned SDK version, e.g. '37.1.0'."""
    try:
        with open(path) as handle:
            version = handle.read().strip()
    except FileNotFoundError:
        logger.error("%s not found; run this from the repository root", path)
        sys.exit(1)
    if not version:
        logger.error("%s is empty", path)
        sys.exit(1)
    return version


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Initialize the Metaplay SDK for this project"
    )
    parser.add_argument(
        "--cli-path", default="metaplay", help="Path to the Metaplay CLI"
    )
    parser.add_argument(
        "--sdk-version",
        default=None,
        help=f"Override the version pinned in {VERSION_FILE}",
    )
    args = parser.parse_args()

    try:
        subprocess.run(
            [args.cli_path, "--version"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            check=False,
        )
    except FileNotFoundError:
        logger.error(
            "Metaplay CLI not found. Install it from https://github.com/metaplay/cli "
            "and make sure it is on your PATH."
        )
        sys.exit(1)

    version = args.sdk_version or read_sdk_version(VERSION_FILE)

    if os.path.exists(SDK_DIR):
        logger.info("%s already exists, skipping download", SDK_DIR)
    else:
        logger.info("Downloading Metaplay SDK %s", version)
        result = subprocess.run(
            [
                args.cli_path,
                "init",
                "sdk",
                f"--sdk-version={version}",
                f"--sdk-directory={SDK_DIR}",
            ],
            check=False,
        )
        if result.returncode != 0:
            logger.error("Failed to download the Metaplay SDK")
            sys.exit(1)

    patches = sorted(glob.glob("patches/*.patch"))
    if not patches:
        logger.error("No patches found in patches/; this project needs them to build")
        sys.exit(1)

    for patch in patches:
        logger.info("Applying %s", patch)
        result = subprocess.run(
            ["git", "apply", "--whitespace=nowarn", patch], check=False
        )
        if result.returncode != 0:
            logger.error(
                "Failed to apply %s. The patch is generated against SDK %s exactly; "
                "check that %s holds that version and that MetaplaySDK/ is unmodified.",
                patch,
                version,
                SDK_DIR,
            )
            sys.exit(1)

    logger.info("Metaplay SDK %s is ready. Next: metaplay dev server", version)


if __name__ == "__main__":
    main()
