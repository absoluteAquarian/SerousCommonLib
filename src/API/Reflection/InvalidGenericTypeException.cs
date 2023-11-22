using System;

namespace SerousCommonLib.API {
	#pragma warning disable CS1591
	public class InvalidGenericTypeException : Exception {
		public InvalidGenericTypeException(string typeString) : base ($"Type \"{typeString}\" cannot be used in this context") { }

		public InvalidGenericTypeException(Type target) : base ($"Type \"{target.GetSimplifiedGenericTypeName()}\" cannot be used in this context") { }

		public InvalidGenericTypeException(Type source, Type target) : base($"Type \"{source.GetSimplifiedGenericTypeName()}\" cannot be implicitly converted to type \"{target.GetSimplifiedGenericTypeName()}\"") { }
	}
}
