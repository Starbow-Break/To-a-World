using UnityEngine;

public class InputSequentialSwitcher : SequentialSwitcher<MonoBehaviour>
{
    protected override void ActiveElement(int index)
    {
        var input = _elements[index].GetComponent<IInput>();
        if (input == null)
        {
            Debug.LogError($"element of index {index} has not IInput Component");
            return;
        }
        
        input.StartInput();
    }

    protected override void InactiveElement(int index)
    {
        var input = _elements[index].GetComponent<IInput>();
        if (input == null)
        {
            Debug.LogError($"element of index {index} has not IInput Component");
            return;
        }
        
        input.StopInput();
    }
}
