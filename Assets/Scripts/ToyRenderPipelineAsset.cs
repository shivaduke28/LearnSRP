using UnityEngine;
using UnityEngine.Rendering;

namespace ToySRP
{
    [CreateAssetMenu(menuName = "Rendering/ToyRenderPipelineAsset")]
    public class ToyRenderPipelineAsset : RenderPipelineAsset
    {
        public RenderTexture cubemap;

        // Assetに紐づいたRenderPipelineを返す。
        // このアセットにパラメータをシリアライズして、初期化時に渡すことができそう。
        protected override RenderPipeline CreatePipeline()
        {
            var sh = FindObjectOfType<CubemapCameraHolder>();
            return new ToyRenderPipeline(this, sh);
        }
    }
}
