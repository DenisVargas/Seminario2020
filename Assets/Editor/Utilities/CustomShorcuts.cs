using UnityEditor;

[InitializeOnLoad]
public class CustomShorcuts : Editor
{
    [MenuItem("Utility/Toogle Inspector Lock")]
    public static void ToogleInspectorLock()
    {
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }
}
