using System;
using System.Runtime.Serialization;

namespace DryIoc.Facilities.AutoTx.Errors
{
#pragma warning disable CA2237
    public class AutoTxFacilityException : InvalidOperationException
    {
	    public AutoTxFacilityException()
	    {
	    }

	    protected AutoTxFacilityException(SerializationInfo info, StreamingContext context) : base(info, context)
	    {
	    }

	    public AutoTxFacilityException(string message) : base(message)
	    {
	    }

	    public AutoTxFacilityException(string message, Exception innerException) : base(message, innerException)
	    {
	    }
    }
}
