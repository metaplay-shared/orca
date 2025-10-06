#!/usr/bin/env python3

import os
import argparse
import subprocess
import glob

import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

def main():
    parser = argparse.ArgumentParser(description="Initialize Metaplay SDK")
    parser.add_argument("--cli-path", default="metaplay", help="Path to metaplay cli")
    parser.add_argument("--sdk-version", default="34.1", help="SDK version")
    args = parser.parse_args()

    # Make sure metaplay cli is installed, don't output anything
    try:
        subprocess.run([args.cli_path], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except FileNotFoundError:
        logger.error(f"Metaplay CLI not found. Make sure it is installed and in PATH.")
        return

    logger.info(f"Initializing Metaplay SDK with version {args.sdk_version}")

    # Initialize Metaplay SDK
    if (os.path.exists("./MetaplaySDK")):
        logger.info("Metaplay SDK already initialized")
    else:
        subprocess.run([args.cli_path, "init", "sdk", f"--sdk-version={args.sdk_version}", "--sdk-directory=./MetaplaySDK"])

        logger.info("Metaplay SDK initialized successfully")

    logger.info("Applying patches")

    # Apply patches
    for patch in glob.glob("patches/*.patch"):
        subprocess.run(["git", "apply", patch])

if __name__ == "__main__":
    main()