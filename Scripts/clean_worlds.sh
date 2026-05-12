#!/bin/bash

# Clean downloaded world assets from persistent data

LINUX_PERSISTENT_DATA_WORLDS="$HOME/.config/unity3d/AtusGames/World creator/Worlds"
WINDOWS_PERSISTENT_DATA_WORLDS="/mnt/c/Users/$USER/AppData/LocalLow/AtusGames/World creator/Worlds"

USE_WINDOWS=false

for arg in "$@"; do
    case $arg in
        --windows)
            USE_WINDOWS=true
            ;;
        *)
            echo "Unknown flag: $arg"
            echo "Usage: clear_worlds.sh [--windows]"
            exit 1
            ;;
    esac
done

if [ "$USE_WINDOWS" = true ]; then
    echo "Cleaning Windows persistent data worlds... (you really code on Windows?)"
    if [ -d "$WINDOWS_PERSISTENT_DATA_WORLDS" ]; then
        rm -rf "$WINDOWS_PERSISTENT_DATA_WORLDS"
        echo "  ✓ Removed: $WINDOWS_PERSISTENT_DATA_WORLDS"
    else
        echo "  - Not found, skipping: $WINDOWS_PERSISTENT_DATA_WORLDS"
    fi
else
    echo "Cleaning Linux persistent data worlds..."
    if [ -d "$LINUX_PERSISTENT_DATA_WORLDS" ]; then
        rm -rf "$LINUX_PERSISTENT_DATA_WORLDS"
        echo "  ✓ Removed: $LINUX_PERSISTENT_DATA_WORLDS"
    else
        echo "  - Not found, skipping: $LINUX_PERSISTENT_DATA_WORLDS"
    fi
fi

echo "Done."
