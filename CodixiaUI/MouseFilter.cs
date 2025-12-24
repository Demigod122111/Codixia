namespace Codixia.UI;

public enum MouseFilter
{
    Stop,   // Consume input, stop propagation
    Pass,   // Receive input, allow propagation
    Ignore  // Do not receive input at all
}