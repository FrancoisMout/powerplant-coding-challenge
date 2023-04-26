Welcome to my solution of the production plan challenge proposed by the SPaaS team within GEM.

## In Visual Studio

The REST API is served on https://localhost:8888 and a swagger is available at https://localhost:8888/swagger/index.html when the application is launched in the development environnement.

## In Command Line Window

Go to the subfolder `\PowerPlantAPI` and execute `dotnet run`.

## Implementation details

1. I use a knapsack algorithm to find a combination of wind turbines matching the load. Since they don't cost anything they are the first I work with.

2. Then I increase the quantity of the other turbines ordered by their cost per MWh. By doing so I try to keep as much wind power as I can.

3. Once a combination of turbines (wind and/or non-wind) is found, which means that the required load can be met by this set of turbines, a rebalance of the powers is executed to give more load to cheaper turbines.

4. Since this algorithm is not perfect and can fail I have included a simple example that my algorithm cannot solve in the file `\example_payloads\payload4.json`.

`Note:` I have used VS2022 with .Net 7.0 for the ASP.NET core WEB API project.

