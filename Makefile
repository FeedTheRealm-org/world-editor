CLEAN_ASSETS ?= true
CLEAN_WORLDS ?= true
CLEAN_SESSION ?= true
WINDOWS ?= false

CLEAN_ASSETS_SCRIPT = Scripts/clean_assets.sh
CLEAN_WORLDS_SCRIPT = Scripts/clear_worlds.sh
CLEAN_SESSION_SCRIPT = Scripts/clean_session.sh

WINDOWS_FLAG = $(if $(filter true,$(WINDOWS)),--windows)

clean: ## Clean downloaded assets, world data, and/or session. Usage: make clean [CLEAN_ASSETS=false] [CLEAN_WORLDS=false] [CLEAN_SESSION=false] [WINDOWS=true]
	$(if $(filter true,$(CLEAN_ASSETS)), bash $(CLEAN_ASSETS_SCRIPT) $(WINDOWS_FLAG))
	$(if $(filter true,$(CLEAN_WORLDS)), bash $(CLEAN_WORLDS_SCRIPT) $(WINDOWS_FLAG))
	$(if $(filter true,$(CLEAN_SESSION)), bash $(CLEAN_SESSION_SCRIPT) $(WINDOWS_FLAG))
.PHONY: clean
