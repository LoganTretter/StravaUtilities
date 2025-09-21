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

## Usage

Currently everything flows through the [StravaApiClient](src/StravaApiClient.cs) class.

There is just one namespace, ```StravaUtilities```, to keep it simple / easy to use.

### With an auth storage provider (preferred)

If you implement the [IStravaApiAthleteAuthInfoStorer](src/Auth/IStravaApiAthleteAuthInfoStorer.cs) interface and provide it to the client, then the client will manage authorization for any subsequent call.

```cs
using StravaUtilities;

...

// assuming stravaApiClientId and StravaApiClientSecret are string variables you populate somehow

// assuming you implement this storer somehow
IStravaApiAthleteAuthInfoStorer authStorer;

// Create a client WITH an auth storage provider
using var client = new StravaApiClient(stravaApiClientId, stravaApiClientSecret, authStorer);

// Make calls - the client will use the storer to get auth info, and cache it
// It will refresh if access token is expired, and store the new token back using the storer

// assuming athleteId is a long variable you populate somehow
var athlete = await client.GetAthlete(athleteId);
```


### Without an auth storage provider

Without a [IStravaApiAthleteAuthInfoStorer](src/Auth/IStravaApiAthleteAuthInfoStorer.cs) provided, you need to manually give auth info to each call.

```cs
using StravaUtilities;

...

// assuming stravaApiClientId and StravaApiClientSecret are string variables you populate somehow

// Create a client WITHOUT an auth storage provider
using var client = new StravaApiClient(stravaApiClientId, stravaApiClientSecret);

// Example auth info that you would need to manage manually... Need to provide at least the AccessToken
var authInfo = new StravaApiAthleteAuthInfo
{
    TokenInfo = new StravaApiTokenInfo
    {
        AccessToken = "TheToken123"
    }
};

// Make calls - auth info must be provided manually each time

// assuming athleteId is a long variable you populate somehow
var athlete = await client.GetAthlete(athleteId, authInfo);
```

## Auth

TODO

https://developers.strava.com/docs/authentication/

## Testing

There is a [test harnness](tests/TestHarness) project, just a console app, that can be used to manually run tests. I just call whatever test I want in [ManualTestRunner](tests/TestHarness/ManualTestRunner.cs).

At this point there are no actual unit tests, because I mostly want to actually call the Strava API, in particular for uploading or updating activities. I want to "clean up" my test activities when I do this, so it's better suited for manual testing.

Configure the test project for your Strava API app, athlete, activities, etc., using an ```appsettings.json``` file (see [appsettings_sample.json](tests/TestHarness/appsettings_sample.json) for an example).

## TODO / Wish List

- Write more about usage
- Write about the auth process
- Integrate scope checks into the API calls
- Add the rest of the Strava API calls and models into the library
- Reorganize into models and api structure
- Make the client thread safe
- Improve error handling / more organization to exceptions
