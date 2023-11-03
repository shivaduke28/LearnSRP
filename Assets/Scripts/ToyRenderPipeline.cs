using UnityEngine;
using UnityEngine.Rendering;

namespace ToySRP
{
    public sealed class ToyRenderPipeline : RenderPipeline
    {
        readonly ToyRenderPipelineAsset asset;

        public ToyRenderPipeline(ToyRenderPipelineAsset asset)
        {
            this.asset = asset;
        }

        // 毎フレーム呼ばれる
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                // depthバッファをクリアする。
                if (camera.clearFlags != CameraClearFlags.Nothing)
                {
                    var cmd = new CommandBuffer();
                    cmd.ClearRenderTarget(true, false, default);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Release();
                }

                // カメラのカリングパラメータを取得する。
                if (!camera.TryGetCullingParameters(out var cullingParameters)) continue;
                // カリングを実行。
                var cullingResults = context.Cull(ref cullingParameters);

                // "LightMode"="ToyOpaque"のPassを使用する。
                var shaderTag = new ShaderTagId("ToyOpaque");

                // Cameraからソート設定を作る。自前で用意することもできるようになってそう。
                var sortingSetting = new SortingSettings(camera);
                // この辺よくわかってない。
                var drawingSettings = new DrawingSettings(shaderTag, sortingSetting);
                // この辺よくわかってない2。
                var filteringSettings = FilteringSettings.defaultValue;

                // カメラの行列を GlobalShaderProperty にセットする。
                context.SetupCameraProperties(camera);

                // draw
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

                // opaqueの後にSkyboxをレンダリングする
                if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                {
                    context.DrawSkybox(camera);
                }

#if UNITY_EDITOR
                // scene viewならギズモをレンダリングする。
                if (camera.cameraType == CameraType.SceneView)
                {
                    context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
                }
#endif

                // 実行。
                context.Submit();
            }
        }
    }
}
