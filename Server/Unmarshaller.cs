using System.Reflection;
using System.Text;

namespace Server;

internal static class Unmarshaller
{
    public static object[] unmarshall(ArraySegment<byte> message, ParameterInfo[] parameters)
    {
        if (parameters.Length == 0)
            return null;

        var array = new object[parameters.Length];


        var arrayIndex = 0;
        var byteArrayIndex = 0;

        foreach (var parameterInfo in parameters)
        {
            if (parameterInfo.ParameterType == typeof(int))
            {
                array[arrayIndex] = BitConverter.ToInt32(message.Array, byteArrayIndex + message.Offset);
                byteArrayIndex += sizeof(int);
            }
            else if (parameterInfo.ParameterType == typeof(float))
            {
                array[arrayIndex] = BitConverter.ToSingle(message.Array, byteArrayIndex + message.Offset);
                byteArrayIndex += sizeof(float);
            }
            else if (parameterInfo.ParameterType == typeof(string))
            {
                var sb = new StringBuilder(message.Count);

                while (message[byteArrayIndex] != 0)
                {
                    sb.Append((char)message[byteArrayIndex]);
                    byteArrayIndex++;
                }

                array[arrayIndex] = sb.ToString();

                byteArrayIndex++;
            }
            else
            {
                throw new Exception($"Unsupported parameter type {parameterInfo.ParameterType.Name}");
            }

            arrayIndex++;
        }

        return array;
    }
}