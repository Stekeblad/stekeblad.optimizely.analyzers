using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System;

namespace Stekeblad.Optimizely.Analyzers.Extensions
{
	/// <summary>
	/// Copied from microsoft.codeanalysis.workspaces.common version 4.0.1,
	/// original qualified symbol name: Microsoft.CodeAnalysis.ValueUsageInfo
	/// </summary>
	[Flags]
	public enum ValueUsageInfo
	{
		//
		// Summary:
		//     Represents default value indicating no usage.
		None = 0,
		//
		// Summary:
		//     Represents a value read. For example, reading the value of a local/field/parameter.
		Read = 1,
		//
		// Summary:
		//     Represents a value write. For example, assigning a value to a local/field/parameter.
		Write = 2,
		//
		// Summary:
		//     Represents a reference being taken for the symbol. For example, passing an argument
		//     to an "in", "ref" or "out" parameter.
		Reference = 4,
		//
		// Summary:
		//     Represents a name-only reference that neither reads nor writes the underlying
		//     value. For example, 'nameof(x)' or reference to a symbol 'x' in a documentation
		//     comment does not read or write the underlying value stored in 'x'.
		Name = 8,
		//
		// Summary:
		//     Represents a value read and/or write. For example, an increment or compound assignment
		//     operation.
		ReadWrite = 3,
		//
		// Summary:
		//     Represents a readable reference being taken to the value. For example, passing
		//     an argument to an "in" or "ref readonly" parameter.
		ReadableReference = 5,
		//
		// Summary:
		//     Represents a readable reference being taken to the value. For example, passing
		//     an argument to an "out" parameter.
		WritableReference = 6,
		//
		// Summary:
		//     Represents a value read or write. For example, passing an argument to a "ref"
		//     parameter.
		ReadableWritableReference = 7
	}

