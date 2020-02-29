# Hacker News Scraper

## Instructions

Requirements are located in 'Assignment' folder.

## Build

### For executable application

Run command: `dotnet publish .\src\ConsoleApp\ConsoleApp.csproj -c Release`

### For docker image

Run command: `docker build -f .\Dockerfile -t hackernews .`

## Run

### Executable

`.\hackernews --posts 10`
`.\hackernews --help`

### Docker container

After building to image "hackernews"

`docker run -it --rm hackernews --posts 10`

`docker run -it --rm hackernews --help`

## Run mock

You can build and run local mock endpoint for testing.
From test\wiremock.net run `StartMock.ps1` and after using it you can remove containers and images with `Clean.ps1`

## Requirements

.NET SDK 3.1 to build executable or docker tools to build and run in a container.

# Libraries used

System.Text.Json besause it's included and lighter than Newtonsoft.

HtmlAgilityPack because no special optimisations were required and off the shelf component solves given problem.

System.CommandLine (still in beta) as it is an interesting community project.