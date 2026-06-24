#!/bin/bash

# Clean downloaded world assets from persistent data for a given environment

ENV_NAME=""
USE_WINDOWS=false

while [ $# -gt 0 ]; do
    case "$1" in
        --env)     ENV_NAME="$2"; shift 2 ;;
        --windows) USE_WINDOWS=true; shift ;;
        *)
            echo "Unknown flag: $1"
            echo "Usage: clean_worlds.sh --env <dev|prod> [--windows]"
            exit 1
            ;;
    esac
done

case "$ENV_NAME" in
    dev)  ENV_DIR="Dev" ;;
    prod) ENV_DIR="Prod" ;;
    "")   echo "an envrioment is required to run"; exit 1 ;;
    *)    echo "Invalid environment: $ENV_NAME (expected dev or prod)"; exit 1 ;;
esac

if [ "$USE_WINDOWS" = true ]; then
    BASE="/mnt/c/Users/$USER/AppData/LocalLow/AtusGames/World creator"
else
    BASE="$HOME/.config/unity3d/AtusGames/World creator"
fi

WORLDS_DIR="$BASE/$ENV_DIR/Worlds"

echo "Cleaning worlds for environment: $ENV_DIR"
if [ -d "$WORLDS_DIR" ]; then
    rm -rf "$WORLDS_DIR"
    echo "  ✓ Removed: $WORLDS_DIR"
else
    echo "  - Not found, skipping: $WORLDS_DIR"
fi

echo "Done."
