namespace PharmaPOS.Domain.Common.Guards;

public static class Guard
{
	public static void ThrowIfUndefined<TEnum>(TEnum value)
		where TEnum : struct, Enum
	{
		if (!Enum.IsDefined(value))
			throw new ArgumentException(
				$"Invalid {typeof(TEnum).Name} value.",
				nameof(value));
	}
}