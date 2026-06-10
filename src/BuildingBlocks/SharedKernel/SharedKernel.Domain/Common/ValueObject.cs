namespace SharedKernel.Domain.Common;

/// <summary>
/// Value Object temel sınıfı.
/// DDD'de Value Object'ler kimlikleri olmayan, sadece özellikleriyle tanımlanan nesnelerdir.
/// Değer eşitliği tüm alanların eşitliğine göre belirlenir.
/// Immutable (değiştirilemez) olmalıdırlar.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Value object'in eşitlik bileşenlerini döndürür.
    /// Bu liste üzerinden eşitlik karşılaştırması yapılır.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// İki value object'in eşit olup olmadığını kontrol eder.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Hash kodunu oluşturur.
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Eşitlik operatörü.
    /// </summary>
    public static bool operator ==(ValueObject left, ValueObject right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Eşitsizlik operatörü.
    /// </summary>
    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !(left == right);
    }
}
