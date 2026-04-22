<p align="center">
    <img src="JetRestaurantLookup\Assets\app-icon.png" alt="App Icon" height="120">
</p>

# JET Restaurant Lookup
Search for restaurants near any UK Postcode using the Just Eat API!

A coding assignment for the Early Careers Software Engineering Program at Just Eat Takeaway.

## Features
- View the name, address & rating of 10 restaurants near any uk postcode.
- Filter by dietary options (e.g., "Vegan"), special offers (e.g., "Freebies"), and cuisine types (e.g., "Italian").
- Open restaurants on Google Maps or [just-eat.co.uk](https://www.just-eat.co.uk)

## UI
The app's theme automatically matches the user's system theme.
### Dark Theme
![Dark Theme UI](Assets/dark-ui-example.png)
### Light Theme
![Light Theme UI](Assets/light-ui-example.png)

## Design & Architecture
The solution is split into three projects to keep concerns separated:

- **JetRestaurantLookup** – UI layer (Avalonia views and view models)
- **JetRestaurantLookup.Core** – core logic (services, models, DTOs, utilities)
- **JetRestaurantLookup.Tests** – unit tests

The application follows the MVVM pattern, with view models handling state and user interactions while keeping the UI layer lightweight.

External API interaction is encapsulated behind `IRestaurantService`, isolating HTTP and API-specific concerns from the rest of the application. The service depends on an injected `HttpClient`, which allows it to be easily tested using mocked HTTP responses without making real network calls.

A mapping layer is used to convert API DTOs into domain models. While the models are currently very similar to the DTOs, this separation was intentional to avoid coupling the rest of the application to the API response structure and to make future changes easier to accommodate.

Some presentation-specific logic (such as filtering and categorization) is handled in the view model, keeping it close to where it is used without introducing unnecessary complexity into the core layer.

## Building from Source

### Prerequisites
**.NET 10 SDK** is required to build, run, and test this project from source.
- **Download:** [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- **Verify installation:**
  ```bash
  dotnet --version
  ```
  Should show 10.0.xxx or higher.

### Commands
Run these in the terminal from the repository root

| Task | Command |
|------|---------|
| **Build (Debug)** | `dotnet build` |
| **Build (Release)** | `dotnet build -c Release` |
| **Run** | `dotnet run` |
| **Run (Release)** | `dotnet run -c Release` |
| **Run tests** | `dotnet test` |
| **Run tests (Release)** | `dotnet test -c Release` |

### Executable Location
**On Windows:**
| Build Type | Path (relative to repository root) |
|---------------------|---------------------------------|
| **Debug**   | `JetRestaurantLookup\bin\Debug\net10.0\JetRestaurantLookup.exe` |
| **Release** | `JetRestaurantLookup\bin\Release\net10.0\JetRestaurantLookup.exe` |

As of now this project has only been tested on Windows 11