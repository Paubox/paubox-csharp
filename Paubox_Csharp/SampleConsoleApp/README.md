# Paubox C# Sample Console Application

This is a sample console application demonstrating how to use the Paubox C# SDK.

## Setup

1. **Configure your API credentials:**

   ```sh
   # Copy the example configuration file
   cp appsettings.example.json appsettings.json

   # Edit the file with your actual credentials
   # Replace the placeholder values with your real API key and username
   ```

2. **Build and run:**

   ```bash
   dotnet build
   dotnet run
   ```

## Configuration

The application uses `appsettings.json` for configuration. The file contains:

- `APIKey`: Your Paubox API key
- `APIUser`: Your Paubox username/domain
- `FromEmail`: The email address that will appear as the sender
- `ToEmail`: The email address that will receive the test message

**Security Note**: The `appsettings.json` file is ignored by git to protect your credentials. Always use `appsettings.example.json` as a template.

## What it does

This sample application demonstrates:

- Sending an email with attachments
- Getting email disposition (delivery status)

The email is sent from the address specified in `FromEmail` to the address specified in `ToEmail` in your configuration file.
