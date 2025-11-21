#!/usr/bin/env bash

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_assets_dir="$(cd "$script_dir/.." && pwd)"

# creates Resources/Models and Resources/Materials inside project Assets
resources_dir="$project_assets_dir/Resources"
mkdir -p "$resources_dir/Models" "$resources_dir/Materials"

# creates the user config Assets folder and write models.json
user_assets_dir="$HOME/.config/unity3d/AtusGames/World creator/Assets"
mkdir -p "$user_assets_dir"

cat > "$user_assets_dir/models.json" <<'JSON'
{
    "assetObjects": [
        {
            "id": 1,
            "name": "Barrel",
            "size": { "x": 1, "y": 1 },
            "modelPath": "Models/<<INSERT MODEL FILE HERE>>",
            "materialPath": "Materials/<<INSERT MATERIAL FILE HERE>>"
        },
        {
            "id": 2,
            "name": "Tree",
            "size": { "x": 1, "y": 1 },
            "modelPath": "Models/<<INSERT MODEL FILE HERE>>",
            "materialPath": "Materials/<<INSERT MATERIAL FILE HERE>>"
        }
    ]
}
JSON

echo "Created:"
echo " - $resources_dir/{Models,Materials}"
echo " - $user_assets_dir/models.json"
echo " Before executing, you must do the following:"
echo " 1) Place your model prefabs in $resources_dir/Models and material prefabs in $resources_dir/Materials. use .fbx or .obj for models and .mat for materials."
echo " 2) Replace <<INSERT MODEL PATH HERE>> and <<INSERT MATERIAL PATH HERE>> in models.json with the correct paths to your model and material prefabs located in the Resources folder."
echo "      TIP: DO NOT add the extension or the Resources/ prefix. For example, if your model is at Assets/Resources/Models/Tree.fbx, use Models/Tree as the modelPath."