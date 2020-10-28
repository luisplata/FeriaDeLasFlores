using System;
using System.Runtime.Serialization;

public class SinEspacioParaPonerMasFloresException : Exception
{
    public SinEspacioParaPonerMasFloresException()
    {
    }

    public SinEspacioParaPonerMasFloresException(string message) : base(message)
    {
    }

    public SinEspacioParaPonerMasFloresException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected SinEspacioParaPonerMasFloresException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
