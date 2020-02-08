# Hacker News Scraper

## Instructions

Requirements are located in 'Assignment' folder.

## Build

### For executable application

Run command: dotnet publish .\src\ConsoleApp\ConsoleApp.csproj -c Release

### For docker image

Run command: docker build -f .\DockerFile -t hackernews .

## Run

### Executable

hackernews --posts 10
hackernews --help

### Docker container

After builting to image "hackernews"

docker run -it --rm hackernews --posts 10
docker run -it --rm hackernews --help
