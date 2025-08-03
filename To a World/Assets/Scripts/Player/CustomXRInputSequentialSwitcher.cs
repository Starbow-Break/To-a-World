using UnityEngine;

public class CustomXRInputSequentialSwitcher : SequentialSwitcher<ABaseCustomXRInput>
{
    protected override void ActiveElement(int index)
    {
        var input = _elements[index];
        if (input == null)
        {
            Debug.LogError($"element of index {index} is null");
            return;
        }
        
        input.WakeUp();
    }

    protected override void InactiveElement(int index)
    {
        var input = _elements[index];
        if (input == null)
        {
            Debug.LogError($"element of index {index} is null");
            return;
        }
        
        input.Sleep();
    }
}
