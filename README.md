## HH Data Management .NET API wrapper examples

This repository contains two different solutions with some examples of how the HH DM .NET API wrapper can be used.

In all examples except for the OAuth specific one, an [API key](https://help.hh-dm.com/extensibility/api/#authentication) is needed to authenticate against the API.

### SimpleConsoleApiWrapperExample

This is a very simple example of a console application that gets a run, calculates the average lap time for the run and then sends the value back to the API.

To get started, the `API_KEY`, `accountId` and `runSheetId` variables need to be populated with values.

### WpfApiWrapperExample

This is a more complete example built as a WPF application.  The application allows the user to select an account, championship, event and car.  The setups for this event and car are then loaded.  The user can select a setup, and then edit it or delete it.  New setups can also be created.  The attached files for the selected setup can also be downloaded, deleted or new files can be added.

To get started, the `API_KEY` variable need to be populated with values.

### OAuthConsoleApiWrapperExample

This is a very simple example that uses OAuth to retrieve all the accounts available to the user that logs in.

This example does not need to be populated with any values and can be run as-is.
