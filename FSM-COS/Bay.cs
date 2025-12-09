using System.Runtime.CompilerServices;

public class Bay<T> : IBay where T : new()
{
    private T[] _data;
    private ulong[] _aliveMask; // 1 = Alive, 0 = Dead/Available
    private int _capacity;
    private int _cursorHint = 0;

    public Bay(int capacity)
    {
        _capacity = capacity;
        _data = new T[capacity];

        // Bitmask: 1 ulong tracks 64 objects. 
        _aliveMask = new ulong[(capacity + 63) / 64];

        // INITIALIZATION: "We take the hit... zero out... initialize"
        for (int i = 0; i < capacity; i++) _data[i] = new T();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Take()
    {
        // "Using the bit field showing alive or dead"
        // We look for a '0' bit.

        for (int i = 0; i < _aliveMask.Length; i++)
        {
            int index = (i + _cursorHint) % _aliveMask.Length; // Rotational scan
            ulong mask = _aliveMask[index];

            if (mask != ulong.MaxValue) // If not all bits are 1 (Full)
            {
                // Find the first zero bit (This is a simplified bit-scan)
                for (int bit = 0; bit < 64; bit++)
                {
                    if ((mask & (1UL << bit)) == 0)
                    {
                        // Found a dead spot. Resurrect it.
                        _aliveMask[index] |= (1UL << bit); // Flip to Alive
                        _cursorHint = index; // Cache locality

                        // "We return the first dead object... no initialization needed"
                        return _data[(index * 64) + bit];
                    }
                }
            }
        }

        // Failsafe: If Bay is full, we should technically chain to a new Bay.
        throw new Exception("BAY FULL - EXPANSION LOGIC REQUIRED");
    }

    public void Free(T item)
    {
        // In a real pointer implementation, we'd calculate offset.
        // Here, we'd need the ID to flip the bit back to 0.
    }

    // "We idle update... so we don't need to initialize"
    public void IdleScrub()
    {
        // This runs when the OS isn't looking.
        // It iterates through DEAD objects and resets their data to default values.
        // This ensures that 'Take()' returns a clean object instantly.
    }
}