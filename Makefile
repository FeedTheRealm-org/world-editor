CLEAN_ASSETS ?= false
CLEAN_WORLDS ?= false
CLEAN_SESSION ?= false
CLEAN_ALL ?= false
WINDOWS ?= false
ENV ?=

ifeq ($(CLEAN_ALL),true)
CLEAN_ASSETS := true
CLEAN_WORLDS := true
CLEAN_SESSION := true
endif

CLEAN_ASSETS_SCRIPT = Scripts/clean_assets.sh
CLEAN_WORLDS_SCRIPT = Scripts/clean_worlds.sh
CLEAN_SESSION_SCRIPT = Scripts/clean_session.sh

WINDOWS_FLAG = $(if $(filter true,$(WINDOWS)),--windows)
ENV_FLAG = --env $(ENV)

# clean: reset a developer's environment to a clean slate.
#
# Examples:
#   Clean everything in Dev:        make clean CLEAN_ALL=true ENV=dev
#   Clean everything in Prod:       make clean CLEAN_ALL=true ENV=prod
#   Clean only Dev worlds:          make clean CLEAN_WORLDS=true ENV=dev
#   Clean Dev assets + worlds:      make clean CLEAN_ASSETS=true CLEAN_WORLDS=true ENV=dev
#   Clean everything on Windows:    make clean CLEAN_ALL=true ENV=dev WINDOWS=true
clean: ## Reset an environment. Usage: make clean ENV=<dev|prod> [CLEAN_ASSETS=true] [CLEAN_WORLDS=true] [CLEAN_SESSION=true] [CLEAN_ALL=true] [WINDOWS=true]
	@if [ -z "$(strip $(ENV))" ]; then echo "an envrioment is required to run"; exit 1; fi
	$(if $(filter true,$(CLEAN_ASSETS)), bash $(CLEAN_ASSETS_SCRIPT) $(ENV_FLAG) $(WINDOWS_FLAG))
	$(if $(filter true,$(CLEAN_WORLDS)), bash $(CLEAN_WORLDS_SCRIPT) $(ENV_FLAG) $(WINDOWS_FLAG))
	$(if $(filter true,$(CLEAN_SESSION)), bash $(CLEAN_SESSION_SCRIPT) $(WINDOWS_FLAG))
.PHONY: clean
