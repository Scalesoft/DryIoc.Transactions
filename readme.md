# DryIoc Transactions (port from Castle.Transactions https://github.com/castleproject/Castle.Transactions)

A project for transaction management on .NET Standard.

## Quick Start

NuGet package is currently not exist.

### ASP.NET Core

If using Transaction attribute in method that is called directly from Startup method then all requests will have the same Activity (it is the same AsyncLocal context).
To resolve this problem call ResetAutoTxActivityContext extension method on IContainer instance from DryIoc after all Transaction operations from Startup has finished.

### Castle Transactions

The original project that manages transactions.

#### Main Features

 * Regular Transactions (+`System.Transactions` interop) - allows you to create transactions with a nice API
 * Dependent Transactions - allows you to fork dependent transactions automatically by declarative programming: `[Transaction(Fork=true)]`
 * Transaction Logging - A trace listener in namespace `DryIoc.Transactions.Logging`, named `TraceListener`.
 * Retry policies for transactions

#### Main Interfaces

 - `ITransactionManager`:
   - *default implementation is `TransactionManager`*
   - keeps tabs on what transaction is currently active
   - coordinates parallel dependent transactions
   - keep the light weight transaction manager (LTM) happy on the CLR

