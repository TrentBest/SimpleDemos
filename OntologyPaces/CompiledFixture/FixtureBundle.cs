using TheSingularityWorkshop.MicroBundleDomain;

namespace SimpleDemos.CompiledFixture;

public sealed class FixtureBundle : IMicroBundle
{
    public MicroBundleDescriptor Descriptor { get; } =
        new(64001, "1.0.0", providers: [new MicroBundleProvider("simpledemos.fixture")]);

    public IReadOnlyList<MicroBundleDependencyRequest> Dependencies { get; } = [];

    public void Load(IMicroBundleLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
    }

    public bool Arbitrate(IMicroBundleArbitrationContext context, int roundIndex)
    {
        ArgumentNullException.ThrowIfNull(context);
        return false;
    }
}
