using Xunit.Sdk;
using Xunit.v3;

// These are real-network integration tests: each test class spins up a live TCP socket server
// and client. Running test collections in parallel makes them contend for CPU and OS socket/port
// resources, producing transient connect failures and intermittent timeouts.
// Disabling parallelization makes the suite deterministic.
[assembly: Parallelization(Mode = ParallelMode.None)]
