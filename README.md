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

 TODO
