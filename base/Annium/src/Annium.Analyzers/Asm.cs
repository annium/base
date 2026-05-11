// Annium.Analyzers is a Roslyn analyzer / code-fix assembly (netstandard2.1). It is NOT a runtime
// component, so the [assembly: AutoScanned] marker the rest of Annium uses for runtime-discovery
// does not apply here. The only attribute this file carries is InternalsVisibleTo for the test
// project, which exercises the Descriptors / analyzer types directly.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Annium.Analyzers.Tests")]
