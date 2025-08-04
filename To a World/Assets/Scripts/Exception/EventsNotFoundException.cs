using System;

public class EventsNotFoundException: Exception
{
    public EventsNotFoundException() : base("Events not found") { }
    public EventsNotFoundException(string message) : base(message) { }
}