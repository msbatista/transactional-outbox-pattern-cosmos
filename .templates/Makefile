BASE_DIR := ./
APP_NAME := $(MY_APP)
TAG := latest
BUILD_CONFIGURATION := Release

setup:
	@echo "Preparing environment ..."

	@if [ ! -f .env ]; then \
		cp .env-sample .env ; \
	fi

	@dotnet restore src/WebApi/WebApi.csproj

build:
	@echo "Building project"
	@dotnet restore src/WebApi/WebApi.csproj
	@dotnet build --configuration $(BUILD_CONFIGURATION)

publish:
	@echo "Publishing artificat"
	@dotnet publish src/WebApi/WebApi.csproj \
		--configuration $(BUILD_CONFIGURATION) \
		--output $(BUILD_STAGE_DIR) \
	    --no-restore
	@zip -r output.zip $(BUILD_STAGE_DIR)

docker_build:
	@echo "Building docker image"
	@docker build -t "$(APP_NAME):$(TAG)" .

docker_run:
	@echo "Starting up  docker container"
	@docker run -i -t --rm -p 5000:80 --env-file ./.env --name="$(APP_NAME)" "$(APP_NAME):$(TAG)"

unit_test:
	@echo "Running unit tests ..."
	@dotnet test test/UnitTest/UnitTest.csproj \
	/p:CollectCoverage=true \
	/p:CoverletOutputFormat=opencover \
	--no-restore
