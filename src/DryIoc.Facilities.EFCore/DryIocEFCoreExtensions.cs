﻿using DryIoc.Facilities.EFCore.Errors;

namespace DryIoc.Facilities.EFCore
{
	public static class DryIocEFCoreExtensions
	{
		public static void AssertHasFacility<T>(this IContainer container)
		{
			var type = typeof(T);
			if (!container.IsRegistered(type))
				throw new EFCoreFacilityException(
					$"The EFCoreFacility is dependent on the '{type}' facility. " +
					$"Please add the \"{type.Name}\" facility to container.");
		}

		public static void AddEFCore(this IContainer container)
		{
			var nhibernateFacility = new EFCoreFacility();
			nhibernateFacility.Init(container);
		}

		public static void AddEFCore(this IContainer container, DefaultLifeStyleOption defaultLifeStyle)
		{
			var nhibernateFacility = new EFCoreFacility(defaultLifeStyle);
			nhibernateFacility.Init(container);
		}
	}
}