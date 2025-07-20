using System.Collections.Generic;

public class EffectInfo {
    public float val;
    public int turn;
}

public class EffectObject {

    public EffectType effectType;
    List<EffectInfo> vecEffectInfo = new List<EffectInfo>();

    public void Add(float val, int turn) {
        EffectInfo effectInfo = new EffectInfo();
        effectInfo.val = val;
        effectInfo.turn = turn;
        vecEffectInfo.Add(effectInfo);
    }

    public float GetEffectVal() {
        float val = 0.0f;

        foreach (EffectInfo info in vecEffectInfo) {
            val += info.val;
        }

        return val;
    }

    public void Resolve() {
        for (int i = vecEffectInfo.Count - 1; i >= 0; i--) {
            if (vecEffectInfo[i].turn == Global.TEMP_EFFECT_ONLY_THIS_ROUND) {
                continue;
            }

            vecEffectInfo[i].turn--;
            if (vecEffectInfo[i].turn == 0) {
                vecEffectInfo.RemoveAt(i);
            }
        }
    }

    public bool IsEmpty() {
        return vecEffectInfo.Count == 0;
    }
}
