using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AxibugEmuOnline.Client
{
    public static class UITool
    {
        private static Dictionary<Graphic, Material> _caches = new Dictionary<Graphic, Material>();
        public static Material GetMaterial(this Graphic graphic)
        {
            if (_caches.TryGetValue(graphic, out var material))
            {
                return material;
            }
            else
            {
                var cloneMat = Material.Instantiate(graphic.material);
                _caches[graphic] = cloneMat;
                graphic.material = cloneMat;
                return cloneMat;
            }
        }
        public static void SetMaterial(this Graphic graphic, Material material)
        {
            graphic.material = material;
            _caches.Remove(graphic);
        }

        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            var temp = graphic.color;
            temp.a = alpha;
            graphic.color = temp;
        }
    }
}
