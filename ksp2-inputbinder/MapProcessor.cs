using UnityEngine.InputSystem;

namespace Codenade.Inputbinder.Processors
{
    public class MapProcessor : InputProcessor<float>
    {
        public override float Process(float value, InputControl control)
        {   
            return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public override string ToString()
        {
            return string.Format("Map(in_min={0},in_max={1},out_min={2},out_max={3})", in_min, in_max, out_min, out_max);
        }

        public float in_min;
        public float in_max;
        public float out_min;
        public float out_max;
    }
}
