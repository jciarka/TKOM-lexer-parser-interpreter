using System.Reflection;

namespace Application.Infrastructure.Lekser.Extentions
{
    public static class StreamReaderExtentions
    {
        readonly static FieldInfo charPosField = typeof(StreamReader).GetField("charPos", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        readonly static FieldInfo byteLenField = typeof(StreamReader).GetField("byteLen", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        readonly static FieldInfo charBufferField = typeof(StreamReader).GetField("charBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

        public static long GetPosition(this StreamReader reader)
        {
            var bufferOffset = reader.BaseStream.Position - (int)byteLenField.GetValue(reader)!;
            var innerBufferPosition = getInternalBufferPosition(reader);

            return bufferOffset + innerBufferPosition;
        }

        public static void SetPosition(this StreamReader reader, long position)
        {
            reader.DiscardBufferedData();
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        private static int getInternalBufferPosition(StreamReader reader)
        {
            var charBuffer = (char[])charBufferField.GetValue(reader)!;
            int charPostion = (int)charPosField.GetValue(reader)!;
            int bytePostion = reader.CurrentEncoding.GetBytes(charBuffer, 0, charPostion).Length;

            return bytePostion;
        }
    }
}

