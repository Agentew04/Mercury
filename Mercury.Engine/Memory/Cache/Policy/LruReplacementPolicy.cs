namespace Mercury.Engine.Memory.Cache.Policy;

public class LruReplacementPolicy : IReplacementPolicy{
    public int ChooseVictim(int set) {
        throw new NotImplementedException();
    }

    public void Update(int set, int lineIndex) {
        throw new NotImplementedException();
    }
}