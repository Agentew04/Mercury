namespace Mercury.Engine.Memory.Cache.Policy;

internal interface IReplacementPolicy {
    int ChooseVictim(int set);
    void Update(int set, int lineIndex);
}