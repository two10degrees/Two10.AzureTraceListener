# Two10.AzureTraceListener

A trace listener designed to run on an on-premesis machine, which will write trace information in batches to a table in Windows Azure Table Storage.

## Installation

From NuGet:

```
PM> Install-Package Two10.AzureTraceListener
```

Clone from GitHub:

```
https://github.com/richorama/Two10.AzureTraceListener.git
```

Then open the solution in Visual Studio and build build.

## Usage

* Add a reference to the `Two10.AzureTraceListener` assembly in your project.

* Update your App.config to set up the trace listener:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <trace>
      <listeners>
        <add name="TableTraceListener"
              type="Two10.AzureTraceListener.TableTraceListener, Two10.AzureTraceListener"
              connectionString="UseDevelopmentStorage=true" 
              tableName="MyTraceTable" />
        <remove name="Default"/>
      </listeners>
    </trace>
  </system.diagnostics>
</configuration>
```
You should set the `connectionString` value to be the connection string of your Windows Azure Storage Account.

> Note: `tableName` is optional, and will default to `trace`.

* Write to `Trace` in your application:

```cs
System.Diagnostics.Trace.WriteLine("Hello World");
```

## How it works

The Trace Listener will periodically (every minute) write any trace information to your table, with the message in a `Value` column. 

The `PartitionKey` is comprised of the machine name and the date:

```
MACHINENAME-YYYY-MM-DD
```

The `RowKey` is the remaining ticks in the epoch, therefore more recent records are written last. 

Example records:

<table class="table table-striped table-bordered table-condensed">
<tbody>
<tr><th>PartitionKey</th><th>RowKey</th><th>Timestamp</th><th>Value</th></tr><tr><td>DISCOVERY-2014-03-21</td><td>2520068972872977706</td><td>2014-03-21T12:05:15.102Z</td><td>Trace message at 21/03/2014 12:05:12</td></tr>
<tr><td>DISCOVERY-2014-03-21</td><td>2520068972882984955</td><td>2014-03-21T12:05:15.102Z</td><td>Trace message at 21/03/2014 12:05:11</td></tr>
</tbody></table>

If a category is supplied, this will also be recorded in the table.

The `Two10.AzureTraceListener.TraceEntity` class can be used to query the table.

## License

MIT