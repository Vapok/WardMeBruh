using System;
using System.Collections.Generic;

namespace WardMeBruh.Models;

[Serializable]
public class SavedPlayerDatabase
{
    public List<SavedPlayerEntry> players = new();
}
