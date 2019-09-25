using Microsoft.Extensions.Internal;
using System;

namespace Kentico.Kontent.Boilerplate.Tests
{
    public class TestClock : ISystemClock
    {
        DateTimeOffset ISystemClock.UtcNow => new DateTimeOffset(DateTime.UtcNow);
    }
}