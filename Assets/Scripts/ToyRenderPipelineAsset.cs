using UnityEngine;
using UnityEngine.Rendering;

namespace ToySRP
{
    [CreateAssetMenu(menuName = "Rendering/ToyRenderPipelineAsset")]
    public class ToyRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] Color someColor;
        public Color SomeColor => someColor;

        // Assetに紐づいたRenderPipelineを返す。
        // このアセットにパラメータをシリアライズして、初期化時に渡すことができそう。
        protected override RenderPipeline CreatePipeline()
        {
            return new ToyRenderPipeline(this);
        }
    }
}
