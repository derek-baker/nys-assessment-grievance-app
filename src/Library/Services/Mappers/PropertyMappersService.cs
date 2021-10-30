using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace App.Services.Mappers
{
	public static class PropMapper<TInput, TOutput>
	{
		private static readonly Func<TInput, TOutput> _cloner;
		private static readonly Action<TInput, TOutput> _copier;

		private static readonly IEnumerable<PropertyInfo> _sourceProperties;
		private static readonly IEnumerable<PropertyInfo> _destinationProperties;

		[SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "<Pending>")]
		static PropMapper()
		{
			_destinationProperties = typeof(TOutput)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(prop => prop.CanWrite);
			_sourceProperties = typeof(TInput)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(prop => prop.CanRead);

			_cloner = CreateCloner();
			_copier = CreateCopier();
		}

		[SuppressMessage("Style", "IDE0034:Simplify 'default' expression", Justification = "<Pending>")]
		private static Func<TInput, TOutput> CreateCloner()
		{
			// Check if type has parameterless constructor - just in case
			if (typeof(TOutput).GetConstructor(Type.EmptyTypes) == null) return ((x) => default(TOutput));

			var input = Expression.Parameter(typeof(TInput), "input");

			// For each property that exists in the destination object, is there a property with the same name in the source object?
			var memberBindings = _sourceProperties.Join(_destinationProperties,
				sourceProperty => sourceProperty.Name,
				destinationProperty => destinationProperty.Name,
				(sourceProperty, destinationProperty) =>
					(MemberBinding)Expression.Bind(destinationProperty,
						Expression.Property(input, sourceProperty)));

			var body = Expression.MemberInit(Expression.New(typeof(TOutput)), memberBindings);
			var lambda = Expression.Lambda<Func<TInput, TOutput>>(body, input);
			return lambda.Compile();
		}

		private static Action<TInput, TOutput> CreateCopier()
		{
			var input = Expression.Parameter(typeof(TInput), "input");
			var output = Expression.Parameter(typeof(TOutput), "output");

			// For each property that exists in the destination object, 
			// is there a property with the same name in the source object?
			var memberAssignments = _sourceProperties.Join(
				_destinationProperties,
				sourceProperty => sourceProperty.Name,
				destinationProperty => destinationProperty.Name,
				(sourceProperty, destinationProperty) =>
					Expression.Assign(
						Expression.Property(output, destinationProperty),
						Expression.Property(input, sourceProperty)
					)
			);

			var body = Expression.Block(memberAssignments);
			var lambda = Expression.Lambda<Action<TInput, TOutput>>(body, input, output);
			return lambda.Compile();
		}

		[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
		[SuppressMessage("Style", "IDE0034:Simplify 'default' expression", Justification = "<Pending>")]
		public static TOutput From(TInput input)
		{
			if (input == null) return default(TOutput);
			return _cloner(input);
		}

		[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
		public static void CopyTo(TInput input, TOutput output)
		{
			_copier(input, output);
		}
	}
}