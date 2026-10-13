## HH Data Management .NET API wrapper examples

This repository contains three different solutions with some examples of how the HH DM .NET API wrapper can be used.

In all examples except for the OAuth specific one, an [API key](https://help.hh-dm.com/extensibility/api/#authentication) is needed to authenticate against the API.

### Central configuration

The examples do not set their own target framework or package versions.

- `Directory.Build.props` defines `DotNetTargetFramework`, which sets the target framework for every example. The WPF example is the exception: it targets `net9.0-windows`, because the DevExpress WPF version it uses (24.2) does not support .NET 10.
- `Directory.Packages.props` uses [central package management](https://learn.microsoft.com/nuget/consume-packages/central-package-management) to define the `HHDev.DataManagement.ApiClientWrapper` version for every example.

Change the version in one of those files to move all examples at once.

### SimpleConsoleApiWrapperExample

This is a very simple example of a console application that gets a run, calculates the average lap time for the run and then sends the value back to the API.

To get started, the `API_KEY`, `accountId` and `runSheetId` variables need to be populated with values.

### WpfApiWrapperExample

This is a more complete example built as a WPF application.  The application allows the user to select an account, championship, event and car.  The setups for this event and car are then loaded.  The user can select a setup, and then edit it or delete it.  New setups can also be created, and the change history of the selected setup (who changed which parameter, from what to what, and when) can be viewed using the `/Setups/History/{id}` endpoint. The history is shown in a DevExpress `GridControl`, which gives every column an Excel-style filter drop-down, a search panel over all columns, grouping and a row count.  The attached files for the selected setup can also be downloaded, deleted or new files can be added.

To get started, run the application, enter your API key in the field at the top of the window and click **Connect** (or press Enter). The accounts available to that key are then loaded. You can enter a different key and click **Connect** again at any time. With **Remember key** ticked, the last key that connected successfully is saved for your Windows user (encrypted with DPAPI, in `%LOCALAPPDATA%\HHDM\WpfApiWrapperExample`) and used automatically the next time the application starts.

#### DevExpress requirement

The setup history window uses the DevExpress WPF `GridControl` (`DevExpress.Wpf.Grid`, version set in `Directory.Packages.props`), so building this example needs a DevExpress WPF licence and access to the DevExpress NuGet feed. Add your personal feed once for your Windows user (the URL is in your account at https://nuget.devexpress.com) rather than committing it to this repository:

```
dotnet nuget add source https://nuget.devexpress.com/<your-feed-key>/api -n "DevExpress"
```

If the DevExpress installer is already set up on your machine, its local package source works too.

### OAuthConsoleApiWrapperExample

This is a very simple example that uses OAuth to retrieve all the accounts available to the user that logs in.

This example does not need to be populated with any values and can be run as-is.
