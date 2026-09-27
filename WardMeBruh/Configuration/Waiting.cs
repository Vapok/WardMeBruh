using System;

namespace WardMeBruh.Configuration;

public class Waiting
{
    public void ConfigurationComplete(bool configDone)
    {
        if (configDone)
            StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler StatusChanged;
}
