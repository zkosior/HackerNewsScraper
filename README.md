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

## Requirements

.NET SDK 3.1 to build executable or docker tools to build and run in container.

# Libraties used

System.Text.Json besause is included and lighter than Newtonsoft

HtmlAgilityPack because no special optimisations were required and off the shelf component solves given problem

System.CommandLine (still in beta) as it is an interesting community project