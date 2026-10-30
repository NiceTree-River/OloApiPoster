# OLO API Desktop Tool

OLO API Windows desktop tool for testing GET, POST, and PUT requests.

## Installation and Usage

This project includes a ready-to-run Windows application. You do not need to install .NET or any development tools.

1. Download `OloApiDesktopTool-Setup.zip` from the file list in this GitHub repository.
2. Extract the ZIP file to any folder.
3. Open the extracted folder, enter the `app` subfolder, and double-click `OloApiDesktopTool.exe`.
4. The application is ready to use.

> If Windows displays a security warning, confirm that the file is from a trusted source and choose **Run**. Extract the ZIP completely before launching the executable.

## Features

- **Restaurant JSON**: Enter the Domain, Key, Secret, and Restaurant ID to retrieve restaurant details.
- **Custom Request**: Send a signed request to a custom domain and path with an optional JSON request body.
- **Restaurant XML Export**: Enter the required information to download and save restaurant menu XML.

The default API domain is `https://ordering.api.olo.com`. Each feature tab has its own editable Domain field. Secret fields are masked for privacy.

## Running from Source

To modify or run the source code, install the .NET 8 SDK and run the following command from the project directory:

```powershell
dotnet run --project .\OloApiDesktopTool.csproj
```
