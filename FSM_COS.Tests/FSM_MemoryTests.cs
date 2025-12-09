namespace FSM_COS.Tests;

[TestFixture]
public class FSM_MemoryTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void FSM_MemoryInitializes()
    {
        var availableMemory = GC.GetTotalMemory(true);
        FSM_Memory.Initialize();
        Assert.That(availableMemory > GC.GetTotalMemory(false));
    }
}
