using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Analytics;
using Map;

public static class DeckHelperFunc {
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

    public static List<UnitObject> GetAllUnitIncludeEmpty(Deck cDeck) {
        return cDeck.GetUnitByPredicate(delegate (UnitObject unit) {
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
        List<UnitObject> listUnit = GetUnitByArchetype(cDeck, eArchetype);
        if (listUnit.Count == count) {
            return listUnit;
        }

        return PickRandomFromList(listUnit, count);
    }

    public static List<UnitObject> PickRandomFromList(List<UnitObject> list, int count) {
        list.Shuffle();

        List<UnitObject> listResult = new List<UnitObject>();
        for (int i = 0; i < count; i++) {
            if (i >= list.Count) {
                break;
            }

            listResult.Add(list[i]);
        }

        return listResult;
    }
}
