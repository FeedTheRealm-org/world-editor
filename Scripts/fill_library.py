import os
import sys
import json
import uuid
import shutil
import glob
from pathlib import Path


def validate_inputs(models_folder, material_file):
    """Validate that the input paths exist."""
    if not os.path.isdir(models_folder):
        print(f"Error: Models folder '{models_folder}' does not exist")
        sys.exit(1)

    if not os.path.isfile(material_file):
        print(f"Error: Material file '{material_file}' does not exist")
        sys.exit(1)


def setup_paths():
    """Setup all the necessary directory paths."""
    script_dir = Path(__file__).parent.absolute()
    project_assets_dir = script_dir.parent
    resources_dir = project_assets_dir / "Assets" / "Resources"
    user_assets_dir = (
        Path.home() / ".config" / "unity3d" / "AtusGames" / "World creator" / "Assets"
    )

    return {
        "resources_dir": resources_dir,
        "models_dir": resources_dir / "Models",
        "materials_dir": resources_dir / "Materials",
        "user_assets_dir": user_assets_dir,
        "json_file": user_assets_dir / "models.json",
    }


def create_directories(paths):
    """Create necessary directories if they don't exist."""
    for dir_path in [
        paths["models_dir"],
        paths["materials_dir"],
        paths["user_assets_dir"],
    ]:
        dir_path.mkdir(parents=True, exist_ok=True)


def copy_material(material_file, materials_dir):
    """Copy the material file to the Materials directory."""
    material_filename = Path(material_file).name  # Get full filename with extension

    destination = materials_dir / material_filename
    print("Copying material file...")
    shutil.copy2(material_file, destination)

    return material_filename


def copy_models(models_folder, models_dir):
    """Copy all model files to the Models directory."""
    model_extensions = ["*.fbx", "*.obj", "*.dae", "*.blend"]
    copied_files = []

    print("Copying model files...")

    for extension in model_extensions:
        pattern = os.path.join(models_folder, extension)
        for model_file in glob.glob(pattern):
            filename = os.path.basename(model_file)  # Get full filename with extension
            destination = models_dir / filename
            shutil.copy2(model_file, destination)
            copied_files.append(filename)  # Store full filename

    return copied_files


def load_existing_json(json_file):
    """Load existing JSON file or create base structure."""
    if json_file.exists():
        try:
            with open(json_file, "r") as f:
                data = json.load(f)
            print("Appending to existing models.json...")
            return data
        except (json.JSONDecodeError, FileNotFoundError):
            print("Error reading existing JSON, creating new one...")
    else:
        print("Creating new models.json...")

    return {"assetObjects": []}


def get_existing_model_names(data):
    """Get set of existing model names to avoid duplicates."""
    return {obj.get("name") for obj in data.get("assetObjects", [])}


def add_new_models(data, copied_models, material_filename, existing_names):
    """Add new model entries to the JSON data."""
    added_count = 0

    for model_filename in copied_models:
        # Extract name without extension for duplicate checking
        model_name = Path(model_filename).stem

        if model_name not in existing_names:
            # Generate a random UUID for the ID
            unique_id = str(uuid.uuid4())

            new_entry = {
                "id": unique_id,
                "name": model_name,
                "size": {"x": 1, "y": 1},
                "modelPath": f"Models/{model_filename}",  # Include full filename with extension
                "materialPath": f"Materials/{material_filename}",  # Include full filename with extension
            }

            data["assetObjects"].append(new_entry)
            added_count += 1
            print(f"Added: {model_filename} (ID: {unique_id[:8]}...)")

    return added_count


def save_json(data, json_file):
    """Save the updated JSON data to file."""
    with open(json_file, "w") as f:
        json.dump(data, f, indent=2)


def list_copied_models(models_dir):
    """List all model files that were copied."""
    model_extensions = ["*.fbx", "*.obj", "*.dae", "*.blend"]
    copied_files = []

    for extension in model_extensions:
        pattern = str(models_dir / extension)
        copied_files.extend([Path(f).name for f in glob.glob(pattern)])

    return copied_files


def main():
    # Check command line arguments
    if len(sys.argv) != 3:
        print(
            "Usage: python3 fill_library.py <models_folder_path> <material_file_path>"
        )
        print(
            "Example: python3 fill_library.py /path/to/models/folder /path/to/material.mat"
        )
        sys.exit(1)

    models_folder = sys.argv[1]
    material_file = sys.argv[2]

    # Validate inputs
    validate_inputs(models_folder, material_file)

    # Setup paths
    paths = setup_paths()

    # Create necessary directories
    create_directories(paths)

    # Copy files
    material_filename = copy_material(material_file, paths["materials_dir"])
    copied_models = copy_models(models_folder, paths["models_dir"])

    # Load existing JSON data
    data = load_existing_json(paths["json_file"])

    # Get existing model names to avoid duplicates
    existing_names = get_existing_model_names(data)

    # Add new models to JSON
    added_count = add_new_models(data, copied_models, material_filename, existing_names)

    # Save updated JSON
    save_json(data, paths["json_file"])

    # Print summary
    print("")
    print("Library filled successfully!")
    print(f"Copied models to: {paths['models_dir']}")
    print(f"Copied material to: {paths['materials_dir']}/{material_filename}")
    print(f"Updated: {paths['json_file']}")
    print("")
    print(f"Added {added_count} new assets")
    print(f"Total assets in library: {len(data['assetObjects'])}")
    print("")
    print("Models copied:")

    copied_files = list_copied_models(paths["models_dir"])
    for file in sorted(copied_files):
        print(f"  - {file}")


if __name__ == "__main__":
    main()
