using Xunit;

// These are real-network integration tests: each test class spins up a live TCP/SSL socket server
// and client. Running test collections in parallel makes them contend for CPU and OS socket/port
// resources, which under load produces transient connect failures and round-trips that exceed the
// Expect.ToAsync timeout — i.e. intermittent "message never arrived" flakiness. Disabling
// parallelization makes the suite deterministic (and, because the tests are I/O-bound, no slower in
// practice).
[assembly: CollectionBehavior(DisableTestParallelization = true)]
