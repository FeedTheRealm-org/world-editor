#!/bin/bash

# Clean session data from persistent data

LINUX_PERSISTENT_DATA_SESSION="$HOME/.config/unity3d/AtusGames/World creator/session.json"
WINDOWS_PERSISTENT_DATA_SESSION="/mnt/c/Users/$USER/AppData/LocalLow/AtusGames/World creator/session.json"

USE_WINDOWS=false

for arg in "$@"; do
    case $arg in
        --windows)
            USE_WINDOWS=true
            ;;
        *)
            echo "Unknown flag: $arg"
            echo "Usage: clean_session.sh [--windows]"
            exit 1
            ;;
    esac
done

if [ "$USE_WINDOWS" = true ]; then
    echo "Cleaning Windows session data... (you really code on Windows?)"
    if [ -f "$WINDOWS_PERSISTENT_DATA_SESSION" ]; then
        rm -f "$WINDOWS_PERSISTENT_DATA_SESSION"
        echo "  ✓ Removed: $WINDOWS_PERSISTENT_DATA_SESSION"
    else
        echo "  - Not found, skipping: $WINDOWS_PERSISTENT_DATA_SESSION"
    fi
else
    echo "Cleaning Linux session data..."
    if [ -f "$LINUX_PERSISTENT_DATA_SESSION" ]; then
        rm -f "$LINUX_PERSISTENT_DATA_SESSION"
        echo "  ✓ Removed: $LINUX_PERSISTENT_DATA_SESSION"
    else
        echo "  - Not found, skipping: $LINUX_PERSISTENT_DATA_SESSION"
    fi
fi

echo "Done."
