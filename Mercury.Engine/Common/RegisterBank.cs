using Mercury.Engine.Mips.Runtime;

namespace Mercury.Engine.Common;

/// <summary>
/// A class to represent an architecture agnostic collection of registers.
/// Separates them by type called banks.
/// </summary>
public class RegisterBank {
    private readonly Dictionary<Type, Array> banks = [];
    private IRegisterHelper provider;

    public RegisterBank(IRegisterHelper provider)
    {
        this.provider = provider;
    }

    /// <summary>
    /// Creates a new bank.
    /// </summary>
    /// <param name="count">The amount of registers to allocate in this bank.</param>
    /// <typeparam name="TRegister">The type key of this bank</typeparam>
    public void DefineBank<TRegister>(int count) where TRegister : struct, Enum {
        banks[typeof(TRegister)] = new int[count];
    }

    /// <summary>
    /// Gets the value of a register.
    /// </summary>
    /// <param name="reg">The register to get the value from</param>
    /// <typeparam name="TRegister">The type of the bank to search</typeparam>
    /// <returns>The value from the register</returns>
    public int Get<TRegister>(TRegister reg) where TRegister : struct, Enum {
        return ((int[])banks[typeof(TRegister)])[Convert.ToInt32(reg)];
    }

    public int Get<TRegister>(int number) where TRegister : struct, Enum {
        TRegister? reg = provider.GetRegisterX<TRegister>(number);
        return reg is null ? 0 : Get(reg.Value);
    }

    public int Get(Enum reg, Type type) {
        return ((int[])banks[type])[Convert.ToInt32(reg)];
    }

    /// <summary>
    /// Sets the value of a register.
    /// </summary>
    /// <param name="reg">The register that will have its value modified</param>
    /// <param name="value">The value to put inside the register</param>
    /// <typeparam name="TRegister">The type key of the bank</typeparam>
    public void Set<TRegister>(TRegister reg, int value) where TRegister : struct, Enum {
        ((int[])banks[typeof(TRegister)])[Convert.ToInt32(reg)] = value;
        dirty.Add((typeof(TRegister), reg));
    }

    public void Set<TRegister>(int number, int value) where TRegister : struct, Enum {
        TRegister? reg = provider.GetRegisterX<TRegister>(number);
        if (reg is null) {
            return;
        }
        Set(reg.Value,value);
    }

    public void Set(Enum reg, Type type, int value) {
        ((int[])banks[type])[Convert.ToInt32(reg)] = value;
        dirty.Add((type, reg));
    }

    /// <summary>
    /// Operator to <see cref="Get{TRegister}"/> and <see cref="Set{TRegister}"/>
    /// values from registers.
    /// </summary>
    /// <param name="reg">The register to read/write.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the
    /// type of the Enum passed is not present in any bank.</exception>
    public int this[Enum reg] {
        get => Get(reg, reg.GetType());
        set => Set(reg, reg.GetType(), value);
    }

    private readonly List<(Type, Enum)> dirty = [];

    public List<(Type, Enum)> GetDirty() {
        List<(Type, Enum)> newList = [..dirty];
        dirty.Clear();
        return newList;
    }
}