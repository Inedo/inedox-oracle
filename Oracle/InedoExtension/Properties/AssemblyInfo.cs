using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Oracle")]
[assembly: AssemblyDescription("Database integration for Oracle 9i and later.")]
[assembly: AssemblyCompany("Inedo, LLC")]
[assembly: AssemblyProduct("BuildMaster")]
[assembly: AssemblyCopyright("Copyright © Inedo 2018")]
[assembly: AssemblyVersion("0.0.0.0")]
[assembly: AssemblyFileVersion("0.0")]
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

#if DEBUG
[assembly: InternalsVisibleTo("OracleUnitTests")]
#endif
