using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;

public static class DeckHelperFunc
{
    public static bool IsDeckEmptyOrDead(Deck cDeck) {
        foreach (UnitObject unit in cDeck) {
            if (unit == null) {
                continue;
            }

            if (unit.IsDead()) {
                continue;
            }

            return false;
        }

        return true;
    }

    public static Deck GetOtherDeck(Deck cDeck) {
        eDeckType eType = cDeck.GetDeckType();
        eDeckType eOtherType = eType == eDeckType.PLAYER ? eDeckType.ENEMY : eDeckType.PLAYER;
        return DeckManager.instance.GetDeckByType(eOtherType);
    }

    public static List<UnitObject> GetUnitByArchetype(Deck cDeck, eUnitArchetype eUnitArchetype) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (unit.IsDead()) {
                return false;
            }

            if (unit.unitSO.eUnitArchetype != eUnitArchetype) {
                return false;
            }

            return true;
        });
    }

    public static List<UnitObject> GetAllDeadUnit(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            return true;
        });
    }

    public static List<UnitObject> GetAllAliveUnit(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (unit.IsDead()) {
                return false;
            }

            return true;
        });
    }

    public static List<UnitObject> GetAllUnitFrontLine(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (!unit.IsFrontPosition()) {
                return false;
            }

            return true;
        });
    }

    public static List<UnitObject> GetAllUnitBelowHpPercent(Deck cDeck, float percent) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (unit.GetHealthPercentage() > percent) {
                return false;
            }

            return true;
        });
    }

    public static List<UnitObject> GetRandomAliveUnit(Deck cDeck, int count, eUnitArchetype eArchetype = eUnitArchetype.NONE) {
        int i = 0;
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
            if (unit == null) {
                return false;
            }

            if (!unit.IsDead()) {
                return false;
            }

            if (eArchetype != eUnitArchetype.NONE && unit.unitSO.eUnitArchetype != eArchetype) {
                return false;
            }

            if (i == count) {
                return false;
            }

            i++;

            return true;
        });
    }
}
