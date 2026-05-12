CLEAN_ASSETS ?= true
CLEAN_WORLDS ?= true

CLEAN_ASSETS_SCRIPT = Scripts/clean_assets.sh
CLEAN_WORLDS_SCRIPT = Scripts/clear_worlds.sh

clean: ## Clean downloaded assets and/or world data. Usage: make clean [CLEAN_ASSETS=false] [CLEAN_WORLDS=false]
	$(if $(filter true,$(CLEAN_ASSETS)), bash $(CLEAN_ASSETS_SCRIPT))
	$(if $(filter true,$(CLEAN_WORLDS)), bash $(CLEAN_WORLDS_SCRIPT))
.PHONY: clean
