# StravaUtilities

A library for making calls to the [Strava API](https://developers.strava.com/docs/), written in C#.

Mostly it contains an API client. But I am calling it "utilities" because I may add more complex functions to it than just a pure API client would have.

## Prerequisites

To call the API, you need to go through your "API application", kind of like your access policy to use the Strava API.

They describe how to create an API app here: https://developers.strava.com/docs/getting-started/#account

## Technologies / references

Standard tools:

- [.NET](https://learn.microsoft.com/en-us/dotnet/)
  - [C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/) for storing Strava API access token
  - [SecretClient](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/security.keyvault.secrets-readme?view=azure-dotnet) for interfacing with vault

## Auth

TODO

https://developers.strava.com/docs/authentication/

## Testing

There is a [test harnness](tests/TestHarness) project, just a console app, that can be used to manually run tests. I just call whatever test I want in [ManualTestRunner](tests/TestHarness/ManualTestRunner.cs).

At this point there are no actual unit tests, because I mostly want to actually call the Strava API, in particular for uploading or updating activities. I want to "clean up" by test activities when I do this, so it's better suited for manual testing.

Configure the test project for your Strava API app, athlete, activities, etc., using an ```appsettings.json``` file (see [appsettings_sample.json](tests/TestHarness/appsettings_sample.json) for an example).
