using UnityEngine;

namespace MINgo.Landing
{
    public sealed class SurfaceTag : MonoBehaviour
    {
        public SurfaceKind kind = SurfaceKind.Unknown;
        public bool allowsShortTakeoff = true;
    }
}
