#!/bin/bash

# Clean downloaded model assets from StreamingAssets and persistent data

STREAMING_ASSETS_MODELS="$(dirname "$0")/../Assets/StreamingAssets/Models"
LINUX_PERSISTENT_DATA_MODELS="$HOME/.config/unity3d/AtusGames/World creator/Models"
WINDOWS_PERSISTENT_DATA_MODELS="/mnt/c/Users/$USER/AppData/LocalLow/AtusGames/World creator/Models"

USE_WINDOWS=false

for arg in "$@"; do
    case $arg in
        --windows)
            USE_WINDOWS=true
            ;;
        *)
            echo "Unknown flag: $arg"
            echo "Usage: clean_assets.sh [--windows]"
            exit 1
            ;;
    esac
done

echo "Cleaning StreamingAssets models..."
if [ -d "$STREAMING_ASSETS_MODELS" ]; then
    rm -rf "$STREAMING_ASSETS_MODELS"
    echo "  ✓ Removed: $STREAMING_ASSETS_MODELS"
else
    echo "  - Not found, skipping: $STREAMING_ASSETS_MODELS"
fi

if [ "$USE_WINDOWS" = true ]; then
    echo "Cleaning Windows persistent data models... (you really code on Windows?)"
    if [ -d "$WINDOWS_PERSISTENT_DATA_MODELS" ]; then
        rm -rf "$WINDOWS_PERSISTENT_DATA_MODELS"
        echo "  ✓ Removed: $WINDOWS_PERSISTENT_DATA_MODELS"
    else
        echo "  - Not found, skipping: $WINDOWS_PERSISTENT_DATA_MODELS"
    fi
else
    echo "Cleaning Linux persistent data models..."
    if [ -d "$LINUX_PERSISTENT_DATA_MODELS" ]; then
        rm -rf "$LINUX_PERSISTENT_DATA_MODELS"
        echo "  ✓ Removed: $LINUX_PERSISTENT_DATA_MODELS"
    else
        echo "  - Not found, skipping: $LINUX_PERSISTENT_DATA_MODELS"
    fi
fi

echo "Done."
