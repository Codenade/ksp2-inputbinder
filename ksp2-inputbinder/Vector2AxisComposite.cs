using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace Codenade.Inputbinder.Composites
{
    [DisplayStringFormat("{x}/{y}")]
    [DisplayName("X/Y")]
    public class Vector2AxisComposite : InputBindingComposite<Vector2>
    {
        public override Vector2 ReadValue(ref InputBindingCompositeContext context) => new Vector2(context.ReadValue<float>(x), context.ReadValue<float>(y));
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context) => ReadValue(ref context).magnitude;

        [InputControl(layout = "Axis")]
        public int x;

        [InputControl(layout = "Axis")]
        public int y;
    }
}
