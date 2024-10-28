# Customer Onboarding Starter Kit

The *Customer Onboarding Starter Kit* is a sample application intended to assist Geotab Platform Partners - resellers primarily using  the API-only option, which precludes the use of the MyGeotab interface - in onboarding new customers by automating certain tasks using MyGeotab SDK-based workflows.  It may be run as-is if it satisfies all requirements, or it can be modified as needed.  Taking the form of a .NET 8.0 console application using the MyGeotab .NET SDK, the code is focused on API calls and associated logic rather than being overwhelmed with application, framework or user interface specific code.

Two utilities, outlined as follows, are included in the Customer Onboarding Starter Kit:

**Create Database & Load Devices:**
- Creates a new MyGeotab database
- Adds an administrative/service user to the new database
- Uploads a list of devices from a CSV file into the new database

**Update Devices:**
- Connects to an existing MyGeotab database
- Loads a list of devices from a CSV file and adds or updates devices in the MyGeotab database, setting the name and driver feedback options of the devices as defined in the CSV file.

## Prerequisites

The sample application requires:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or higher
- MyAdmin credentials (entered manually when prompted) with the *MyAdminApiUser* and *Device_Admin* roles.

## Getting started

**IMPORTANT:**  See the [Customer Onboarding Starter Kit User Guide](https://docs.google.com/document/d/16Z9gHSgOjNJtSBVLngNPXQlgW2ikPWjXZRLkl8HAqHY) before attempting to run this application; the user guide contains important information about necessary configuration.

```shell
> git clone https://github.com/Geotab/Customer-Onboarding-Starter-Kit.git customer-onboarding-starter-kit
> cd customer-onboarding-starter-kit
> dotnet run
```