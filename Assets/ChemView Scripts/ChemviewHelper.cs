using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChemviewHelper {
    public enum MoleculeSubType { Basic, Drugs, Enantiomers, All}

    public static T ParseEnum<T>(string value)
    {
        return (T)MoleculeSubType.Parse(typeof(T), value, true);
    }
}
