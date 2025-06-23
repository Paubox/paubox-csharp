# Contributing <!-- omit in toc -->

Bug reports and pull requests are welcome on GitHub at <https://github.com/paubox/paubox-csharp>.

- [Setting up](#setting-up)
- [Running the unit tests](#running-the-unit-tests)
- [Running the sample console app](#running-the-sample-console-app)

## Setting up

You can use a Microsoft IDE such as Microsoft Visual Studio or Microsoft Visual Studio Code.

If you do not wish to do that, follow the steps below:

1. Install the .NET CLI

    ```sh
    sudo apt-get install dotnet-sdk-8.0 # Ubuntu
     # or
    brew install --cask dotnet-sdk # macOS
    ```

2. Restore the projects

    ```sh
    dotnet restore Paubox_Csharp/Paubox_Csharp.sln
    ```

3. Build the projects

    ```sh
    dotnet build Paubox_Csharp/Paubox_Csharp.sln
    ```

## Running the unit tests

A separate unit test project is provided in `Paubox_Csharp/UnitTestProject/UnitTestProject.csproj` to test the library.

```sh
dotnet test Paubox_Csharp/UnitTestProject/UnitTestProject.csproj
```

## Running the sample console app

A separate sample project is provided in `Paubox_Csharp/SampleConsoleApp/SampleConsoleApp.csproj` to perform QA on the
library.

```sh
dotnet run --project Paubox_Csharp/SampleConsoleApp/SampleConsoleApp.csproj
```
