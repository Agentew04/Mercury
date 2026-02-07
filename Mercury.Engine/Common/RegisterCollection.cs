using System.Buffers;
using System.Runtime.CompilerServices;

namespace Mercury.Engine.Common;

/// <summary>
/// A class to represent an architecture agnostic collection of registers.
/// Separates them by type called banks.
/// </summary>
public class RegisterCollection {
    private readonly Dictionary<Type, Array> banks = [];
    private IRegisterHelper provider;

    public RegisterCollection(IRegisterHelper provider)
    {
        this.provider = provider;
    }

    /// <summary>
    /// Creates a new bank.
    /// </summary>
    /// <param name="count">The amount of registers to allocate in this bank.</param>
    /// <typeparam name="TRegister">The type key of this bank</typeparam>
    public void DefineGroup<TRegister>(int count) where TRegister : struct, Enum {
        banks[typeof(TRegister)] = new int[count];
    }

    /// <summary>
    /// Creates a new bank.
    /// </summary>
    /// <typeparam name="TRegister">The type key of this bank. Also defines the amount of registers</typeparam>
    /// <typeparam name="THelper">The <see cref="IRegisterHelper"/> used to get the register amount</typeparam>
    public void DefineGroup<TRegister,THelper>() where TRegister : struct, Enum where THelper : IRegisterHelper {
        DefineGroup<TRegister>(THelper.GetCount<TRegister>());
    }

    /// <summary>
    /// Gets the value of a register.
    /// </summary>
    /// <param name="reg">The register to get the value from</param>
    /// <typeparam name="TRegister">The type of the bank to search</typeparam>
    /// <returns>The value from the register</returns>
    public int Get<TRegister>(TRegister reg) where TRegister : struct, Enum {
        return ((int[])banks[typeof(TRegister)])[Unsafe.As<TRegister,int>(ref reg)];
    }

    public int Get<TRegister>(int number) where TRegister : struct, Enum {
        TRegister? reg = provider.GetRegisterX<TRegister>(number);
        return reg is null ? 0 : Get(reg.Value);
    }

    public int Get(Enum reg, Type type) {
        // return ((int[])banks[type])[Convert.ToInt32(reg)];
        return ((int[])banks[type])[(int)(object)reg];
    }
    

    /// <summary>
    /// Sets the value of a register.
    /// </summary>
    /// <param name="reg">The register that will have its value modified</param>
    /// <param name="value">The value to put inside the register</param>
    /// <typeparam name="TRegister">The type key of the bank</typeparam>
    public void Set<TRegister>(TRegister reg, int value) where TRegister : struct, Enum {
        ((int[])banks[typeof(TRegister)])[Unsafe.As<TRegister,int>(ref reg)] = value;
        dirty.Add((typeof(TRegister), Unsafe.As<TRegister,int>(ref reg)));
    }

    public void Set<TRegister>(int number, int value) where TRegister : struct, Enum {
        TRegister? reg = provider.GetRegisterX<TRegister>(number);
        if (reg is null) {
            return;
        }
        Set(reg.Value,value);
    }

    // public void Set(Enum reg, Type type, int value) {
    //     ((int[])banks[type])[Convert.ToInt32(reg)] = value;
    //     dirty.Add((type, reg));
    // }

    // /// <summary>
    // /// Operator to <see cref="Get(Enum,Type)"/> and <see cref="Set(Enum,Type,int)"/>
    // /// values from registers.
    // /// </summary>
    // /// <param name="reg">The register to read/write.</param>
    // /// <exception cref="KeyNotFoundException">Thrown when the
    // /// type of the Enum passed is not present in any bank.</exception>
    // public int this[Enum reg] {
    //     get => Get(reg, reg.GetType());
    //     set => Set(reg, reg.GetType(), value);
    // }

    private readonly List<ValueTuple<Type, int>> dirty = [];
    private (Type, int)[]? lastArray;
    private readonly ArrayPool<(Type,int)> arrayPool = ArrayPool<(Type,int)>.Shared;

    public ValueTuple<Type, int>[] GetDirty(out int count) {
        if (lastArray is not null) {
            arrayPool.Return(lastArray);
        }
        lastArray = arrayPool.Rent(dirty.Count);
        count = dirty.Count;
        for (int i = 0; i < dirty.Count; i++) {
            lastArray[i] = dirty[i];
        }
        dirty.Clear();
        return lastArray;
    }
}