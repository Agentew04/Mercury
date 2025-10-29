namespace SAAE.Engine.Memory.Cache;

public class LfuReplacementPolicy : IReplacementPolicy{
    public int ChooseVictim(int set) {
        throw new NotImplementedException();
    }

    public void Update(int set, int lineIndex) {
        throw new NotImplementedException();
    }
}