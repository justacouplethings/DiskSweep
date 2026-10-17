namespace DiskSweep.Models;

public enum SafetyLevel
{
    Safe,           // Zero risk: auto-regenerated caches, obsolete temp files
    Recommended,    // Normal: user temp files, logs older than a day
    Caution         // User review: Recycle Bin files or user downloads cache
}
