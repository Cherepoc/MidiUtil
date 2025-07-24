using System.Buffers.Binary;
using System.IO;

namespace MidiUtil.Midi;

/// <summary>
///     Big endian binary reader
/// </summary>
public sealed class BigEndianBinaryReader
{
    private readonly byte[] _data;

    public BigEndianBinaryReader(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        _data = memoryStream.ToArray();
    }

    public int Position { get; private set; }

    public int Length => _data.Length;

    public byte[] ToArray()
    {
        var newArray = new byte[_data.Length];
        Array.Copy(_data, newArray, _data.Length);
        return newArray;
    }

    public byte ReadByte()
    {
        ValidateReadLength(1);

        return _data[Position++];
    }

    public byte[] ReadBytes(int length)
    {
        ValidateReadLength(length);

        var span = _data.AsSpan(Position, length);
        Position += length;
        return span.ToArray();
    }

    public ushort ReadUInt16()
    {
        const int length = 2;
        ValidateReadLength(length);

        var span = _data.AsSpan(Position, length);
        var value = BinaryPrimitives.ReadUInt16BigEndian(span);
        Position += length;
        return value;
    }

    public uint ReadUInt32()
    {
        const int length = 2;
        ValidateReadLength(length);

        var span = _data.AsSpan(Position, length);
        var value = BinaryPrimitives.ReadUInt32BigEndian(span);
        Position += length;
        return value;
    }

    private void ValidateReadLength(int length)
    {
        var newPosition = Position + length;
        if (newPosition > _data.Length)
            throw new ArgumentException("Attempt to read beyond the end of the data.", nameof(length));
    }
}
