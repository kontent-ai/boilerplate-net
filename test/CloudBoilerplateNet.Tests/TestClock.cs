using Microsoft.Extensions.Internal;
using System;

namespace CloudBoilerplateNet.Tests
{
    public class TestClock : ISystemClock
    {
        DateTimeOffset ISystemClock.UtcNow => new DateTimeOffset(DateTime.UtcNow);
    }
}