	/// <summary>
	/// /// Copied from microsoft.codeanalysis.workspaces.common version 4.0.1,
	/// original qualified symbol name: Microsoft.CodeAnalysis.OperationExtensions
	/// </summary>
	/// <remarks>Not all methods from original file is present below</remarks>
	public static class IOperationExtensions
	{
		public static ValueUsageInfo GetValueUsageInfo(this IOperation operation, ISymbol containingSymbol)
		{
			if (operation is ILocalReferenceOperation localReferenceOperation && localReferenceOperation.IsDeclaration && !localReferenceOperation.IsImplicit)
			{
				return ValueUsageInfo.Write;
			}

			IOperation parent;
			if (operation is IDeclarationPatternOperation)
			{
				while (true)
				{
					parent = operation.Parent;
					if (!(parent is IBinaryPatternOperation) && !(parent is INegatedPatternOperation) && !(parent is IRelationalPatternOperation))
					{
						break;
					}

					operation = operation.Parent;
				}

				parent = operation.Parent;
				if (!(parent is IPatternCaseClauseOperation))
				{
					if (!(parent is IRecursivePatternOperation))
					{
						if (!(parent is ISwitchExpressionArmOperation))
						{
							if (!(parent is IIsPatternOperation))
							{
								if (parent is IPropertySubpatternOperation)
								{
									return ValueUsageInfo.Write;
								}

								return ValueUsageInfo.ReadWrite;
							}

							return ValueUsageInfo.Write;
						}

						return ValueUsageInfo.Write;
					}

					return ValueUsageInfo.Write;
				}

				return ValueUsageInfo.Write;
			}

			if (operation.Parent is IAssignmentOperation assignmentOperation && assignmentOperation.Target == operation)
			{
				if (!operation.Parent.IsAnyCompoundAssignment())
				{
					return ValueUsageInfo.Write;
				}

				return ValueUsageInfo.ReadWrite;
			}

			if (operation.Parent is IIncrementOrDecrementOperation)
			{
				return ValueUsageInfo.ReadWrite;
			}

			if (operation.Parent is IParenthesizedOperation operation2)
			{
				return operation2.GetValueUsageInfo(containingSymbol) & ~ValueUsageInfo.WritableReference;
			}

			parent = operation.Parent;
			if (parent is INameOfOperation || parent is ITypeOfOperation || parent is ISizeOfOperation)
			{
				return ValueUsageInfo.Name;
			}

			if (operation.Parent is IArgumentOperation argumentOperation)
			{
				switch (argumentOperation.Parameter?.RefKind)
				{
					case RefKind.In:
						return ValueUsageInfo.ReadableReference;
					case RefKind.Out:
						return ValueUsageInfo.WritableReference;
					case RefKind.Ref:
						return ValueUsageInfo.ReadableWritableReference;
					default:
						return ValueUsageInfo.Read;
				}
			}

			if (operation.Parent is IReturnOperation operation3)
			{
				switch (operation3.GetRefKind(containingSymbol))
				{
					case RefKind.In:
						return ValueUsageInfo.ReadableReference;
					case RefKind.Ref:
						return ValueUsageInfo.ReadableWritableReference;
					default:
						return ValueUsageInfo.Read;
				}
			}

			if (operation.Parent is IConditionalOperation conditionalOperation)
			{
				if (operation == conditionalOperation.WhenTrue || operation == conditionalOperation.WhenFalse)
				{
					return conditionalOperation.GetValueUsageInfo(containingSymbol);
				}

				return ValueUsageInfo.Read;
			}

			if (operation.Parent is IReDimClauseOperation reDimClauseOperation && reDimClauseOperation.Operand == operation)
			{
				if (!(reDimClauseOperation.Parent is IReDimOperation obj) || !obj.Preserve)
				{
					return ValueUsageInfo.Write;
				}

				return ValueUsageInfo.ReadWrite;
			}

			if (operation.Parent is IDeclarationExpressionOperation operation4)
			{
				return operation4.GetValueUsageInfo(containingSymbol);
			}

			if (operation.IsInLeftOfDeconstructionAssignment(out IDeconstructionAssignmentOperation _))
			{
				return ValueUsageInfo.Write;
			}

			if (operation.Parent is IVariableInitializerOperation variableInitializerOperation && variableInitializerOperation.Parent is IVariableDeclaratorOperation variableDeclaratorOperation)
			{
				switch (variableDeclaratorOperation.Symbol.RefKind)
				{
					case RefKind.Ref:
						return ValueUsageInfo.ReadableWritableReference;
					case RefKind.In:
						return ValueUsageInfo.ReadableReference;
				}
			}

			return ValueUsageInfo.Read;
		}

		public static RefKind GetRefKind(this IReturnOperation operation, ISymbol containingSymbol)
		{
			return (operation.TryGetContainingAnonymousFunctionOrLocalFunction() ?? (containingSymbol as IMethodSymbol))?.RefKind ?? RefKind.None;
		}

		public static IMethodSymbol TryGetContainingAnonymousFunctionOrLocalFunction(this IOperation operation)
		{
			for (operation = operation?.Parent; operation != null; operation = operation.Parent)
			{
				switch (operation.Kind)
				{
					case OperationKind.AnonymousFunction:
						return ((IAnonymousFunctionOperation)operation).Symbol;
					case OperationKind.LocalFunction:
						return ((ILocalFunctionOperation)operation).Symbol;
				}
			}

			return null;
		}

		public static bool IsInLeftOfDeconstructionAssignment(this IOperation operation, out IDeconstructionAssignmentOperation deconstructionAssignment)
		{
			deconstructionAssignment = null;
			IOperation operation2 = operation;
			for (IOperation parent = operation.Parent; parent != null; parent = parent.Parent)
			{
				switch (parent.Kind)
				{
					case OperationKind.DeconstructionAssignment:
						deconstructionAssignment = (IDeconstructionAssignmentOperation)parent;
						return deconstructionAssignment.Target == operation2;
					case OperationKind.Conversion:
					case OperationKind.Parenthesized:
					case OperationKind.Tuple:
						break;
					default:
						return false;
				}

				operation2 = parent;
			}

			return false;
		}

		public static bool IsAnyCompoundAssignment(this IOperation operation)
		{
			if (operation is ICompoundAssignmentOperation || operation is ICoalesceAssignmentOperation)
			{
				return true;
			}

			return false;
		}
	}
}
