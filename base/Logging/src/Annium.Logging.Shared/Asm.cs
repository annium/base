using System.Runtime.CompilerServices;
using Annium.Core.Runtime.Types;
using Annium.Logging;

[assembly: AutoScanned(typeof(LogLevel))]
[assembly: InternalsVisibleTo("Annium.Logging.Shared.Tests")]
[assembly: InternalsVisibleTo("Annium.Logging.InMemory.Tests")]
[assembly: InternalsVisibleTo("Annium.Graylog.Logging.Tests")]
[assembly: InternalsVisibleTo("Annium.Seq.Logging.Tests")]
