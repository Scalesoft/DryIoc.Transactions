﻿#if !NETCOREAPP

using System.Threading;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

#endif
