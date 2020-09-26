using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interaction;
using Core.InventorySystem;

class cmd_LightOnTorch : IQueryComand
{
    //La animacion.
    Action TriggerAnimation = delegate { };

    IInteractionComponent component;
    Torch equipedTorch;

    public cmd_LightOnTorch(Action triggerAnimation, IInteractionComponent component, Torch equipedTorch)
    {
        TriggerAnimation = triggerAnimation;
        this.component = component;
        this.equipedTorch = equipedTorch;
    }

    public bool completed { get; set; } = false;
    public bool isReady { get; set; } = false;
    public bool cashed => false;

    public void SetUp()
    {
        isReady = true;
    }

    public void Execute()
    {
        component.ExecuteOperation(OperationType.lightOnTorch, new object[] { equipedTorch });
        completed = true;
    }

    public void Cancel()
    {
        completed = false;
    }
}
