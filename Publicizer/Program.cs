using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Publicizer {
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	class DummyAttribute : Attribute { }

	// NOTE: This project builds a functional assembly, but the reference to Enumerable+Iterator<T> is still invalid
	//       Hence, this project will just be left here as a proof of concept.

	internal static class Program {
		private static ModuleDefinition writingModule = null!;

		private const System.Reflection.BindingFlags NonPublic = System.Reflection.BindingFlags.NonPublic;
		private const System.Reflection.BindingFlags Instance = System.Reflection.BindingFlags.Instance;

		public static void Main() {
			try {
				// Create the assembly that will be written to disk
				AssemblyDefinition assembly = new AssemblyDefinition("SerousCommonLib.Publicizer", new Version(1, 0, 5, 2));
			
				writingModule = new ModuleDefinition("SerousCommonLib.Publicizer", KnownCorLibs.SystemPrivateCoreLib_v6_0_0_0);
				assembly.Modules.Add(writingModule);

				// Add the types
				var attr = ConstructIgnoresAccessChecksToAttribute(out MethodDefinition attrCtor);
				ConstructIterator(attr);

				// Add the custom attribute to the assembly
				assembly.CustomAttributes.Add(new CustomAttribute(attrCtor,
					new CustomAttributeSignature(
						new CustomAttributeArgument(writingModule.CorLibTypeFactory.String, "System.Linq")
					)));

				// Write the assembly to disk
				Directory.CreateDirectory("out");
				assembly.Write(Path.Combine("out", "SerousCommonLib.Publicizer.dll"));
			} catch (Exception ex) {
				Console.WriteLine("Assembly generation failed: \n" + ex);
				Console.ReadKey(true);
			}
		}

		private static TypeDefinition ConstructIgnoresAccessChecksToAttribute(out MethodDefinition attrCtor) {
			// Create the type definition
			TypeDefinition attr = new TypeDefinition(
				"System.Runtime.CompilerServices",
				"IgnoresAccessChecksToAttribute",
				TypeAttributes.NotPublic | TypeAttributes.Sealed);

			writingModule.TopLevelTypes.Add(attr);

			// Assign an AttributeUsage attribute to the type
			var attributeUsage = writingModule.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", nameof(AttributeUsageAttribute))
				.ImportWith(writingModule.DefaultImporter);
			var attributeTargets = writingModule.CorLibTypeFactory.CorLibScope.CreateTypeReference("System", nameof(AttributeTargets))
				.ImportWith(writingModule.DefaultImporter)
				.ToTypeSignature();
			var attributeUsageCtor = attributeUsage.CreateMemberReference(".ctor",
				MethodSignature.CreateInstance(
					writingModule.CorLibTypeFactory.Void,
					attributeTargets
				))
				.ImportWith(writingModule.DefaultImporter);

			attr.CustomAttributes.Add(new CustomAttribute(attributeUsageCtor,
				new CustomAttributeSignature(
					new CustomAttributeArgument[] {
						new CustomAttributeArgument(attributeTargets, AttributeTargets.Assembly)
					},
					new CustomAttributeNamedArgument[] {
						new CustomAttributeNamedArgument(CustomAttributeArgumentMemberType.Property,
							nameof(AttributeUsageAttribute.AllowMultiple),
							writingModule.CorLibTypeFactory.Boolean,
							new CustomAttributeArgument(writingModule.CorLibTypeFactory.Boolean, true))
					}
				)));

			// Add the base type
			attr.BaseType = writingModule.DefaultImporter.ImportType(typeof(Attribute));

			// Add the assembly name field
			var assemblyNameField = new FieldDefinition("_assemblyName",
				FieldAttributes.Private,
				new FieldSignature(writingModule.CorLibTypeFactory.String));

			attr.Fields.Add(assemblyNameField);

			var assemblyNameFieldDescriptor = writingModule.DefaultImporter.ImportField(assemblyNameField);

			// Add the constructor
			attr.Methods.Add(attrCtor = CreateIgnoresAccessChecksToAttribute_MakeConstructor(attr.BaseType, assemblyNameFieldDescriptor));

			// Add the AssemblyName property
			var assemblyNameProperty = CreateIgnoresAccessChecksToAttribute_MakeProperty_AssemblyName(assemblyNameFieldDescriptor);
			attr.Properties.Add(assemblyNameProperty);
			attr.Methods.Add(assemblyNameProperty.GetMethod!);

			return attr;
		}

		private static MethodDefinition CreateIgnoresAccessChecksToAttribute_MakeConstructor(ITypeDefOrRef baseType, IFieldDescriptor assemblyNameField) {
			MethodDefinition ctor = new MethodDefinition(".ctor",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void, writingModule.CorLibTypeFactory.String));

			// Get the base type's constructor
			var baseCtor = baseType.CreateMemberReference(".ctor", MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void))
				.ImportWith(writingModule.DefaultImporter);

			// Create the constructor body
			CilMethodBody body;
			ctor.MethodBody = body = new CilMethodBody(ctor);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Call, baseCtor);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Ldarg_1);
			body.Instructions.Add(CilOpCodes.Stfld, assemblyNameField);
			body.Instructions.Add(CilOpCodes.Ret);

			return ctor;
		}

		private static PropertyDefinition CreateIgnoresAccessChecksToAttribute_MakeProperty_AssemblyName(IFieldDescriptor assemblyNameField) {
			PropertyDefinition assemblyNameProperty = new PropertyDefinition("AssemblyName",
				PropertyAttributes.None,
				PropertySignature.CreateInstance(writingModule.CorLibTypeFactory.String));

			// Create the get method
			MethodDefinition assemblyNameProperty_get = new MethodDefinition("get_AssemblyName",
				MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.String));

			// Create the get method body
			CilMethodBody body;
			assemblyNameProperty_get.MethodBody = body = new CilMethodBody(assemblyNameProperty_get);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Ldfld, assemblyNameField);
			body.Instructions.Add(CilOpCodes.Ret);

			// Assign the get method to the property
			assemblyNameProperty.SetSemanticMethods(assemblyNameProperty_get, null);

			return assemblyNameProperty;
		}

		private static TypeDefinition ConstructIterator(TypeDefinition attr) {
			Type corLibIterator = typeof(Enumerable).GetNestedType("Iterator`1", NonPublic)
				?? throw new InvalidOperationException("Could not find the Iterator<T> type in System.Linq.Enumerable");
			
			GenericParameter genericParameter = new GenericParameter("TSource", GenericParameterAttributes.VarianceMask);

			// Create the type definition
			TypeDefinition iterator = new TypeDefinition(
				"SerousCommonLib.Publicizer",
				"Iterator_Public`1",
				TypeAttributes.Abstract | TypeAttributes.Public);

			writingModule.TopLevelTypes.Add(iterator);

			// Add the generic parameter
			iterator.GenericParameters.Add(genericParameter);
			
			GenericParameterSignature genericParameterSignature = new GenericParameterSignature(GenericParameterType.Type, genericParameter.Number);

			// Set the base type to the corlib iterator
			iterator.BaseType = writingModule.DefaultImporter.ImportType(corLibIterator)
				.MakeGenericInstanceType(genericParameterSignature)
				.ToTypeDefOrRef();

			// Add the constructor
			iterator.Methods.Add(CreateIterator_MakeConstructor(iterator.BaseType));

			// Create a property that accesses the "internal int _state" field
			var stateProperty = CreateIterator_MakeProperty_State(iterator.BaseType);
			iterator.Properties.Add(stateProperty);
			iterator.Methods.Add(stateProperty.GetMethod!);
			iterator.Methods.Add(stateProperty.SetMethod!);

			// Create a method that accesses the "internal TSource _current" field
			iterator.Methods.Add(CreateIterator_MakeMethod_SetCurrent(iterator.BaseType, genericParameterSignature));

			return iterator;
		}

		private static MethodDefinition CreateIterator_MakeConstructor(ITypeDefOrRef baseType) {
			MethodDefinition ctor = new MethodDefinition(".ctor",
				MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void));

			// Get the base type's constructor
			var baseCtor = baseType.CreateMemberReference(".ctor", MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void))
				.ImportWith(writingModule.DefaultImporter);

			// Create the constructor body
			CilMethodBody body;
			ctor.MethodBody = body = new CilMethodBody(ctor);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Call, baseCtor);
			body.Instructions.Add(CilOpCodes.Ret);

			return ctor;
		}

		private static PropertyDefinition CreateIterator_MakeProperty_State(ITypeDefOrRef baseType) {
			PropertyDefinition stateProperty = new PropertyDefinition("State",
				PropertyAttributes.None,
				PropertySignature.CreateInstance(writingModule.CorLibTypeFactory.Int32));

			// Get the _state field
			var stateField = baseType.CreateMemberReference("_state", new FieldSignature(writingModule.CorLibTypeFactory.Int32))
				.ImportWith(writingModule.DefaultImporter);

			// Create the get method
			MethodDefinition stateProperty_get = new MethodDefinition("get_State",
				MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Int32));

			// Create the get method body
			CilMethodBody body;
			stateProperty_get.MethodBody = body = new CilMethodBody(stateProperty_get);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Ldfld, stateField);
			body.Instructions.Add(CilOpCodes.Ret);

			// Create the set method
			MethodDefinition stateProperty_set = new MethodDefinition("set_State",
				MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void, writingModule.CorLibTypeFactory.Int32));

			stateProperty_set.Parameters[0].GetOrCreateDefinition().Name = "value";

			// Create the set method body
			stateProperty_set.MethodBody = body = new CilMethodBody(stateProperty_set);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Ldarg_1);
			body.Instructions.Add(CilOpCodes.Stfld, stateField);
			body.Instructions.Add(CilOpCodes.Ret);

			// Assign the get and set methods to the property
			stateProperty.SetSemanticMethods(stateProperty_get, stateProperty_set);

			return stateProperty;
		}

		private static MethodDefinition CreateIterator_MakeMethod_SetCurrent(ITypeDefOrRef baseType, GenericParameterSignature parameter) {
			// Get the _current field
			var currentField = baseType.CreateMemberReference("_current", new FieldSignature(parameter))
				.ImportWith(writingModule.DefaultImporter);

			// Create the method
			MethodDefinition setCurrent = new MethodDefinition("SetCurrent",
				MethodAttributes.Family | MethodAttributes.HideBySig,
				MethodSignature.CreateInstance(writingModule.CorLibTypeFactory.Void, parameter));

			setCurrent.Parameters[0].GetOrCreateDefinition().Name = "value";

			// Create the set method body
			CilMethodBody body;
			setCurrent.MethodBody = body = new CilMethodBody(setCurrent);
			body.Instructions.Add(CilOpCodes.Ldarg_0);
			body.Instructions.Add(CilOpCodes.Ldarg_1);
			body.Instructions.Add(CilOpCodes.Stfld, currentField);
			body.Instructions.Add(CilOpCodes.Ret);

			return setCurrent;
		}
	}
}
