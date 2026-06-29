#!/bin/bash

# Clean downloaded model and material assets from persistent data for a given environment

ENV_NAME=""
USE_WINDOWS=false

while [ $# -gt 0 ]; do
    case "$1" in
        --env)     ENV_NAME="$2"; shift 2 ;;
        --windows) USE_WINDOWS=true; shift ;;
        *)
            echo "Unknown flag: $1"
            echo "Usage: clean_assets.sh --env <dev|prod> [--windows]"
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

echo "Cleaning assets for environment: $ENV_DIR"
for sub in Models Materials; do
    DIR="$BASE/$ENV_DIR/$sub"
    if [ -d "$DIR" ]; then
        rm -rf "$DIR"
        echo "  ✓ Removed: $DIR"
    else
        echo "  - Not found, skipping: $DIR"
    fi
done

echo "Done."
