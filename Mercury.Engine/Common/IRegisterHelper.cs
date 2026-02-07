namespace Mercury.Engine.Common;

/// <summary>
/// Interface comum a classes que fazer mapeamento de valores inteiros e instâncias de <see cref="Enum"/>
/// com os nomes fortemente tipados de uma arquitetura.
/// </summary>
public interface IRegisterHelper
{
    /// <summary>
    /// Returns a registered register based on a type <typeparamref name="TRegister"/> and a number.
    /// </summary>
    /// <param name="number">The number of the register</param>
    /// <typeparam name="TRegister">The type of the register</typeparam>
    /// <returns>The register or null if it does not exist or doesn't have a number</returns>
    public static abstract TRegister? GetRegister<TRegister>(int number) where TRegister : struct, Enum;
    
    /// <inheritdoc cref="GetRegister{TRegister}"/>
    public TRegister? GetRegisterX<TRegister>(int number) where TRegister : struct, Enum;

    public static abstract int GetCount<TRegister>() where TRegister : struct, Enum;
    
    /// <summary>
    /// Returns the number of a generic register.
    /// </summary>
    /// <param name="reg">The generic parameter of the register</param>
    /// <returns>The number of the register if it has one or null if it doesn't</returns>
    public static abstract int? GetRegisterNumber(Enum reg);

    /// <inheritdoc cref="GetRegisterNumber(Enum)"/>
    public int? GetRegisterNumberX(Enum reg);
    
    /// <summary>
    /// Returns the name of a generic register.
    /// </summary>
    /// <param name="reg">The register to get the name from</param>
    /// <returns>The name of the register</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the type of <see cref="reg"/> is not registered</exception>
    public static abstract string GetRegisterName(Enum reg);
    
    /// <inheritdoc cref="GetRegisterName(Enum)"/>
    public string GetRegisterNameX(Enum reg);

    /// <summary>
    /// Returns a register from a name and a type.
    /// </summary>
    /// <param name="name">The name of the register</param>
    /// <param name="type">The type of the register</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <see cref="type"/> is not registered
    /// </exception>
    /// <returns>A generic enum pointing to the register or null if that type does not have any register with the name
    /// provided</returns>
    public static abstract Enum? GetRegisterFromName(string name, Type type);

    /// <inheritdoc cref="GetRegisterFromName"/>
    public Enum? GetRegisterFromNameX(string name, Type type);

    /// <summary>
    /// Returns a register from a number and a type.
    /// </summary>
    /// <param name="number">The number of the register</param>
    /// <param name="type">The generic type of the register</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <see cref="type"/> is not registered</exception>
    /// <returns>Returns the register found or null if the type passed does not have any register with that number</returns>
    public static abstract Enum? GetRegisterFromNumber(int number, Type type);
    
    /// <inheritdoc cref="GetRegisterFromNumber"/>
    public Enum? GetRegisterFromNumberX(int number, Type type);
}