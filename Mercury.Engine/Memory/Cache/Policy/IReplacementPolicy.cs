namespace SAAE.Engine.Memory.Cache;

internal interface IReplacementPolicy {
    int ChooseVictim(int set);
    void Update(int set, int lineIndex);
}