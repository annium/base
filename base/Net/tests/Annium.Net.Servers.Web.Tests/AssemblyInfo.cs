using Xunit;

// These are real-network integration tests: each test class spins up a live HttpListener web server
// and HTTP/WebSocket client. Running test collections in parallel makes them contend for CPU and OS
// socket/port resources, and the resulting load spike destabilizes the timing-sensitive WebSocket
// close-handshake tests in Annium.Net.WebSockets.Tests when both suites run concurrently under `just test`.
// Disabling parallelization keeps this assembly's peak concurrent-server count at one.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
