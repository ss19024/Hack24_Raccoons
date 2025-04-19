
namespace Rokid.UXR.Components
{
    public interface IShaper
    {
        string ShaperName { get; }
        void OnValidate();
        void RefreshMesh();
    }

}
